using System;
using System.Collections.Generic;

namespace ShortestPath.Models;

public class AlgorithmResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public double[] Distances { get; set; } = Array.Empty<double>();
    public int[] Previous { get; set; } = Array.Empty<int>();
    public List<int> Path { get; set; } = new();
    public double PathCost { get; set; }
    public int IterationCount { get; set; }
    public int VisitedNodes { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public string AlgorithmName { get; set; } = string.Empty;

    public static AlgorithmResult Error(string message) =>
        new() { Success = false, ErrorMessage = message };
}