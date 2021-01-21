using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using QuikGraph;

using CartaCore.Data;
using CartaCore.Utility;

namespace CartaCore.Serialization.Json.Jgf
{
    using FreeformGraph = IEdgeListAndIncidenceGraph<FreeformVertex, Edge<FreeformVertex>>;

    public class Jgf
    {
        [JsonPropertyName("graph")]
        public JgfGraph JsonGraph { get; set; }
        [JsonIgnore]
        public FreeformGraph Graph
        {
            get
            {
                AdjacencyGraph<FreeformVertex, Edge<FreeformVertex>> graph = new AdjacencyGraph<FreeformVertex, Edge<FreeformVertex>>();
                graph.AddVertexRange(JsonGraph.Nodes);
                graph.AddEdgeRange(JsonGraph.Edges);
                return graph;
            }
        }

        public Jgf() { }
        public Jgf(FreeformGraph graph)
        {
            JsonGraph = new JgfGraph(graph);
        }
    }

    public class JgfGraph
    {
        [JsonPropertyName("directed")]
        public bool JsonDirected { get; set; }

        [JsonPropertyName("nodes")]
        public Dictionary<string, JgfNode> JsonNodes { get; set; }
        [JsonPropertyName("edges")]
        public List<JgfEdge> JsonEdges { get; set; }

        [JsonIgnore]
        public IEnumerable<FreeformVertex> Nodes
        {
            get
            {
                foreach (KeyValuePair<string, JgfNode> pair in JsonNodes)
                {
                    Guid.TryParse(pair.Key, out Guid id);

                    yield return new FreeformVertex
                    {
                        Id = id,
                        Properties = pair.Value.Properties
                    };
                }
            }
        }
        [JsonIgnore]
        public IEnumerable<Edge<FreeformVertex>> Edges
        {
            get => JsonEdges.Select(edge => edge.Edge);
        }

        public JgfGraph() { }
        public JgfGraph(FreeformGraph graph)
        {
            JsonDirected = true;

            JsonNodes = graph.Vertices.ToDictionary(
                vertex => vertex.Id.ToString(),
                vertex => new JgfNode(vertex)
            );
            JsonEdges = graph.Edges.Select(
                edge => new JgfEdge(edge)
            ).ToList();
        }
    }

    public class JgfNode
    {
        [JsonPropertyName("label")]
        public string JsonLabel { get; set; }
        [JsonPropertyName("data")]
        public SortedList<string, object> JsonData { get; set; }
        [JsonPropertyName("metadata")]
        public SortedList<string, string> JsonMetadata { get; set; }

        [JsonIgnore]
        public SortedList<string, FreeformVertexProperty> Properties
        {
            get
            {
                if (JsonData == null)
                    return new SortedList<string, FreeformVertexProperty>();

                return new SortedList<string, FreeformVertexProperty>(
                    JsonData.ToDictionary(
                        pair => pair.Key,
                        pair =>
                        {
                            Type type = null;
                            if (JsonMetadata != null && JsonMetadata.TryGetValue(pair.Key, out string typeString))
                                type = typeString.ToFriendlyType();


                            return new FreeformVertexProperty
                            {
                                Value = pair.Value,
                                Type = type
                            };
                        }
                    )
                );
            }
        }

        public JgfNode() { }
        public JgfNode(FreeformVertex vertex)
        {
            JsonLabel = "";
            JsonData = vertex.Properties == null ? null :
            new SortedList<string, object>(
                vertex.Properties
                    .ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value.Value
                    )
            );
            JsonMetadata = vertex.Properties == null ? null :
            new SortedList<string, string>(
                vertex.Properties
                    .ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value.Type.ToFriendlyString()
                    )
            );
        }
    }

    public class JgfEdge
    {
        [JsonPropertyName("source")]
        public string JsonSourceId { get; set; }
        [JsonPropertyName("target")]
        public string JsonTargetId { get; set; }

        [JsonIgnore]
        public Edge<FreeformVertex> Edge
        {
            get
            {
                Guid.TryParse(JsonSourceId, out Guid source);
                Guid.TryParse(JsonTargetId, out Guid target);
                return new Edge<FreeformVertex>(
                    new FreeformVertex { Id = source },
                    new FreeformVertex { Id = target }
                );
            }
        }

        public JgfEdge() { }
        public JgfEdge(Edge<FreeformVertex> edge)
        {
            JsonSourceId = edge.Source.Id.ToString();
            JsonTargetId = edge.Target.Id.ToString();
        }
    }
}