using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using CartaCore.Data;

namespace CartaWeb.Serialization.Xml
{
    /// <summary>
    /// Represents a graph with metadata in Graph Exchange XML format.
    /// </summary>
    [XmlRoot(ElementName = "gexf", Namespace = "http://www.gexf.net/1.2draft")]
    public class GexFormat
    {
        /// <summary>
        /// Gets or sets the graph data.
        /// </summary>
        /// <value>
        /// The graph data.
        /// </value>
        [XmlElement(ElementName = "graph")]
        public GexFormatGraph Data { get; set; }

        /// <summary>
        /// Gets the graph.
        /// </summary>
        /// <value>
        /// The graph.
        /// </value>
        [XmlIgnore]
        public SubGraph Graph
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
        public static async Task<GexFormat> CreateAsync(IEntireGraph graph)
        {
            GexFormat gexFormat = new GexFormat();
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
        /// Gets or sets the graph nodes.
        /// </summary>
        /// <value>
        /// The graph nodes.
        /// </value>
        [XmlArray(ElementName = "nodes")]
        [XmlArrayItem(ElementName = "node")]
        public List<GexFormatNode> Nodes { get; set; }
        /// <summary>
        /// Gets or sets the graph edges.
        /// </summary>
        /// <value>
        /// The graph edges.
        /// </value>
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
        /// Gets or sets the property definitions.
        /// </summary>
        /// <value>
        /// The property definitions.
        /// </value>
        [XmlElement(ElementName = "attributes")]
        public GexFormatPropertyDefinitionList PropertyDefinitions { get; set; }

        /// <summary>
        /// Gets the graph.
        /// </summary>
        /// <value>
        /// The graph.
        /// </value>
        public SubGraph Graph
        {
            get
            {
                // Create a graph.
                SubGraph graph = new SubGraph(null, EdgeType == GexFormatEdgeType.Directed);

                // Get the property mapping.
                Dictionary<int, (string Name, Property Property)> properties = PropertyDefinitions.Definitions
                    .ToDictionary
                    (
                        def => def.Id,
                        def => (def.Name,
                        new Property(Identity.Create(def.Name)))
                    );

                // Add the vertices and edges.
                graph.AddVertexRange(Nodes.Select(node =>
                    new Vertex
                    (
                        Identity.Create(node.Id),
                        node.Properties.Select
                        (
                            property => new Property
                            (
                                Identity.Create(property.Id),
                                property.Observations
                            )
                        ).ToList()
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
        public static async Task<GexFormatGraph> CreateAsync(IEntireGraph graph)
        {
            GexFormatGraph gexFormatGraph = new GexFormatGraph();

            // We need the property mapping before creating the nodes.
            Dictionary<string, (int index, Property prop)> properties =
                new Dictionary<string, (int index, Property prop)>();
            await foreach (Vertex vertex in graph.GetVertices())
            {
                foreach (Property property in vertex.Properties)
                {
                    if (!properties.ContainsKey(property.Identifier.ToString()))
                    {
                        properties.Add
                        (
                            property.Identifier.ToString(),
                            (
                                properties.Count,
                                property
                            )
                        );
                    }
                }
            }
            Dictionary<string, int> propertyIds = properties.ToDictionary
            (
                pair => pair.Key,
                pair => pair.Value.index
            );

            // Set the nodes and edges.
            gexFormatGraph.Nodes = await graph.GetVertices()
                .Select(vertex => new GexFormatNode(vertex, propertyIds))
                .ToListAsync();
            gexFormatGraph.Edges = await graph.GetEdges()
                .Select(edge => new GexFormatEdge(edge))
                .ToListAsync();

            // Set the graph properties.
            gexFormatGraph.Mode = GexFormatMode.Static;
            gexFormatGraph.EdgeType = graph.IsDirected() ? GexFormatEdgeType.Directed : GexFormatEdgeType.Undirected;
            gexFormatGraph.PropertyDefinitions = new GexFormatPropertyDefinitionList(properties);

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
            Id = vertex.Identifier.ToString();

            Label = vertex.Label;
            Description = vertex.Description;

            Properties = vertex.Properties.Select(property =>
            {
                return new GexFormatProperty(properties[property.Identifier.ToString()], property);
            }).ToList();
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
        /// Gets the edge.
        /// </summary>
        /// <value>
        /// The edge.
        /// </value>
        [XmlIgnore]
        public Edge Edge
        {
            get
            {
                return new Edge
                (
                    Identity.Create(Source),
                    Identity.Create(Target)
                );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatEdge"/> class with the specified ID and endpoints.
        /// </summary>
        /// <param name="edge">The edge source and target to convert to a new format.</param>
        public GexFormatEdge(Edge edge)
        {
            Id = edge.Identifier.ToString();
            Source = edge.Source.ToString();
            Target = edge.Target.ToString();
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
        public GexFormatPropertyDefinitionList(Dictionary<string, (int index, Property prop)> properties)
        {
            Class = GexFormatClassType.Node;
            Definitions = properties.Select
            (
                pair => new GexFormatPropertyDefinition
                (
                    pair.Value.index,
                    pair.Key,
                    pair.Value.prop
                )
            ).ToList();
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
        /// <param name="property">The property to convert to a new format.</param>
        public GexFormatPropertyDefinition(int id, string name, Property property)
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
        /// Gets or sets the property ID.
        /// </summary>
        /// <value>
        /// The property ID. This must match an ID specified in a <see cref="GexFormatPropertyDefinition" />.
        /// </value>
        [XmlAttribute(AttributeName = "for")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the property observations.
        /// </summary>
        /// <value>The property observations.</value>
        [XmlArray(ElementName = "values")]
        [XmlArrayItem(ElementName = "value")]
        public List<GexFormatObservation> Values { get; set; }

        /// <summary>
        /// Gets the property observations.
        /// </summary>
        /// <value>The property observations.</value>
        [XmlIgnore]
        public IEnumerable<object> Observations
        {
            get
            {
                return Values.Select(observation => observation.Observation);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatProperty"/> class with the specified ID and property.
        /// </summary>
        /// <param name="id">The property ID.</param>
        /// <param name="property">The property to convert to a new format.</param>
        public GexFormatProperty(int id, Property property)
        {
            Id = id;
            Values = property.Values
                .Select(observation => new GexFormatObservation(observation))
                .ToList();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatProperty"/> class.
        /// </summary>
        public GexFormatProperty() { }
    }

    /// <summary>
    /// Represents a property observation in Graph Exchange XML format.
    /// </summary>
    public class GexFormatObservation
    {
        /// <summary>
        /// Gets or sets the observation value.
        /// </summary>
        /// <value>
        /// The observation value.
        /// </value>
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets the observation.
        /// </summary>
        /// <value>The observation.</value>
        [XmlIgnore]
        public string Observation
        {
            get
            {
                return Value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatObservation"/> class with the specified observation.
        /// </summary>
        /// <param name="value">The observation of a property.</param>
        public GexFormatObservation(object value)
        {
            Value = value.ToString();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GexFormatObservation"/> class.
        /// </summary>
        public GexFormatObservation() { }
    }
}