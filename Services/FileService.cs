using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ShortestPath.Models;

namespace ShortestPath.Services;

public class FileService
{
    public async Task SaveResultsAsync(string filePath, Graph graph, AlgorithmResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== SHORTEST PATH RESULTS ===");
        sb.AppendLine($"Date/Time: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine($"Algorithm: {result.AlgorithmName}");
        sb.AppendLine();
        sb.AppendLine($"Node count: {graph.NodeCount}");
        sb.AppendLine();

        sb.AppendLine("Adjacency matrix:");
        sb.Append("     ");
        for (int j = 0; j < graph.NodeCount; j++) sb.Append($"[{j}]".PadLeft(8));
        sb.AppendLine();
        for (int i = 0; i < graph.NodeCount; i++)
        {
            sb.Append($"[{i}]  ");
            for (int j = 0; j < graph.NodeCount; j++)
            {
                double val = graph.AdjacencyMatrix[i, j];
                string cell = double.IsPositiveInfinity(val) ? "inf" : val.ToString("0.##");
                sb.Append(cell.PadLeft(8));
            }
            sb.AppendLine();
        }

        sb.AppendLine();
        if (!result.Success)
        {
            sb.AppendLine($"Error: {result.ErrorMessage}");
        }
        else
        {
            sb.AppendLine(result.Path.Count > 0
                ? $"Path: {string.Join(" -> ", result.Path)}\nCost: {result.PathCost:0.##}"
                : "No path found.");
            sb.AppendLine();
            sb.AppendLine("Distances from source:");
            for (int i = 0; i < result.Distances.Length; i++)
            {
                string d = double.IsPositiveInfinity(result.Distances[i]) ? "inf" : result.Distances[i].ToString("0.##");
                sb.AppendLine($"  Node {i}: {d}");
            }
        }

        sb.AppendLine();
        sb.AppendLine($"Iterations: {result.IterationCount}");
        sb.AppendLine($"Nodes visited: {result.VisitedNodes}");
        sb.AppendLine($"Execution time: {result.ExecutionTime.TotalMilliseconds:0.###} ms");

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }
}