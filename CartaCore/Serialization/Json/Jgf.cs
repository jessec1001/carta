using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using QuikGraph;
using QuikGraph.Algorithms;

namespace CartaCore.Serialization.Json
{
    public class Jgf
    {
        [JsonPropertyName("graph")]
        public JgfGraph Graph { get; set; }

        [JsonIgnore]
        public IUndirectedGraph<int, Edge<int>> GraphValue
        {
            get
            {
                UndirectedGraph<int, Edge<int>> graph = new UndirectedGraph<int, Edge<int>>();
                graph.AddVertexRange(Graph.NodeValues);
                graph.AddEdgeRange(Graph.EdgeValues);
                return graph;
            }
        }

        public Jgf() { }
        public Jgf(IUndirectedGraph<int, Edge<int>> graph)
        {
            Graph = new JgfGraph(graph);
        }
    }

    public class JgfGraph
    {
        [JsonPropertyName("directed")]
        public bool Directed { get; set; }

        [JsonPropertyName("nodes")]
        public Dictionary<string, JgfNode> Nodes { get; set; }
        [JsonPropertyName("edges")]
        public List<JgfEdge> Edges { get; set; }

        [JsonIgnore]
        public IEnumerable<int> NodeValues => Nodes.Values.Select(node => node.NodeValue);
        [JsonIgnore]
        public IEnumerable<Edge<int>> EdgeValues => Edges.Select(edge => edge.EdgeValue);

        public JgfGraph() { }
        public JgfGraph(IUndirectedGraph<int, Edge<int>> graph)
        {
            VertexIdentity<int> vi = graph.GetVertexIdentity();
            EdgeIdentity<int, Edge<int>> ei = graph.GetEdgeIdentity();

            Nodes = graph.Vertices.ToDictionary(
                vertex => vi(vertex),
                vertex => new JgfNode(vi, ei, vertex)
            );
            Edges = graph.Edges.Select(
                edge => new JgfEdge(vi, ei, edge)
            ).ToList();

            Directed = false;
        }
    }

    public class JgfNode
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonIgnore]
        public int NodeValue
        {
            get
            {
                int.TryParse(Label, out int result);
                return result;
            }
        }

        public JgfNode() { }
        public JgfNode(VertexIdentity<int> vertexId, EdgeIdentity<int, Edge<int>> edgeId, int vertex)
        {
            Label = vertexId(vertex);
        }
    }

    public class JgfEdge
    {
        [JsonPropertyName("source")]
        public string SourceId { get; set; }
        [JsonPropertyName("target")]
        public string TargetId { get; set; }

        [JsonIgnore]
        public Edge<int> EdgeValue
        {
            get
            {
                int.TryParse(SourceId, out int source);
                int.TryParse(TargetId, out int target);
                return new Edge<int>(source, target);
            }
        }

        public JgfEdge() { }
        public JgfEdge(VertexIdentity<int> vertexId, EdgeIdentity<int, Edge<int>> edgeId, Edge<int> edge)
        {
            SourceId = vertexId(edge.Source);
            TargetId = vertexId(edge.Target);
        }
    }
}