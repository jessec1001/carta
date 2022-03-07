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
    public struct GraphPlotVertex
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
        /// The value of the vertex as calculated by the coloring strategy. 
        /// </summary>
        public double? Value { get; set; }

        /// <summary>
        /// The style of the vertex.
        /// </summary>
        public PlotStyle? Style { get; set; }
    }
    // TODO: Make this consistent with VisJS format if possible.
    /// <summary>
    /// Represents an edge in a graph visualization.
    /// </summary>
    public struct GraphPlotEdge
    {
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
    public class GraphPlot : Plot
    {
        /// <inheritdoc />
        public override string Type => "graph";

        /// <summary>
        /// The vertices to visualize.
        /// </summary>
        public GraphPlotVertex[] Vertices { get; set; }
        /// <summary>
        /// The edges to visualize.
        /// </summary>
        public GraphPlotEdge[] Edges { get; set; }

        // TODO: Port colormap code to backend.
        /// <summary>
        /// The colormap to use for mapping numeric data to colors. If not specified, a default color will be used
        /// instead.
        /// </summary>
        public string Colormap { get; init; }
    }

    /// <summary>
    /// The input for the <see cref="PlotGraphOperation" /> operation.
    /// </summary>
    public struct PlotGraphOperationIn
    {
        /// <summary>
        /// The title of the graph visualization.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The graph to visualize.
        /// </summary>
        public Graph Graph { get; set; }

        /// <summary>
        /// The strategy to use for coloring the graph.
        /// </summary>
        public GraphColorStrategy ColorStrategy { get; set; }
        /// <summary>
        /// The color map to use for mapping numeric data to colors. If not specified, a default color will be used
        /// instead.
        /// </summary>
        public string ColorMap { get; set; }

        /// <summary>
        /// An optional style to apply to vertices.
        /// </summary>
        public PlotStyle? VertexStyle { get; set; }
        /// <summary>
        /// An optional style to apply to edges.
        /// </summary>
        public PlotStyle? EdgeStyle { get; set; }
        /// <summary>
        /// An optional style to apply to the axes.
        /// </summary>
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
        public override async Task<PlotGraphOperationOut> Perform(
            PlotGraphOperationIn input,
            OperationJob job)
        {
            // Check if the graph is enumerable.
            if (input.Graph is not IEnumerableComponent<IVertex, IEdge> enumerableComponent)
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

            // Generate the data for the graph plot.
            List<GraphPlotVertex> graphPlotVertices = new();
            List<GraphPlotEdge> graphPlotEdges = new();
            await foreach (Vertex vertex in enumerableComponent.GetVertices())
            {
                // Create the data for the vertex.
                GraphPlotVertex graphPlotVertex = new()
                {
                    Id = vertex.Id.ToString(),
                    Label = vertex.Label,
                    // Value = vertex.Value,
                    Style = input.VertexStyle
                };
                graphPlotVertices.Add(graphPlotVertex);

                // Create the data for the edges.
                foreach (Edge edge in vertex.Edges)
                {
                    GraphPlotEdge graphPlotEdge = new()
                    {
                        Source = edge.Source.ToString(),
                        Target = edge.Target.ToString(),
                        Directed = edge.Directed,
                        Style = input.EdgeStyle
                    };
                    graphPlotEdges.Add(graphPlotEdge);
                }
            }

            // Create a new graph plot structure.
            GraphPlot graphPlot = new()
            {
                Vertices = graphPlotVertices.ToArray(),
                Edges = graphPlotEdges.ToArray(),
                Style = styles,
                Colormap = input.ColorMap
            };
            return new() { Plot = graphPlot };
        }
    }
}