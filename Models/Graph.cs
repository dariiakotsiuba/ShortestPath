using System;
using System.Collections.Generic;

namespace ShortestPath.Models;

public class GraphEdge
{
    public int From { get; set; }
    public int To { get; set; }
    public double Weight { get; set; }
}

public class GraphNode
{
    public int Id { get; set; }
    public string Label => Id.ToString();
    public double X { get; set; }
    public double Y { get; set; }
}

public class Graph
{
    public int NodeCount { get; private set; }
    public double[,] AdjacencyMatrix { get; private set; }
    public List<GraphNode> Nodes { get; private set; }

    public Graph(int nodeCount)
    {
        NodeCount = nodeCount;
        AdjacencyMatrix = new double[nodeCount, nodeCount];
        Nodes = new List<GraphNode>();

        for (int i = 0; i < nodeCount; i++)
            for (int j = 0; j < nodeCount; j++)
                AdjacencyMatrix[i, j] = i == j ? 0 : double.PositiveInfinity;

        for (int i = 0; i < nodeCount; i++)
        {
            double angle = 2 * Math.PI * i / nodeCount - Math.PI / 2;
            Nodes.Add(new GraphNode
            {
                Id = i,
                X = 300 + 200 * Math.Cos(angle),
                Y = 300 + 200 * Math.Sin(angle)
            });
        }
    }

    public List<GraphEdge> GetEdges()
    {
        var edges = new List<GraphEdge>();
        for (int i = 0; i < NodeCount; i++)
            for (int j = 0; j < NodeCount; j++)
                if (i != j && !double.IsPositiveInfinity(AdjacencyMatrix[i, j]))
                    edges.Add(new GraphEdge { From = i, To = j, Weight = AdjacencyMatrix[i, j] });
        return edges;
    }

    public bool HasNegativeEdge()
    {
        for (int i = 0; i < NodeCount; i++)
            for (int j = 0; j < NodeCount; j++)
                if (AdjacencyMatrix[i, j] < 0)
                    return true;
        return false;
    }

    public bool IsEmpty()
    {
        for (int i = 0; i < NodeCount; i++)
            for (int j = 0; j < NodeCount; j++)
                if (i != j && !double.IsPositiveInfinity(AdjacencyMatrix[i, j]))
                    return false;
        return true;
    }
}