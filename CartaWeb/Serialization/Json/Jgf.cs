using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using QuikGraph;

using CartaCore.Data;
using CartaCore.Utility;

namespace CartaWeb.Serialization.Json
{
    using FreeformGraph = IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge>;

    /// <summary>
    /// Represents a single or multiple freeform graph in JSON Graph format.
    /// </summary>
    /// <remarks>
    /// This class allows for easy serialization and deserialization to and from JSON specified by the JSON Graph
    /// Format.
    /// </remarks>
    public class JgFormat
    {
        /// <summary>
        /// Gets or sets the single graph.
        /// </summary>
        /// <value>
        /// The single graph.
        /// </value>
        [JsonPropertyName("graph")]
        public JgFormatGraph GraphSingle { get; set; }
        /// <summary>
        /// Gets or sets the multiple graphs.
        /// </summary>
        /// <value>
        /// The multiple graphs.
        /// </value>
        [JsonPropertyName("graphs")]
        public List<JgFormatGraph> GraphsMultiple { get; set; }

        /// <summary>
        /// Gets the freeform graph.
        /// </summary>
        /// <value>
        /// The freeform graph.
        /// </value>
        [JsonIgnore]
        public FreeformGraph Graph
        {
            get
            {
                // Check that the appropriate property exists first.
                // Then, return the graph.
                if (GraphSingle is null) return null;
                return GraphSingle.Graph;
            }
        }
        /// <summary>
        /// Gets the freeform graphs.
        /// </summary>
        /// <value>
        /// The freeform graphs.
        /// </value>
        [JsonIgnore]
        public IEnumerable<FreeformGraph> Graphs
        {
            get
            {
                // Check that the appropriate property exists first.
                // Then, return the list of graph.
                if (GraphsMultiple is null) return null;
                return GraphsMultiple.Select(graph => graph.Graph);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormat"/> class with the specified graph.
        /// </summary>
        /// <param name="graph">The graph to convert to a new format.</param>
        public JgFormat(FreeformGraph graph)
        {
            GraphSingle = new JgFormatGraph(graph);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormat"/> class with the specified enumerable of graphs.
        /// </summary>
        /// <param name="graphs">The graphs to convert to a new format.</param>
        public JgFormat(IEnumerable<FreeformGraph> graphs)
        {
            GraphsMultiple = graphs.Select(graph => new JgFormatGraph(graph)).ToList();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormat"/> class.
        /// </summary>
        public JgFormat() { }
    }

    /// <summary>
    /// Represents a freeform graph in JSON Graph format.
    /// </summary>
    public class JgFormatGraph
    {
        /// <summary>
        /// Gets or sets the whether the graph is directed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the graph is directed; otherwise <c>false</c>.
        /// </value>
        [JsonPropertyName("directed")]
        public bool Directed { get; set; }

        /// <summary>
        /// Gets or sets the graph nodes.
        /// </summary>
        /// <value>
        /// The map of identifier-node pairs of graph nodes.
        /// </value>
        [JsonPropertyName("nodes")]
        public Dictionary<string, JgFormatNode> Nodes { get; set; }
        /// <summary>
        /// Gets or sets the graph edges.
        /// </summary>
        /// <value>
        /// The graph edges.
        /// </value>
        [JsonPropertyName("edges")]
        public List<JgFormatEdge> Edges { get; set; }

        /// <summary>
        /// Gets the freeform graph.
        /// </summary>
        /// <value>
        /// The freeform graph.
        /// </value>
        [JsonIgnore]
        public FreeformGraph Graph
        {
            get
            {
                // Create a graph with the correct directed variant.
                FreeformGraph graph;
                if (Directed)
                    graph = new AdjacencyGraph<FreeformVertex, FreeformEdge>();
                else
                    graph = new UndirectedGraph<FreeformVertex, FreeformEdge>();

                // Add the vertices and edges.
                graph.AddVertexRange(Nodes.Select(pair => new FreeformVertex(Guid.Parse(pair.Key))
                {
                    Label = pair.Value.Label,
                    Description = pair.Value.Description,
                    Properties = pair.Value.Properties
                }));
                graph.AddEdgeRange(Edges.Select(edge => edge.Edge));

                return graph;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormatGraph"/> class with the specified graph.
        /// </summary>
        /// <param name="graph">The graph to convert to a new format.</param>
        public JgFormatGraph(FreeformGraph graph)
        {
            Directed = graph.IsDirected;

            Nodes = graph.Vertices.ToDictionary(
                vertex => vertex.Id.ToString(),
                vertex => new JgFormatNode(vertex)
            );
            Edges = graph.Edges.Select(
                edge => new JgFormatEdge(edge)
            ).ToList();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormatGraph"/> class.
        /// </summary>
        public JgFormatGraph() { }
    }

    /// <summary>
    /// Represents a freeform vertex in JSON Graph format.
    /// </summary>
    public class JgFormatNode
    {
        /// <summary>
        /// Gets or sets the vertex label.
        /// </summary>
        /// <value>
        /// The vertex label which is visible on vertex visualization.
        /// </value>
        [JsonPropertyName("label")]
        public string Label { get; set; }
        /// <summary>
        /// Gets or sets the vertex description.
        /// </summary>
        /// <value>
        /// The vertex description which pops up on vertex visualization.
        /// </value>
        [JsonPropertyName("title")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the vertex metadata.
        /// </summary>
        /// <value>
        /// The vertex metadata.
        /// </value>
        [JsonPropertyName("metadata")]
        public SortedList<string, JgProperty> Metadata { get; set; }

        /// <summary>
        /// Gets the freeform properties.
        /// </summary>
        /// <value>
        /// The freeform properties.
        /// </value>
        [JsonIgnore]
        public SortedList<string, FreeformProperty> Properties
        {
            get
            {
                // Check for no metadata before trying to read it.
                if (Metadata == null)
                    return new SortedList<string, FreeformProperty>();

                // Return the metadata parsed into a sorted list.
                return new SortedList<string, FreeformProperty>(
                    Metadata.ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value.Property
                    )
                );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormatNode"/> class with the specified node.
        /// </summary>
        /// <param name="vertex">The vertex to convert to a new format.</param>
        public JgFormatNode(FreeformVertex vertex)
        {
            Label = vertex.Label;
            Description = vertex.Description;

            Metadata = vertex.Properties == null ? null :
            new SortedList<string, JgProperty>(
                vertex.Properties
                    .ToDictionary(
                        pair => pair.Key,
                        pair => new JgProperty(pair.Value)
                    )
            );
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormatNode"/> class.
        /// </summary>
        public JgFormatNode() { }
    }

    /// <summary>
    /// Represents a freeform edge in JSON Graph format.
    /// </summary>
    public class JgFormatEdge
    {
        /// <summary>
        /// Gets or sets the source vertex ID.
        /// </summary>
        /// <value>
        /// The source vertex ID.
        /// </value>
        [JsonPropertyName("source")]
        public Guid Source { get; set; }
        /// <summary>
        /// Gets or sets the target vertex ID.
        /// </summary>
        /// <value>
        /// The target vertex ID.
        /// </value>
        [JsonPropertyName("target")]
        public Guid Target { get; set; }

        /// <summary>
        /// Gets the freeform edge.
        /// </summary>
        /// <value>
        /// The freeform edge.
        /// </value>
        [JsonIgnore]
        public FreeformEdge Edge
        {
            get
            {
                return new FreeformEdge
                (
                    new FreeformVertex(Source),
                    new FreeformVertex(Target)
                );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormatEdge"/> class with the specified edge.
        /// </summary>
        /// <param name="edge">The edge to convert to a new format.</param>
        public JgFormatEdge(FreeformEdge edge)
        {
            Source = edge.Source.Id;
            Target = edge.Target.Id;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="JgFormatEdge"/> class.
        /// </summary>
        public JgFormatEdge() { }
    }

    /// <summary>
    /// Represents a vertex property in JSON Graph format.
    /// </summary>
    public class JgProperty
    {
        /// <summary>
        /// Gets or sets the property type.
        /// </summary>
        /// <value>
        /// The property type.
        /// </value>
        [JsonPropertyName("type")]
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        /// <value>
        /// The property value.
        /// </value>
        [JsonPropertyName("value")]
        public object Value { get; set; }

        /// <summary>
        /// Gets the freeform property.
        /// </summary>
        /// <value>
        /// The freeform property.
        /// </value>
        [JsonIgnore]
        public FreeformProperty Property
        {
            get => new FreeformProperty
            {
                Value = Value,
                Type = Type.TypeDeserialize()
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JgProperty"/> class with the specified property.
        /// </summary>
        /// <param name="property">The property to convert to a new format.</param>
        public JgProperty(FreeformProperty property)
        {
            Value = property.Value;
            Type = TypeExtensions.TypeSerialize(property.Type);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="JgProperty"/> class.
        /// </summary>
        public JgProperty() { }
    }
}