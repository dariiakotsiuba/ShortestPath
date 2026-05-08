using System;
using System.Collections.Generic;
using System.Diagnostics;
using ShortestPath.Models;

namespace ShortestPath.Algorithms;

public class DijkstraAlgorithm : IShortestPathAlgorithm
{
    public string Name => "Dijkstra";
    public string Description => "Greedy algorithm for graphs with non-negative weights. Complexity: O((V+E) log V)";

    public AlgorithmResult FindShortestPath(Graph graph, int source, int target)
    {
        var sw = Stopwatch.StartNew();
        int n = graph.NodeCount;
        var dist = new double[n];
        var prev = new int[n];
        var visited = new bool[n];
        int iterations = 0, visitedCount = 0;

        for (int i = 0; i < n; i++) { dist[i] = double.PositiveInfinity; prev[i] = -1; }
        dist[source] = 0;

        var pq = new SortedSet<(double dist, int node)>(Comparer<(double, int)>.Create((a, b) =>
            a.Item1 != b.Item1 ? a.Item1.CompareTo(b.Item1) : a.Item2.CompareTo(b.Item2)));
        pq.Add((0, source));

        while (pq.Count > 0)
        {
            var (d, u) = pq.Min;
            pq.Remove(pq.Min);
            iterations++;
            if (visited[u]) continue;
            visited[u] = true;
            visitedCount++;
            if (u == target) break;

            for (int v = 0; v < n; v++)
            {
                if (v == u || double.IsPositiveInfinity(graph.AdjacencyMatrix[u, v])) continue;
                double newDist = dist[u] + graph.AdjacencyMatrix[u, v];
                if (newDist < dist[v]) { dist[v] = newDist; prev[v] = u; pq.Add((newDist, v)); }
            }
        }

        sw.Stop();
        var path = ReconstructPath(prev, source, target);
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