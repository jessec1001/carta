using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Operations.Attributes;
using CartaCore.Operations.Visualization;

namespace CartaCore.Operations
{
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
    }
    /// <summary>
    /// The output for the <see cref="PlotGraphOperation" /> operation.
    /// </summary>
    public struct PlotGraphOperationOut { }

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
        // TODO: Make this consistent with VisJS format if possible.
        /// <summary>
        /// The value of a vertex in a graph plot.
        /// </summary>
        private struct GraphPlotValue
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
            /// The unique identifiers of the vertices that this vertex is connected to.
            /// </summary>
            public List<string> Neighbors { get; set; }
        }

        /// <inheritdoc />
        public override async Task<PlotGraphOperationOut> Perform(
            PlotGraphOperationIn input,
            OperationContext callingContext)
        {
            // Check if the graph is enumerable.
            if (input.Graph is not IEntireGraph entireGraph)
                throw new ArgumentException("The graph must be a finite graph.");

            // Generate the data for the graph plot.
            List<GraphPlotValue> graphPlotValues = new();
            await foreach (Vertex vertex in entireGraph.GetVertices())
            {
                // Create the data for this vertex.
                GraphPlotValue graphPlotValue = new()
                {
                    Id = vertex.Identifier.ToString(),
                    Label = vertex.Label,
                    Neighbors = new()
                };
                foreach (Edge edge in vertex.OutEdges)
                    graphPlotValue.Neighbors.Add(edge.Target.ToString());

                // Add the data for this vertex to the graph plot.
                graphPlotValues.Add(graphPlotValue);
            }
            Plot<GraphPlotValue> graphPlot = new()
            {
                Data = graphPlotValues.ToArray()
            };

            // Output the visualization data to the calling context.
            if (callingContext is not null && callingContext.Output.TryAdd(input.Name, graphPlot))
                return new PlotGraphOperationOut();
            else
                throw new ArgumentException($"Cannot set visualization '{input.Name}'.");
        }
    }
}