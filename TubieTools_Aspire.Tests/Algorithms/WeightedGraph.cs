using System;
using System.Collections.Generic;
using System.Linq;

namespace TubieTools_Aspire.Tests.Algorithms
{
    /// <summary>
    /// Represents a weighted edge in a graph.
    /// Supports both directed and undirected graphs.
    /// </summary>
    public class WeightedEdge : IComparable<WeightedEdge>
    {
        /// <summary>
        /// Source vertex identifier
        /// </summary>
        public int Source { get; set; }

        /// <summary>
        /// Destination vertex identifier
        /// </summary>
        public int Destination { get; set; }

        /// <summary>
        /// Edge weight (distance, cost, time, etc.)
        /// Must be non-negative for Dijkstra's algorithm
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Optional edge metadata (road name, route type, etc.)
        /// Useful for logistics applications
        /// </summary>
        public string Metadata { get; set; }

        public WeightedEdge()
        {
        }

        public WeightedEdge(int source, int destination, double weight, string metadata = null)
        {
            Source = source;
            Destination = destination;
            Weight = weight;
            Metadata = metadata;
        }

        /// <summary>
        /// Compares edges by weight for ordering
        /// </summary>
        public int CompareTo(WeightedEdge other)
        {
            if (other == null)
                return 1;
            return Weight.CompareTo(other.Weight);
        }

        public override string ToString()
        {
            return $"{Source} → {Destination} (Weight: {Weight:F2}" +
                   (string.IsNullOrEmpty(Metadata) ? ")" : $", {Metadata})");
        }
    }

    /// <summary>
    /// Represents a vertex in a graph with optional metadata
    /// </summary>
    public class GraphVertex
    {
        /// <summary>
        /// Unique vertex identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Vertex label or name (e.g., location name)
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Optional geographic coordinates (latitude, longitude)
        /// </summary>
        public (double Latitude, double Longitude)? Coordinates { get; set; }

        /// <summary>
        /// Optional metadata (city, postal code, etc.)
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        public GraphVertex()
        {
            Metadata = new Dictionary<string, object>();
        }

        public GraphVertex(int id, string label = null)
        {
            Id = id;
            Label = label;
            Metadata = new Dictionary<string, object>();
        }

        public override string ToString()
        {
            return $"Vertex {Id}" + (string.IsNullOrEmpty(Label) ? "" : $" ({Label})");
        }
    }

    /// <summary>
    /// Represents a weighted undirected or directed graph
    /// Optimized for use with Dijkstra's algorithm, A*, and other pathfinding algorithms
    /// </summary>
    public class WeightedGraph
    {
        private readonly Dictionary<int, List<WeightedEdge>> _adjacencyList;
        private readonly Dictionary<int, GraphVertex> _vertices;
        private readonly bool _isDirected;

        /// <summary>
        /// Gets whether the graph is directed
        /// </summary>
        public bool IsDirected => _isDirected;

        /// <summary>
        /// Gets the number of vertices in the graph
        /// </summary>
        public int VertexCount => _vertices.Count;

        /// <summary>
        /// Gets the number of edges in the graph
        /// </summary>
        public int EdgeCount
        {
            get
            {
                int count = 0;
                foreach (var edges in _adjacencyList.Values)
                    count += edges.Count;
                return _isDirected ? count : count / 2;
            }
        }

        /// <summary>
        /// Creates a new weighted graph
        /// </summary>
        /// <param name="isDirected">If true, creates a directed graph; otherwise undirected</param>
        public WeightedGraph(bool isDirected = false)
        {
            _isDirected = isDirected;
            _adjacencyList = new Dictionary<int, List<WeightedEdge>>();
            _vertices = new Dictionary<int, GraphVertex>();
        }

        /// <summary>
        /// Adds a vertex to the graph
        /// </summary>
        public void AddVertex(int id, string label = null, (double, double)? coordinates = null)
        {
            if (_vertices.ContainsKey(id))
                throw new ArgumentException($"Vertex {id} already exists");

            var vertex = new GraphVertex(id, label);
            if (coordinates.HasValue)
                vertex.Coordinates = coordinates.Value;

            _vertices[id] = vertex;
            _adjacencyList[id] = new List<WeightedEdge>();
        }

        /// <summary>
        /// Adds a vertex to the graph
        /// </summary>
        public void AddVertex(GraphVertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            if (_vertices.ContainsKey(vertex.Id))
                throw new ArgumentException($"Vertex {vertex.Id} already exists");

            _vertices[vertex.Id] = vertex;
            _adjacencyList[vertex.Id] = new List<WeightedEdge>();
        }

        /// <summary>
        /// Adds an directed edge to the graph
        /// </summary>
        public void AddEdge(int source, int destination, double weight, string metadata = null)
        {
            if (!_vertices.ContainsKey(source))
                throw new ArgumentException($"Source vertex {source} does not exist");

            if (!_vertices.ContainsKey(destination))
                throw new ArgumentException($"Destination vertex {destination} does not exist");

            if (weight < 0)
                throw new ArgumentException("Edge weight must be non-negative for Dijkstra's algorithm");

            var edge = new WeightedEdge(source, destination, weight, metadata);
            _adjacencyList[source].Add(edge);

            // If undirected, add reverse edge as well
            if (!_isDirected)
            {
                var reverseEdge = new WeightedEdge(destination, source, weight, metadata);
                _adjacencyList[destination].Add(reverseEdge);
            }
        }

        /// <summary>
        /// Gets all edges from a given vertex
        /// </summary>
        public IEnumerable<WeightedEdge> GetEdgesFrom(int vertexId)
        {
            if (!_adjacencyList.ContainsKey(vertexId))
                throw new ArgumentException($"Vertex {vertexId} does not exist");

            return _adjacencyList[vertexId];
        }

        /// <summary>
        /// Gets all vertices in the graph
        /// </summary>
        public IEnumerable<GraphVertex> GetVertices()
        {
            return _vertices.Values;
        }

        /// <summary>
        /// Gets a specific vertex
        /// </summary>
        public GraphVertex GetVertex(int vertexId)
        {
            if (!_vertices.ContainsKey(vertexId))
                throw new ArgumentException($"Vertex {vertexId} does not exist");

            return _vertices[vertexId];
        }

        /// <summary>
        /// Checks if vertex exists
        /// </summary>
        public bool ContainsVertex(int vertexId)
        {
            return _vertices.ContainsKey(vertexId);
        }

        /// <summary>
        /// Gets all edges in the graph
        /// </summary>
        public IEnumerable<WeightedEdge> GetAllEdges()
        {
            var edges = new HashSet<string>();
            foreach (var edgeList in _adjacencyList.Values)
            {
                foreach (var edge in edgeList)
                {
                    string key = _isDirected 
                        ? $"{edge.Source}->{edge.Destination}" 
                        : edge.Source < edge.Destination 
                            ? $"{edge.Source}-{edge.Destination}" 
                            : $"{edge.Destination}-{edge.Source}";

                    if (edges.Add(key))
                        yield return edge;
                }
            }
        }

        /// <summary>
        /// Clears all vertices and edges
        /// </summary>
        public void Clear()
        {
            _adjacencyList.Clear();
            _vertices.Clear();
        }
    }
}
