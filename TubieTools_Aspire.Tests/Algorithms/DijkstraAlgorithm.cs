using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TubieTools_Aspire.Tests.Algorithms
{
    /// <summary>
    /// Represents the result of a single shortest path query
    /// </summary>
    public class DijkstraPathResult
    {
        /// <summary>
        /// Source vertex of the path
        /// </summary>
        public int Source { get; set; }

        /// <summary>
        /// Destination vertex of the path
        /// </summary>
        public int Destination { get; set; }

        /// <summary>
        /// Shortest distance from source to destination
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Path as ordered list of vertex IDs
        /// </summary>
        public List<int> Path { get; set; }

        /// <summary>
        /// Whether a path exists from source to destination
        /// </summary>
        public bool PathExists { get; set; }

        /// <summary>
        /// Time taken to compute the path (milliseconds)
        /// </summary>
        public long ComputationTimeMs { get; set; }

        /// <summary>
        /// Number of vertices visited during computation
        /// </summary>
        public int VerticesVisited { get; set; }

        /// <summary>
        /// Metadata about the path (edges traversed, etc.)
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        public DijkstraPathResult()
        {
            Path = new List<int>();
            Metadata = new Dictionary<string, object>();
        }

        public override string ToString()
        {
            if (!PathExists)
                return $"No path from {Source} to {Destination}";

            return $"Path {Source}→{Destination}: Distance={Distance:F2}, " +
                   $"Length={Path.Count}, Time={ComputationTimeMs}ms";
        }
    }

    /// <summary>
    /// Represents metrics for a complete shortest path tree computation
    /// </summary>
    public class DijkstraMetrics
    {
        /// <summary>
        /// Source vertex
        /// </summary>
        public int Source { get; set; }

        /// <summary>
        /// Total time to compute shortest path tree
        /// </summary>
        public long TotalComputationTimeMs { get; set; }

        /// <summary>
        /// Number of vertices processed
        /// </summary>
        public int VerticesProcessed { get; set; }

        /// <summary>
        /// Number of edges examined
        /// </summary>
        public int EdgesExamined { get; set; }

        /// <summary>
        /// Distances to all reachable vertices
        /// </summary>
        public Dictionary<int, double> Distances { get; set; }

        /// <summary>
        /// Previous vertex in shortest path to each destination
        /// </summary>
        public Dictionary<int, int> Previous { get; set; }

        /// <summary>
        /// Whether all vertices were processed
        /// </summary>
        public bool IsComplete { get; set; }

        public DijkstraMetrics()
        {
            Distances = new Dictionary<int, double>();
            Previous = new Dictionary<int, int>();
        }
    }

    /// <summary>
    /// Dijkstra's algorithm implementation for finding shortest paths in weighted graphs.
    /// 
    /// Features:
    /// - Single-source shortest path computation
    /// - Multi-target shortest path queries
    /// - Support for both directed and undirected graphs
    /// - Performance metrics and analysis
    /// - Optimized for logistics and route planning
    /// - Supports geographic coordinates for distance calculations
    /// 
    /// Time Complexity: O((V + E) log V) with binary heap
    /// Space Complexity: O(V)
    /// 
    /// Requirements:
    /// - All edge weights must be non-negative
    /// - Graph must be connected (or at least contain a path between query vertices)
    /// </summary>
    public class DijkstraAlgorithm
    {
        private readonly WeightedGraph _graph;
        private readonly Dictionary<int, DijkstraMetrics> _computedTrees;

        /// <summary>
        /// Creates a new Dijkstra algorithm instance
        /// </summary>
        public DijkstraAlgorithm(WeightedGraph graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _computedTrees = new Dictionary<int, DijkstraMetrics>();
        }

        /// <summary>
        /// Finds the shortest path between two vertices
        /// </summary>
        public DijkstraPathResult FindShortestPath(int source, int destination)
        {
            if (!_graph.ContainsVertex(source))
                throw new ArgumentException($"Source vertex {source} does not exist");

            if (!_graph.ContainsVertex(destination))
                throw new ArgumentException($"Destination vertex {destination} does not exist");

            var stopwatch = Stopwatch.StartNew();

            // If we've already computed the tree from this source, use cached result
            if (_computedTrees.TryGetValue(source, out var cachedMetrics))
            {
                return BuildPathResult(source, destination, cachedMetrics, stopwatch.ElapsedMilliseconds);
            }

            // Compute shortest path tree from source
            var metrics = ComputeShortestPathTree(source);
            _computedTrees[source] = metrics;

            stopwatch.Stop();

            return BuildPathResult(source, destination, metrics, stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Computes shortest paths from source to all reachable vertices
        /// This is the main Dijkstra algorithm implementation
        /// </summary>
        public DijkstraMetrics ComputeShortestPathTree(int source)
        {
            if (!_graph.ContainsVertex(source))
                throw new ArgumentException($"Source vertex {source} does not exist");

            var stopwatch = Stopwatch.StartNew();

            var metrics = new DijkstraMetrics { Source = source };
            var distances = new Dictionary<int, double>();
            var previous = new Dictionary<int, int>();
            var visited = new HashSet<int>();

            // Priority queue: (distance, vertex)
            var pq = new PriorityQueue<(double, int)>();

            // Initialize distances
            foreach (var vertex in _graph.GetVertices())
            {
                distances[vertex.Id] = double.PositiveInfinity;
                previous[vertex.Id] = -1;
            }

            distances[source] = 0;
            pq.Enqueue((0, source), 0);

            int verticesProcessed = 0;
            int edgesExamined = 0;

            // Main Dijkstra loop
            while (pq.Count > 0)
            {
                var (currentDist, currentVertex) = pq.Dequeue();

                // Skip if already visited (relaxation already done)
                if (visited.Contains(currentVertex))
                    continue;

                visited.Add(currentVertex);
                verticesProcessed++;

                // If distance to current vertex is greater than recorded, it's an outdated entry
                if (currentDist > distances[currentVertex])
                    continue;

                // Examine all edges from current vertex
                foreach (var edge in _graph.GetEdgesFrom(currentVertex))
                {
                    edgesExamined++;
                    int nextVertex = edge.Destination;
                    double newDistance = distances[currentVertex] + edge.Weight;

                    // Relaxation: if we found a shorter path, update it
                    if (newDistance < distances[nextVertex])
                    {
                        distances[nextVertex] = newDistance;
                        previous[nextVertex] = currentVertex;
                        pq.Enqueue((newDistance, nextVertex), (int)newDistance);
                    }
                }
            }

            stopwatch.Stop();

            metrics.Distances = distances;
            metrics.Previous = previous;
            metrics.VerticesProcessed = verticesProcessed;
            metrics.EdgesExamined = edgesExamined;
            metrics.TotalComputationTimeMs = stopwatch.ElapsedMilliseconds;
            metrics.IsComplete = visited.Count == _graph.VertexCount;

            return metrics;
        }

        /// <summary>
        /// Builds a DijkstraPathResult from computed metrics
        /// </summary>
        private DijkstraPathResult BuildPathResult(
            int source,
            int destination,
            DijkstraMetrics metrics,
            long queryTimeMs)
        {
            var result = new DijkstraPathResult
            {
                Source = source,
                Destination = destination,
                ComputationTimeMs = queryTimeMs
            };

            if (!metrics.Distances.TryGetValue(destination, out var distance))
            {
                result.PathExists = false;
                result.Distance = double.PositiveInfinity;
                return result;
            }

            if (double.IsInfinity(distance))
            {
                result.PathExists = false;
                result.Distance = double.PositiveInfinity;
                return result;
            }

            result.Distance = distance;
            result.PathExists = true;

            // Reconstruct path from previous array
            result.Path = ReconstructPath(source, destination, metrics.Previous);
            result.VerticesVisited = metrics.VerticesProcessed;
            result.Metadata["EdgesExamined"] = metrics.EdgesExamined;
            result.Metadata["TreeComputationTimeMs"] = metrics.TotalComputationTimeMs;

            return result;
        }

        /// <summary>
        /// Reconstructs the path from previous array
        /// </summary>
        private List<int> ReconstructPath(int source, int destination, Dictionary<int, int> previous)
        {
            var path = new List<int>();
            int current = destination;

            // Build path backwards from destination to source
            while (current != -1)
            {
                path.Add(current);
                if (current == source)
                    break;
                current = previous[current];
            }

            // Reverse to get source → destination order
            path.Reverse();

            return path;
        }

        /// <summary>
        /// Finds shortest paths from source to multiple destinations
        /// </summary>
        public Dictionary<int, DijkstraPathResult> FindShortestPathsToMultipleDestinations(
            int source,
            IEnumerable<int> destinations)
        {
            var results = new Dictionary<int, DijkstraPathResult>();
            var metrics = ComputeShortestPathTree(source);
            _computedTrees[source] = metrics;

            foreach (var destination in destinations)
            {
                results[destination] = BuildPathResult(source, destination, metrics, 0);
            }

            return results;
        }

        /// <summary>
        /// Finds shortest paths from multiple sources to a single destination (reverse Dijkstra)
        /// </summary>
        public Dictionary<int, DijkstraPathResult> FindShortestPathsFromMultipleSources(
            IEnumerable<int> sources,
            int destination)
        {
            var results = new Dictionary<int, DijkstraPathResult>();

            foreach (var source in sources)
            {
                results[source] = FindShortestPath(source, destination);
            }

            return results;
        }

        /// <summary>
        /// Gets all shortest paths up to a certain distance/cost threshold
        /// Useful for finding all destinations reachable within a budget
        /// </summary>
        public Dictionary<int, DijkstraPathResult> FindPathsUpToDistance(int source, double maxDistance)
        {
            var results = new Dictionary<int, DijkstraPathResult>();
            var metrics = ComputeShortestPathTree(source);

            foreach (var vertex in _graph.GetVertices())
            {
                if (metrics.Distances.TryGetValue(vertex.Id, out var distance) && distance <= maxDistance)
                {
                    results[vertex.Id] = BuildPathResult(source, vertex.Id, metrics, 0);
                }
            }

            return results;
        }

        /// <summary>
        /// Computes all-pairs shortest paths (runs Dijkstra from every vertex)
        /// Warning: Expensive for large graphs - O(V * (V + E) log V)
        /// </summary>
        public Dictionary<int, Dictionary<int, DijkstraPathResult>> ComputeAllPairsShortestPaths()
        {
            var allPaths = new Dictionary<int, Dictionary<int, DijkstraPathResult>>();

            foreach (var vertex in _graph.GetVertices())
            {
                var pathsFromVertex = FindShortestPathsToMultipleDestinations(
                    vertex.Id,
                    _graph.GetVertices().Select(v => v.Id)
                );
                allPaths[vertex.Id] = pathsFromVertex;
            }

            return allPaths;
        }

        /// <summary>
        /// Gets the shortest path tree computed from a source vertex
        /// Returns null if not yet computed
        /// </summary>
        public DijkstraMetrics GetComputedTree(int source)
        {
            _computedTrees.TryGetValue(source, out var metrics);
            return metrics;
        }

        /// <summary>
        /// Clears cached shortest path trees
        /// </summary>
        public void ClearCache()
        {
            _computedTrees.Clear();
        }

        /// <summary>
        /// Gets cache statistics
        /// </summary>
        public Dictionary<string, object> GetCacheStatistics()
        {
            return new Dictionary<string, object>
            {
                { "CachedTrees", _computedTrees.Count },
                { "TotalCachedMetrics", _computedTrees.Count },
                { "AverageComputationTime", _computedTrees.Values.Average(m => m.TotalComputationTimeMs) },
                { "TotalVerticesProcessed", _computedTrees.Values.Sum(m => m.VerticesProcessed) }
            };
        }
    }

    /// <summary>
    /// Simple min-heap priority queue implementation
    /// Used by Dijkstra algorithm for efficient min extraction
    /// </summary>
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> _items;

        public int Count => _items.Count;

        public PriorityQueue()
        {
            _items = new List<T>();
        }

        /// <summary>
        /// Adds item to priority queue
        /// Priority is determined by item's CompareTo method
        /// </summary>
        public void Enqueue(T item, int priority = 0)
        {
            _items.Add(item);
            int childIndex = _items.Count - 1;

            while (childIndex > 0)
            {
                int parentIndex = (childIndex - 1) / 2;
                if (_items[childIndex].CompareTo(_items[parentIndex]) >= 0)
                    break;

                // Swap
                var temp = _items[childIndex];
                _items[childIndex] = _items[parentIndex];
                _items[parentIndex] = temp;

                childIndex = parentIndex;
            }
        }

        /// <summary>
        /// Removes and returns the minimum item
        /// </summary>
        public T Dequeue()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("Priority queue is empty");

            T minItem = _items[0];
            int lastIndex = _items.Count - 1;
            _items[0] = _items[lastIndex];
            _items.RemoveAt(lastIndex);

            if (_items.Count == 0)
                return minItem;

            int parentIndex = 0;
            while (true)
            {
                int leftChild = 2 * parentIndex + 1;
                int rightChild = 2 * parentIndex + 2;
                int smallest = parentIndex;

                if (leftChild < _items.Count && _items[leftChild].CompareTo(_items[smallest]) < 0)
                    smallest = leftChild;

                if (rightChild < _items.Count && _items[rightChild].CompareTo(_items[smallest]) < 0)
                    smallest = rightChild;

                if (smallest == parentIndex)
                    break;

                // Swap
                var temp = _items[parentIndex];
                _items[parentIndex] = _items[smallest];
                _items[smallest] = temp;

                parentIndex = smallest;
            }

            return minItem;
        }

        /// <summary>
        /// Peeks at minimum item without removing it
        /// </summary>
        public T Peek()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("Priority queue is empty");
            return _items[0];
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}
