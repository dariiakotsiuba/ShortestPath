using ShortestPath.Models;

namespace ShortestPath.Algorithms;

public interface IShortestPathAlgorithm
{
    string Name { get; }
    string Description { get; }
    AlgorithmResult FindShortestPath(Graph graph, int source, int target);
}