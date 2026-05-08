using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ShortestPath.Models;

namespace ShortestPath.Algorithms;

public class BellmanFordAlgorithm : IShortestPathAlgorithm
{
    public string Name => "Bellman-Ford";
    public string Description => "Supports negative weights and detects negative cycles. Complexity: O(V·E)";

    public AlgorithmResult FindShortestPath(Graph graph, int source, int target)
    {
        var sw = Stopwatch.StartNew();
        int n = graph.NodeCount;
        var dist = new double[n];
        var prev = new int[n];
        int iterations = 0;

        for (int i = 0; i < n; i++) { dist[i] = double.PositiveInfinity; prev[i] = -1; }
        dist[source] = 0;

        var edges = graph.GetEdges();
        for (int iter = 0; iter < n - 1; iter++)
        {
            bool updated = false;
            foreach (var edge in edges)
            {
                iterations++;
                if (!double.IsPositiveInfinity(dist[edge.From]) &&
                    dist[edge.From] + edge.Weight < dist[edge.To])
                {
                    dist[edge.To] = dist[edge.From] + edge.Weight;
                    prev[edge.To] = edge.From;
                    updated = true;
                }
            }
            if (!updated) break;
        }

        foreach (var edge in edges)
            if (!double.IsPositiveInfinity(dist[edge.From]) &&
                dist[edge.From] + edge.Weight < dist[edge.To])
            {
                sw.Stop();
                return AlgorithmResult.Error("Graph contains a negative cycle — shortest path is undefined.");
            }

        sw.Stop();
        var path = ReconstructPath(prev, source, target);
        int visitedCount = dist.Count(d => !double.IsPositiveInfinity(d));
        return new AlgorithmResult
        {
            Success = true, Distances = dist, Previous = prev, Path = path,
            PathCost = dist[target], IterationCount = iterations,
            VisitedNodes = visitedCount, ExecutionTime = sw.Elapsed, AlgorithmName = Name
        };
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