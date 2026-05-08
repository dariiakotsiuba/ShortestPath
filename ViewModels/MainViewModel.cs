using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ShortestPath.Algorithms;
using ShortestPath.Models;
using ShortestPath.Services;

namespace ShortestPath.ViewModels;

public class MainViewModel : ReactiveObject
{
    private readonly FileService _fileService = new();

    public ReactiveCommand<Unit, Unit> RunCommand { get; set; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<Unit, Unit> SaveCommand { get; set; } = ReactiveCommand.Create(() => { });

    private int _nodeCount = 5;
    public int NodeCount
    {
        get => _nodeCount;
        set
        {
            if (value < 2 || value > 20) return;
            this.RaiseAndSetIfChanged(ref _nodeCount, value);
            RebuildGraph();
        }
    }

    public ObservableCollection<MatrixRow> MatrixRows { get; } = new();

    public List<IShortestPathAlgorithm> Algorithms { get; } = new()
    {
        new DijkstraAlgorithm(),
        new BellmanFordAlgorithm(),
        new AStarAlgorithm()
    };

    private IShortestPathAlgorithm _selectedAlgorithm;
    public IShortestPathAlgorithm SelectedAlgorithm
    {
        get => _selectedAlgorithm;
        set => this.RaiseAndSetIfChanged(ref _selectedAlgorithm, value);
    }

    private int _sourceNode = 0;
    public int SourceNode
    {
        get => _sourceNode;
        set => this.RaiseAndSetIfChanged(ref _sourceNode, Math.Clamp(value, 0, NodeCount - 1));
    }

    private int _targetNode = 1;
    public int TargetNode
    {
        get => _targetNode;
        set => this.RaiseAndSetIfChanged(ref _targetNode, Math.Clamp(value, 0, NodeCount - 1));
    }

    private AlgorithmResult? _result;
    public AlgorithmResult? Result
    {
        get => _result;
        set => this.RaiseAndSetIfChanged(ref _result, value);
    }

    private string _statusMessage = "Ready.";
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    private bool _hasError;
    public bool HasError
    {
        get => _hasError;
        set => this.RaiseAndSetIfChanged(ref _hasError, value);
    }

    private Graph _graph;
    public Graph CurrentGraph
    {
        get => _graph;
        private set => this.RaiseAndSetIfChanged(ref _graph, value);
    }

    private string _complexityInfo = string.Empty;
    public string ComplexityInfo
    {
        get => _complexityInfo;
        set => this.RaiseAndSetIfChanged(ref _complexityInfo, value);
    }

    public MainViewModel()
    {
        _selectedAlgorithm = Algorithms[0];
        _graph = new Graph(_nodeCount);
        RebuildGraph();
        LoadSampleGraph();
    }

    private void LoadSampleGraph()
    {
        double inf = double.PositiveInfinity;
        var sample = new double[5, 5]
        {
            { 0, 6,   inf, 1,   inf },
            { 6, 0,   5,   2,   2   },
            { inf, 5, 0,   inf, 5   },
            { 1, 2,   inf, 0,   1   },
            { inf, 2, 5,   1,   0   }
        };
        for (int i = 0; i < _nodeCount && i < 5; i++)
            for (int j = 0; j < _nodeCount && j < 5; j++)
                MatrixRows[i].Cells[j].Value = double.IsPositiveInfinity(sample[i, j])
                    ? string.Empty
                    : sample[i, j].ToString(System.Globalization.CultureInfo.InvariantCulture);
        SyncMatrixToGraph();
    }

    private void RebuildGraph()
    {
        MatrixRows.Clear();
        for (int i = 0; i < _nodeCount; i++)
        {
            var row = new MatrixRow(i, _nodeCount);
            foreach (var cell in row.Cells)
                cell.WhenAnyValue(c => c.Value).Subscribe(_ => SyncMatrixToGraph());
            MatrixRows.Add(row);
        }
        _graph = new Graph(_nodeCount);
        CurrentGraph = _graph;
        Result = null;
        StatusMessage = "Graph updated.";
        ComplexityInfo = string.Empty;
    }

    public void SyncMatrixToGraph()
    {
        if (MatrixRows.Count != _nodeCount) return;
        var g = new Graph(_nodeCount);
        for (int i = 0; i < _nodeCount; i++)
            for (int j = 0; j < _nodeCount; j++)
            {
                if (i == j) continue;
                var raw = MatrixRows[i].Cells[j].Value?.Trim();
                if (string.IsNullOrEmpty(raw)) continue;
                if (double.TryParse(raw, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double w))
                    g.AdjacencyMatrix[i, j] = w;
            }
        CurrentGraph = g;
    }

    public void Run()
    {
        HasError = false;
        SyncMatrixToGraph();

        if (CurrentGraph.IsEmpty())
        {
            HasError = true;
            StatusMessage = "⚠ Graph is empty — add at least one edge.";
            Result = null;
            return;
        }
        if (SelectedAlgorithm is DijkstraAlgorithm && CurrentGraph.HasNegativeEdge())
        {
            HasError = true;
            StatusMessage = "⚠ Dijkstra does not support negative weights. Use Bellman-Ford.";
            Result = null;
            return;
        }
        if (SourceNode == TargetNode)
        {
            HasError = true;
            StatusMessage = "⚠ Source and target nodes must be different.";
            Result = null;
            return;
        }

        var res = SelectedAlgorithm.FindShortestPath(CurrentGraph, SourceNode, TargetNode);
        Result = res;

        if (!res.Success)
        {
            HasError = true;
            StatusMessage = $"✗ {res.ErrorMessage}";
        }
        else if (res.Path.Count == 0 || double.IsPositiveInfinity(res.PathCost))
        {
            StatusMessage = $"No path exists between {SourceNode} and {TargetNode}.";
        }
        else
        {
            HasError = false;
            StatusMessage = $"✓ Path: {string.Join(" → ", res.Path)}  |  Cost: {res.PathCost:0.##}";
        }

        ComplexityInfo = $"Algorithm: {res.AlgorithmName}  |  Iterations: {res.IterationCount}  |  " +
                         $"Nodes visited: {res.VisitedNodes}  |  Time: {res.ExecutionTime.TotalMilliseconds:0.###} ms";
    }

    public async Task SaveResultsAsync(string path)
    {
        if (Result == null) { StatusMessage = "⚠ No results to save."; return; }
        await _fileService.SaveResultsAsync(path, CurrentGraph, Result);
        StatusMessage = $"✓ Saved: {path}";
    }
}

public class MatrixRow
{
    public int RowIndex { get; }
    public ObservableCollection<MatrixCell> Cells { get; }
    public MatrixRow(int row, int count)
    {
        RowIndex = row;
        Cells = new ObservableCollection<MatrixCell>();
        for (int j = 0; j < count; j++)
            Cells.Add(new MatrixCell(row, j));
    }
}

public class MatrixCell : ReactiveObject
{
    public int Row { get; }
    public int Col { get; }
    public bool IsDiagonal => Row == Col;
    private string _value = string.Empty;
    public string Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }
    public MatrixCell(int row, int col) { Row = row; Col = col; }
}