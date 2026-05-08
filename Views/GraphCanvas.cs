using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ShortestPath.Models;

namespace ShortestPath.Views;

public class GraphCanvas : Control
{
    public static readonly StyledProperty<Graph?> GraphProperty =
        AvaloniaProperty.Register<GraphCanvas, Graph?>(nameof(Graph));
    public static readonly StyledProperty<AlgorithmResult?> ResultProperty =
        AvaloniaProperty.Register<GraphCanvas, AlgorithmResult?>(nameof(Result));
    public static readonly StyledProperty<int> SourceNodeProperty =
        AvaloniaProperty.Register<GraphCanvas, int>(nameof(SourceNode));
    public static readonly StyledProperty<int> TargetNodeProperty =
        AvaloniaProperty.Register<GraphCanvas, int>(nameof(TargetNode));

    public Graph? Graph { get => GetValue(GraphProperty); set => SetValue(GraphProperty, value); }
    public AlgorithmResult? Result { get => GetValue(ResultProperty); set => SetValue(ResultProperty, value); }
    public int SourceNode { get => GetValue(SourceNodeProperty); set => SetValue(SourceNodeProperty, value); }
    public int TargetNode { get => GetValue(TargetNodeProperty); set => SetValue(TargetNodeProperty, value); }

    static GraphCanvas()
    {
        AffectsRender<GraphCanvas>(GraphProperty, ResultProperty, SourceNodeProperty, TargetNodeProperty);
    }

    public override void Render(DrawingContext ctx)
    {
        base.Render(ctx);
        var graph = Graph;
        if (graph == null) return;

        double w = Bounds.Width, h = Bounds.Height;
        double cx = w / 2, cy = h / 2;
        double radius = Math.Min(w, h) / 2 - 50;
        double nodeRadius = 22;

        var positions = new Point[graph.NodeCount];
        for (int i = 0; i < graph.NodeCount; i++)
        {
            double angle = 2 * Math.PI * i / graph.NodeCount - Math.PI / 2;
            positions[i] = new Point(cx + radius * Math.Cos(angle), cy + radius * Math.Sin(angle));
        }

        var pathEdges = new HashSet<(int, int)>();
        if (Result?.Path != null && Result.Path.Count > 1)
            for (int i = 0; i < Result.Path.Count - 1; i++)
                pathEdges.Add((Result.Path[i], Result.Path[i + 1]));

        var edgePen = new Pen(Brushes.DimGray, 2);
        var pathPen = new Pen(Brushes.DodgerBlue, 4);
        var labelTypeface = new Typeface("Segoe UI");

        for (int i = 0; i < graph.NodeCount; i++)
        {
            for (int j = i + 1; j < graph.NodeCount; j++)
            {
                if (double.IsPositiveInfinity(graph.AdjacencyMatrix[i, j])) continue;
                bool onPath = pathEdges.Contains((i, j)) || pathEdges.Contains((j, i));
                var pen = onPath ? pathPen : edgePen;

                var p1 = positions[i]; var p2 = positions[j];
                var dir = new Vector(p2.X - p1.X, p2.Y - p1.Y);
                double len = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y);
                var unit = new Vector(dir.X / len, dir.Y / len);
                var from = new Point(p1.X + unit.X * nodeRadius, p1.Y + unit.Y * nodeRadius);
                var to = new Point(p2.X - unit.X * nodeRadius, p2.Y - unit.Y * nodeRadius);

                ctx.DrawLine(pen, from, to);

                double mx = (from.X + to.X) / 2, my = (from.Y + to.Y) / 2;
                string wLabel = graph.AdjacencyMatrix[i, j].ToString("0.##");
                var ft = new FormattedText(wLabel, System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, labelTypeface, 11,
                    onPath ? Brushes.DodgerBlue : Brushes.Gray);
                ctx.DrawText(ft, new Point(mx - ft.Width / 2, my - ft.Height / 2));
            }
        }

        for (int i = 0; i < graph.NodeCount; i++)
        {
            var pos = positions[i];
            bool isSource = i == SourceNode, isTarget = i == TargetNode;
            bool onPath = Result?.Path?.Contains(i) == true;

            IBrush nodeBrush = Brushes.White;
            IBrush borderBrush = Brushes.DimGray;
            if (isSource)      { nodeBrush = Brushes.LimeGreen;  borderBrush = Brushes.DarkGreen; }
            else if (isTarget) { nodeBrush = Brushes.OrangeRed;  borderBrush = Brushes.DarkRed; }
            else if (onPath)   { nodeBrush = Brushes.SteelBlue;  borderBrush = Brushes.Navy; }

            ctx.DrawEllipse(nodeBrush, new Pen(borderBrush, 2.5), pos, nodeRadius, nodeRadius);

            var nodeFt = new FormattedText(i.ToString(), System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI", FontStyle.Normal, FontWeight.Bold), 14,
                isSource || isTarget || onPath ? Brushes.White : Brushes.Black);
            ctx.DrawText(nodeFt, new Point(pos.X - nodeFt.Width / 2, pos.Y - nodeFt.Height / 2));

            if (Result?.Distances != null && i < Result.Distances.Length)
            {
                double d = Result.Distances[i];
                string dLabel = double.IsPositiveInfinity(d) ? "inf" : d.ToString("0.##");
                var dFt = new FormattedText($"d={dLabel}", System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, labelTypeface, 10, Brushes.SlateGray);
                ctx.DrawText(dFt, new Point(pos.X - dFt.Width / 2, pos.Y - nodeRadius - 16));
            }
        }
    }
}