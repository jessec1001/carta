using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using CartaCore.Graphs;
using CartaCore.Graphs.Components;

namespace CartaWeb.Serialization.Xml
{
    /// <summary>
    /// Represents a graph with metadata in Graph Exchange XML format.
    /// </summary>
    [XmlRoot(ElementName = "gexf", Namespace = "http://www.gexf.net/1.2draft")]
    public class GexFormat
    {
        /// <summary>
        /// The graph data.
        /// </summary>
        [XmlElement(ElementName = "graph")]
        public GexFormatGraph Data { get; set; }

        /// <summary>
        /// Constructs the graph.
        /// </summary>
        [XmlIgnore]
        public MemoryGraph Graph
        {
            get => Data.Graph;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormat"/> class.
        /// </summary>
        public GexFormat() { }

        /// <summary>
        /// Constructs a <see cref="GexFormat"/> version of the specified graph.
        /// </summary>
        /// <param name="graph">The graph to convert to GEX format.</param>
        /// <returns>The GEX formatted graph.</returns>
        public static async Task<GexFormat> CreateAsync(IGraph graph)
        {
            GexFormat gexFormat = new();
            gexFormat.Data = await GexFormatGraph.CreateAsync(graph);
            return gexFormat;
        }
    }

    /// <summary>
    /// Represents whether the graph is temporally static or dynamic in Graph Exchange XML format.
    /// </summary>
    public enum GexFormatMode
    {
        /// <summary>
        /// The graph is static.
        /// </summary>
        [XmlEnum(Name = "static")]
        Static
    }

    /// <summary>
    /// Represents the type of edge in Graph Exchange XML format.
    /// </summary>
    public enum GexFormatEdgeType
    {
        /// <summary>
        /// Edges are directed.
        /// </summary>
        [XmlEnum(Name = "directed")]
        Directed,
        /// <summary>
        /// Edges are undirected.
        /// </summary>
        [XmlEnum(Name = "undirected")]
        Undirected
    }

    /// <summary>
    /// Represents a graph in Graph Exchange XML format.
    /// </summary>
    public class GexFormatGraph
    {
        /// <summary>
        /// The graph nodes.
        /// </summary>
        [XmlArray(ElementName = "nodes")]
        [XmlArrayItem(ElementName = "node")]
        public List<GexFormatNode> Nodes { get; set; }
        /// <summary>
        /// The graph edges.
        /// </summary>
        [XmlArray(ElementName = "edges")]
        [XmlArrayItem(ElementName = "edge")]
        public List<GexFormatEdge> Edges { get; set; }

        /// <summary>
        /// The temporal mode of the graph.
        /// </summary>
        [XmlAttribute(AttributeName = "mode")]
        public GexFormatMode Mode { get; set; }
        /// <summary>
        /// The type of edge in the graph.
        /// </summary>
        [XmlAttribute(AttributeName = "defaultedgetype")]
        public GexFormatEdgeType EdgeType { get; set; }

        /// <summary>
        /// The property definitions.
        /// </summary>
        [XmlElement(ElementName = "attributes")]
        public GexFormatPropertyDefinitionList PropertyDefinitions { get; set; }

        /// <summary>
        /// Constructs the graph.
        /// </summary>
        public MemoryGraph Graph
        {
            get
            {
                // Create a graph.
                MemoryGraph graph = new(null);

                // Get the property mapping.
                Dictionary<int, string> propertyNames = PropertyDefinitions.Definitions
                    .ToDictionary
                    (
                        def => def.Id,
                        def => def.Name
                    );

                // Add the vertices and edges.
                graph.AddVertexRange(Nodes.Select(node =>
                    new Vertex
                    (
                        node.Id,
                        node.Properties.ToDictionary(
                            property => (string)propertyNames[property.Id],
                            property => (IProperty)new Property(property.Value)
                        )
                    )
                    {
                        Label = node.Label,
                        Description = node.Description
                    }));
                graph.AddEdgeRange(Edges.Select(edge => edge.Edge));

                return graph;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatGraph"/> class.
        /// </summary>
        public GexFormatGraph() { }

        /// <summary>
        /// Constructs a <see cref="GexFormatGraph"/> version of the specified graph.
        /// </summary>
        /// <param name="graph">The graph to convert to GEX format.</param>
        /// <returns>The GEX formatted graph.</returns>
        public static async Task<GexFormatGraph> CreateAsync(IGraph graph)
        {
            GexFormatGraph gexFormatGraph = new();

            // Set the nodes and edges.
            if (graph.Components.TryFind(out IEnumerableComponent<IVertex, IEdge> enumerableComponent))
            {
                // We need the property mapping before creating the nodes.
                SortedList<string, IProperty> properties = new();
                await foreach (Vertex vertex in enumerableComponent.GetVertices())
                {
                    foreach (KeyValuePair<string, IProperty> pair in vertex.Properties)
                    {
                        if (!properties.ContainsKey(pair.Key))
                            properties.Add(pair.Key, pair.Value);
                    }
                }
                Dictionary<string, int> propertyIds = new();
                for (int k = 0; k < properties.Count; k++)
                    propertyIds.Add(properties.Keys[k], k);

                // Initialize the nodes and edges.
                gexFormatGraph.Nodes = new List<GexFormatNode>();
                gexFormatGraph.Edges = new List<GexFormatEdge>();

                // Add each of the vertices and edges to the graph.
                await foreach (IVertex vertex in enumerableComponent.GetVertices())
                {
                    gexFormatGraph.Nodes.Add(new GexFormatNode(vertex, propertyIds));
                    foreach (IEdge edge in vertex.Edges)
                        gexFormatGraph.Edges.Add(new GexFormatEdge(edge));
                }

                // Set the graph properties.
                gexFormatGraph.Mode = GexFormatMode.Static;
                gexFormatGraph.EdgeType = GexFormatEdgeType.Directed;
                gexFormatGraph.PropertyDefinitions = new GexFormatPropertyDefinitionList(properties);
            }

            return gexFormatGraph;
        }
    }

    /// <summary>
    /// Represents a vertex in Graph Exchange XML format.
    /// </summary>
    public class GexFormatNode
    {
        /// <summary>
        /// Gets or sets the vertex ID.
        /// </summary>
        /// <value>
        /// The vertex ID.
        /// </value>
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the vertex label.
        /// </summary>
        /// <value>
        /// The vertex label which is visible on vertex visualization.
        /// </value>
        [XmlAttribute(AttributeName = "label")]
        public string Label { get; set; }
        /// <summary>
        /// Gets or sets the vertex description.
        /// </summary>
        /// <value>
        /// The vertex description which pops up on vertex visualization.
        /// </value>
        [XmlAttribute(AttributeName = "title")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the vertex properties.
        /// </summary>
        /// <value>
        /// The vertex properties.
        /// </value>
        [XmlArray(ElementName = "attvalues")]
        [XmlArrayItem(ElementName = "attvalue")]
        public List<GexFormatProperty> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatNode"/> class with the specified vertex and
        /// and properties mapping.
        /// </summary>
        /// <param name="vertex">The vertex to convert to a new format.</param>
        /// <param name="properties">The property mapping.</param>
        public GexFormatNode(IVertex vertex, Dictionary<string, int> properties)
        {
            Id = vertex.Id;
            if (vertex is IElement element)
            {
                Label = element.Label;
                Description = element.Description;
            }
            if (vertex is IProperty property)
            {
                Properties = property.Properties.Select(pair =>
                {
                    return new GexFormatProperty(properties[pair.Key], pair.Value);
                }).ToList();
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatNode"/> class.
        /// </summary>
        public GexFormatNode() { }
    }

    /// <summary>
    /// Represents an edge in Graph Exchange XML format.
    /// </summary>
    public class GexFormatEdge
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>
        /// The edge ID.
        /// </value>
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the source vertex ID.
        /// </summary>
        /// <value>
        /// The source vertex ID.
        /// </value>
        [XmlAttribute(AttributeName = "source")]
        public string Source { get; set; }
        /// <summary>
        /// Gets or sets the target vertex ID.
        /// </summary>
        /// <value>
        /// The target vertex ID.
        /// </value>
        [XmlAttribute(AttributeName = "target")]
        public string Target { get; set; }

        /// <summary>
        /// Constructs the edge.
        /// </summary>
        [XmlIgnore]
        public Edge Edge => new(Source, Target);

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatEdge"/> class with the specified ID and endpoints.
        /// </summary>
        /// <param name="edge">The edge source and target to convert to a new format.</param>
        public GexFormatEdge(IEdge edge)
        {
            Id = edge.Id;
            Source = edge.Source;
            Target = edge.Target;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatEdge"/> class.
        /// </summary>
        public GexFormatEdge() { }
    }

    /// <summary>
    /// Represents the class that an attribute belongs to in Graph Exchange XML format.
    /// </summary>
    public enum GexFormatClassType
    {
        /// <summary>
        /// The attribute belongs to a node.
        /// </summary>
        [XmlEnum(Name = "node")]
        Node,
        /// <summary>
        /// The attribute belongs to an edge.
        /// </summary>
        [XmlEnum(Name = "edge")]
        Edge
    }

    /// <summary>
    /// Represents definitions for properties in the Graph Exchange XML format.
    /// </summary>
    public class GexFormatPropertyDefinitionList
    {
        /// <summary>
        /// Gets or sets the properties class.
        /// </summary>
        /// <value>
        /// The class of the property definitions.
        /// </value>
        [XmlAttribute(AttributeName = "class")]
        public GexFormatClassType Class { get; set; }

        /// <summary>
        /// Gets or sets the property definitions.
        /// </summary>
        /// <value>
        /// The property definitions.
        /// </value>
        [XmlElement(ElementName = "attribute")]
        public List<GexFormatPropertyDefinition> Definitions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatPropertyDefinitionList"/> class with the specified
        /// properties.
        /// </summary>
        /// <param name="properties">The properties to convert to a new format.</param>
        public GexFormatPropertyDefinitionList(SortedList<string, IProperty> properties)
        {
            Class = GexFormatClassType.Node;
            Definitions = properties
                .Select((pair, index) => new GexFormatPropertyDefinition(index, pair.Key))
                .ToList();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatPropertyDefinitionList"/> class.
        /// </summary>
        public GexFormatPropertyDefinitionList() { }
    }

    /// <summary>
    /// Represents the definition for a vertex property in Graph Exchange XML format.
    /// </summary>
    public class GexFormatPropertyDefinition
    {
        /// <summary>
        /// Gets or sets the property ID.
        /// </summary>
        /// <value>
        /// The property ID.
        /// </value>
        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        /// <value>
        /// The property name.
        /// </value>
        [XmlAttribute(AttributeName = "title")]
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the property type.
        /// </summary>
        /// <value>
        /// The property type.
        /// </value>
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatPropertyDefinition"/> class with the specified ID,
        /// name, and property.
        /// </summary>
        /// <param name="id">The property ID.</param>
        /// <param name="name">The property name.</param>
        public GexFormatPropertyDefinition(int id, string name)
        {
            Id = id;

            Name = name;
            Type = "string";
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatPropertyDefinition"/> class.
        /// </summary>
        public GexFormatPropertyDefinition() { }
    }

    /// <summary>
    /// Represents a vertex property in Graph Exchange XML format.
    /// </summary>
    public class GexFormatProperty
    {
        /// <summary>
        /// The property identifier. This must match an ID specified in a <see cref="GexFormatPropertyDefinition" />.
        /// </summary>
        [XmlAttribute(AttributeName = "for")]
        public int Id { get; set; }

        /// <summary>
        /// The property value.
        /// </summary>
        [XmlAttribute(AttributeName = "value")]
        public object Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatProperty"/> class with the specified ID and property.
        /// </summary>
        /// <param name="id">The property ID.</param>
        /// <param name="property">The property to convert to a new format.</param>
        public GexFormatProperty(int id, IProperty property)
        {
            Id = id;
            Value = property.Value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatProperty"/> class.
        /// </summary>
        public GexFormatProperty() { }
    }
}