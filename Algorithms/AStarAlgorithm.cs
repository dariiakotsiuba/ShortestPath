using System;
using System.Collections.Generic;
using System.Diagnostics;
using ShortestPath.Models;

namespace ShortestPath.Algorithms;

public class AStarAlgorithm : IShortestPathAlgorithm
{
    public string Name => "A*";
    public string Description => "Heuristic algorithm extending Dijkstra with an estimated distance to goal. Complexity: O(E log V)";

    public AlgorithmResult FindShortestPath(Graph graph, int source, int target)
    {
        var sw = Stopwatch.StartNew();
        int n = graph.NodeCount;
        var gScore = new double[n];
        var fScore = new double[n];
        var prev = new int[n];
        var closed = new bool[n];
        int iterations = 0, visitedCount = 0;

        for (int i = 0; i < n; i++) { gScore[i] = double.PositiveInfinity; fScore[i] = double.PositiveInfinity; prev[i] = -1; }
        gScore[source] = 0;
        fScore[source] = Heuristic(graph, source, target);

        var open = new SortedSet<(double f, int node)>(Comparer<(double, int)>.Create((a, b) =>
            a.Item1 != b.Item1 ? a.Item1.CompareTo(b.Item1) : a.Item2.CompareTo(b.Item2)));
        open.Add((fScore[source], source));

        while (open.Count > 0)
        {
            var (_, u) = open.Min;
            open.Remove(open.Min);
            iterations++;
            if (u == target) break;
            if (closed[u]) continue;
            closed[u] = true;
            visitedCount++;

            for (int v = 0; v < n; v++)
            {
                if (v == u || double.IsPositiveInfinity(graph.AdjacencyMatrix[u, v]) || closed[v]) continue;
                double tentative = gScore[u] + graph.AdjacencyMatrix[u, v];
                if (tentative < gScore[v])
                {
                    prev[v] = u; gScore[v] = tentative;
                    fScore[v] = tentative + Heuristic(graph, v, target);
                    open.Add((fScore[v], v));
                }
            }
        }

        sw.Stop();
        var path = ReconstructPath(prev, source, target);
        return new AlgorithmResult
        {
            Success = true, Distances = gScore, Previous = prev, Path = path,
            PathCost = gScore[target], IterationCount = iterations,
            VisitedNodes = visitedCount, ExecutionTime = sw.Elapsed, AlgorithmName = Name
        };
    }

    private static double Heuristic(Graph graph, int from, int to)
    {
        var a = graph.Nodes[from]; var b = graph.Nodes[to];
        return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2)) * 0.01;
    }

    private static List<int> ReconstructPath(int[] prev, int source, int target)
    {
        var path = new List<int>();
        int current = target;
        while (current != -1)
        {
            path.Insert(0, current);
            if (current == source) break;
            current = prev[current];
            if (path.Count > prev.Length) return new List<int>();
        }
        if (path.Count == 0 || path[0] != source) return new List<int>();
        return path;
    }
}