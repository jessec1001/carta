using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Visualization
{
    public enum GraphColorStrategy
    {
        Hierarchical,
        Random,
        Connectedness
    }

    // TODO: Make this consistent with VisJS format if possible.
    public struct GraphPlotVertex
    {
        public string Id { get; set; }
        public string Label { get; set; }

        public double? Value { get; set; }

        public PlotStyle? Style { get; set; }
    }
    // TODO: Make this consistent with VisJS format if possible.
    public struct GraphPlotEdge
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public bool Directed { get; set; }

        public PlotStyle? Style { get; set; }
    }
    public class GraphPlot : Plot
    {
        public override string Type => "graph";

        public GraphPlotVertex[] Vertices { get; set; }
        public GraphPlotEdge[] Edges { get; set; }

        // TODO: Port colormap code to backend.
        public string Colormap { get; init; }
    }

    /// <summary>
    /// The input for the <see cref="PlotGraphOperation" /> operation.
    /// </summary>
    public struct PlotGraphOperationIn
    {
        /// <summary>
        /// The name of the output visualization.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The graph to visualize.
        /// </summary>
        public Graph Graph { get; set; }

        public GraphColorStrategy ColorStrategy { get; set; }
        public string ColorMap { get; set; }

        public PlotStyle? VertexStyle { get; set; }
        public PlotStyle? EdgeStyle { get; set; }
        public PlotStyle? AxesStyle { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="PlotGraphOperation" /> operation.
    /// </summary>
    public struct PlotGraphOperationOut
    {
        public GraphPlot Plot { get; set; }
    }

    /// <summary>
    /// Visualizes a graph plot of a network graph.
    /// </summary>
    [OperationName(Display = "Graph Plot", Type = "visualizeGraphPlot")]
    [OperationTag(OperationTags.Visualization)]
    [OperationVisualizer("graph")]
    public class PlotGraphOperation : TypedOperation
    <
        PlotGraphOperationIn,
        PlotGraphOperationOut
    >
    {
        /// <inheritdoc />
        public override async Task<PlotGraphOperationOut> Perform(
            PlotGraphOperationIn input,
            OperationContext callingContext)
        {
            // Check if the graph is enumerable.
            if (input.Graph is not IEntireGraph entireGraph)
                throw new ArgumentException("The graph must be a finite graph.");

            // Generate the data for the graph plot.
            List<GraphPlotVertex> graphPlotVertices = new();
            List<GraphPlotEdge> graphPlotEdges = new();
            await foreach (Vertex vertex in entireGraph.GetVertices())
            {
                // Create the data for the vertex.
                GraphPlotVertex graphPlotVertex = new()
                {
                    Id = vertex.Identifier.ToString(),
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
                Colormap = input.ColorMap
            };

            // TODO: Remove this and replace with a plot output to this operation.
        }
    }
}