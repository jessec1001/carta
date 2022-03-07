using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Graphs.Components;
using CartaCore.Serialization.Json;

namespace CartaWeb.Serialization.Json
{
    /// <summary>
    /// Represents a graph in Vis format.
    /// </summary>
    /// <remarks>
    /// This class allows for easy serialization and deserialization to and from JSON. This format allows the
    /// serialized data to immediately be used by Vis.js.
    /// </remarks>
    public class VisFormat
    {
        /// <summary>
        /// The graph identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// The attributes of the graph.
        /// </summary>
        [JsonPropertyName("attributes")]
        public VisFormatAttributes Attributes { get; set; }

        /// <summary>
        /// The nodes contained in the graph.
        /// </summary>
        [JsonPropertyName("nodes")]
        public HashSet<VisFormatNode> Nodes { get; set; }
        /// <summary>
        /// The edges contained in the graph.
        /// </summary>
        [JsonPropertyName("edges")]
        public HashSet<VisFormatEdge> Edges { get; set; }

        /// <summary>
        /// The generated graph structure.
        /// </summary>
        [JsonIgnore]
        public MemoryGraph Graph
        {
            get
            {
                // Create a graph and add the vertices and edges.
                MemoryGraph graph = new(Id);
                graph.AddVertexRange(Nodes.Select(node => node.Vertex));
                graph.AddEdgeRange(Edges.Select(edge => edge.Edge));

                return graph;
            }
        }

        /// <summary>
        /// Constructs a <see cref="VisFormat"/> version of the specified graph.
        /// </summary>
        /// <param name="graph">The graph to convert to Vis format.</param>
        /// <returns>The Vis formatted graph.</returns>
        public static async Task<VisFormat> CreateAsync(IGraph graph)
        {
            VisFormat visFormat = new();

            // Read the graph attributes.
            if (graph is IIdentifiable identifiable) visFormat.Id = identifiable.Id;
            visFormat.Attributes = new VisFormatAttributes(graph.Attributes);

            // Read the content of the graph.
            if (graph.Components.TryFind(out IEnumerableComponent<Vertex, Edge> enumerableComponent))
            {
                // Initialize the nodes and edges.
                visFormat.Nodes = new HashSet<VisFormatNode>();
                visFormat.Edges = new HashSet<VisFormatEdge>();

                // Add each of the vertices and edges to the graph.
                await foreach (Vertex vertex in enumerableComponent.GetVertices())
                {
                    visFormat.Nodes.Add(new VisFormatNode(vertex));
                    foreach (Edge edge in vertex.Edges)
                        visFormat.Edges.Add(new VisFormatEdge(edge));
                }
            }

            return visFormat;
        }
    }

    /// <summary>
    /// Represents the attributes of a graph in Vis format.
    /// </summary>
    public class VisFormatAttributes
    {
        /// <summary>
        /// Whether the graph is dynamic.
        /// </summary>
        [JsonPropertyName("dynamic")]
        public bool Dynamic { get; set; }
        /// <summary>
        /// Whether the graph is finite.
        /// </summary>
        [JsonPropertyName("finite")]
        public bool Finite { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatAttributes"/> class.
        /// </summary>
        public VisFormatAttributes() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatAttributes"/> class.
        /// </summary>
        /// <param name="attributes">The graph attributes.</param>
        public VisFormatAttributes(GraphAttributes attributes)
        {
            Dynamic = attributes.Dynamic;
            Finite = attributes.Finite;
        }
    }

    /// <summary>
    /// Represents a vertex property in Vis format.
    /// </summary>
    public class VisFormatProperty
    {
        /// <summary>
        /// The value of the property.
        /// </summary>
        [JsonPropertyName("value")]
        [JsonConverter(typeof(JsonObjectConverter))]
        public object Value { get; set; }
        /// <summary>
        /// The subproperties of the property.
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, VisFormatProperty> Properties { get; set; }

        /// <summary>
        /// Constructs the property.
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        [JsonIgnore]
        public Property Property
        {
            get
            {
                return new Property
                (
                    Identity.Create(Id),
                    Value
                )
                {
                    Subproperties = Subproperties?.Select(subproperty => subproperty.Property)
                };
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatProperty"/> class with the specified key and value.
        /// </summary>
        /// <param name="property">The property.</param>
        public VisFormatProperty(Property property)
        {
            Id = property.Identifier.ToString();
            Value = property.Value;
            Subproperties = property.Subproperties?.Select(subproperty => new VisFormatProperty(subproperty)).ToList();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatProperty"/> class.
        /// </summary>
        public VisFormatProperty() { }
    }

    // TODO: Needs to reimplement equality and hashing. 
    /// <summary>
    /// Represents a node in Vis format.
    /// </summary>
    public class VisFormatNode : VisFormatProperty
    {
        /// <summary>
        /// The node identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// The node label which is visible on vertex visualization.
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; set; }
        /// <summary>
        /// The vertex description which pops up on vertex visualization.
        /// </summary>
        [JsonPropertyName("title")]
        public string Description { get; set; }

        /// <summary>
        /// The vertex properties.
        /// </summary>
        /// <value>
        /// The vertex properties.
        /// </value>
        [JsonPropertyName("properties")]
        public Dictionary<string, VisFormatProperty> Properties { get; set; }

        /// <summary>
        /// Constructs the vertex.
        /// </summary>
        [JsonIgnore]
        public Vertex Vertex => new(
            Id,
            Properties?.ToDictionary(
                (KeyValuePair<string, VisFormatProperty> entry) => entry.Key,
                (KeyValuePair<string, VisFormatProperty> entry) => entry.Value.Property
            ) ?? new Dictionary<string, Property>()
        )
        {
            Label = Label,
            Description = Description
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatNode"/> class.
        /// </summary>
        public VisFormatNode() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatNode"/> class with the specified node.
        /// </summary>
        /// <param name="vertex">The vertex to convert to a new format.</param>
        public VisFormatNode(Vertex vertex)
        {
            Id = vertex.Id;
            Label = vertex.Label;
            Description = vertex.Description;
            Properties = vertex.Properties?.Select(property => new VisFormatProperty(property)).ToList();
        }
    }

    // TODO: Needs to reimplement equality and hashing. 
    /// <summary>
    /// Represents an edge in Vis format.
    /// </summary>
    public class VisFormatEdge : VisFormatProperty
    {
        /// <summary>
        /// The edge identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// The source vertex identifier.
        /// </summary>
        [JsonPropertyName("from")]
        public string Source { get; set; }
        /// <summary>
        /// The target vertex identifier.
        /// </summary>
        [JsonPropertyName("to")]
        public string Target { get; set; }
        /// <summary>
        /// Whether the edge is directed or undirected.
        /// </summary>
        [JsonPropertyName("directed")]
        public bool Directed { get; set; }

        // TODO: Add properties here.
        /// <summary>
        /// Constructs the edge.
        /// </summary>
        [JsonIgnore]
        public Edge Edge => new(Source, Target) { Directed = Directed };

        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatEdge"/> class with the specified ID and endpoints.
        /// </summary>
        /// <param name="edge">The edge source and target to convert to a new format.</param>
        public VisFormatEdge(Edge edge)
        {
            Id = edge.Id;
            Source = edge.Source;
            Target = edge.Target;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="VisFormatEdge"/> class.
        /// </summary>
        public VisFormatEdge() { }
    }
}