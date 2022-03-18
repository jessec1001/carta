using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Graphs.Components;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Visualization
{
    // TODO: This operation will need to access previously pipelined results in order to update coloring schemes.

    /// <summary>
    /// The strategy to use for coloring the graph.
    /// </summary>
    public enum GraphColorStrategy
    {
        /// <summary>
        /// Color based on hierarchical level and argument.
        /// </summary>
        [EnumMember(Value = "Hierarchical")]
        Hierarchical,
        /// <summary>
        /// Color based on unique identifiers.
        /// </summary>
        [EnumMember(Value = "Random")]
        Random,
        /// <summary>
        /// Color based on number of connected nodes.
        /// </summary>
        [EnumMember(Value = "Connectedness")]
        Connectedness
    }

    // TODO: Make this consistent with VisJS format if possible.
    /// <summary>
    /// Represents a vertex in a graph visualization.
    /// </summary>
    public struct GraphPlotVertex : IVertex<GraphPlotEdge>
    {
        /// <summary>
        /// The unique identifier of the vertex.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The label of the vertex.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The properties of the vertex.
        /// </summary>
        public IDictionary<string, IProperty> Properties { get; set; }

        /// <summary>
        /// The value of the vertex as calculated by the coloring strategy. 
        /// </summary>
        public double? Value { get; set; }

        /// <summary>
        /// The style of the vertex.
        /// </summary>
        public PlotStyle? Style { get; set; }

        /// <inheritdoc />
        public IEnumerable<GraphPlotEdge> Edges { get; set; }
    }
    // TODO: Make this consistent with VisJS format if possible.
    /// <summary>
    /// Represents an edge in a graph visualization.
    /// </summary>
    public struct GraphPlotEdge : IEdge
    {
        /// <summary>
        /// The unique identifier of the edge.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The unique identifier for the source vertex.
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// The unique identifier for the target vertex.
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// Whether the edge is directed or not.
        /// </summary>
        public bool Directed { get; set; }

        /// <summary>
        /// The style of the edge.
        /// </summary>
        public PlotStyle? Style { get; set; }

    }
    /// <summary>
    /// The data for a graph visualization.
    /// </summary>
    public class GraphPlot : Plot, IEnumerableComponent<GraphPlotVertex, GraphPlotEdge>
    {
        /// <inheritdoc />
        public override string Type => "graph";

        /// <inheritdoc />
        public ComponentStack Components { get; set; }

        // TODO: Port colormap code to backend.
        /// <summary>
        /// The colormap to use for mapping numeric data to colors. If not specified, a default color will be used
        /// instead.
        /// </summary>
        public string Colormap { get; init; }

        /// <summary>
        /// The graph to visualize.
        /// </summary>
        private readonly IEnumerableComponent<Vertex, Edge> Graph;
        /// <summary>
        /// The style for vertices.
        /// </summary>
        private readonly PlotStyle? VertexStyle;
        /// <summary>
        /// The style for edges.
        /// </summary>
        private readonly PlotStyle? EdgeStyle;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphPlot"/> class.
        /// </summary>
        /// <param name="graph">The enumerable graph.</param>
        /// <param name="vertexStyle">The default vertex style.</param>
        /// <param name="edgeStyle">The default edge style.</param>
        public GraphPlot(IEnumerableComponent<Vertex, Edge> graph, PlotStyle? vertexStyle, PlotStyle? edgeStyle)
        {
            Graph = graph;
            VertexStyle = vertexStyle;
            EdgeStyle = edgeStyle;
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<GraphPlotVertex> GetVertices()
        {
            // Generate the data for the graph plot.
            await foreach (Vertex vertex in Graph.GetVertices())
            {
                // Create the data for the edges.
                List<GraphPlotEdge> edges = new();
                foreach (Edge edge in vertex.Edges)
                {
                    GraphPlotEdge graphPlotEdge = new()
                    {
                        Id = edge.Id,
                        Source = edge.Source.ToString(),
                        Target = edge.Target.ToString(),
                        Directed = edge.Directed,
                        Style = EdgeStyle
                    };
                    edges.Add(graphPlotEdge);
                }

                // Create the data for the vertex.
                GraphPlotVertex graphPlotVertex = new()
                {
                    Id = vertex.Id,
                    Label = vertex.Label,
                    Properties = vertex.Properties,
                    Style = VertexStyle
                };
                yield return graphPlotVertex;
            }
        }
    }

    /// <summary>
    /// The input for the <see cref="PlotGraphOperation" /> operation.
    /// </summary>
    public struct PlotGraphOperationIn
    {
        /// <summary>
        /// The title of the graph visualization.
        /// </summary>
        [FieldName("Title")]
        public string Title { get; set; }

        /// <summary>
        /// The graph to visualize.
        /// </summary>
        [FieldName("Graph")]
        public Graph Graph { get; set; }

        // TODO: Reimplement.
        // /// <summary>
        // /// The strategy to use for coloring the graph.
        // /// </summary>
        // public GraphColorStrategy ColorStrategy { get; set; }
        // /// <summary>
        // /// The color map to use for mapping numeric data to colors. If not specified, a default color will be used
        // /// instead.
        // /// </summary>
        // public string ColorMap { get; set; }

        /// <summary>
        /// An optional style to apply to vertices.
        /// </summary>
        [FieldName("Vertex Style")]
        public PlotStyle? VertexStyle { get; set; }
        /// <summary>
        /// An optional style to apply to edges.
        /// </summary>
        [FieldName("Edge Style")]
        public PlotStyle? EdgeStyle { get; set; }
        /// <summary>
        /// An optional style to apply to the axes.
        /// </summary>
        [FieldName("Axes Style")]
        public PlotStyle? AxesStyle { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="PlotGraphOperation" /> operation.
    /// </summary>
    public struct PlotGraphOperationOut
    {
        /// <summary>
        /// The generated plot.
        /// </summary>
        [FieldName("Plot")]
        public GraphPlot Plot { get; set; }
    }

    /// <summary>
    /// Visualizes a graph plot of a network graph.
    /// </summary>
    [OperationName(Display = "Graph Plot", Type = "visualizeGraphPlot")]
    [OperationTag(OperationTags.Visualization)]
    public class PlotGraphOperation : TypedOperation
    <
        PlotGraphOperationIn,
        PlotGraphOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<PlotGraphOperationOut> Perform(
            PlotGraphOperationIn input,
            OperationJob job)
        {
            // Check if the graph is enumerable.
            if (!input.Graph.Components.TryFind(out IEnumerableComponent<Vertex, Edge> enumerable))
                throw new ArgumentException("The graph must be a finite graph.");

            // Interpret the axes styles if specified.
            // Notice that we ignore the stroke width because it does not have meaning for the axes yet.
            Dictionary<string, string> styles = new();
            if (input.AxesStyle is not null)
            {
                PlotStyle axesStyle = input.AxesStyle.Value;
                if (axesStyle.FillColor is not null) styles["background"] = axesStyle.FillColor;
                if (axesStyle.StrokeColor is not null) styles["color"] = axesStyle.StrokeColor;
            }

            // Create a new graph plot structure.
            GraphPlot graphPlot = null;
            graphPlot = new(enumerable, input.VertexStyle, input.EdgeStyle)
            {
                Style = styles,
            };
            return Task.FromResult(new PlotGraphOperationOut() { Plot = graphPlot });
        }
    }
}