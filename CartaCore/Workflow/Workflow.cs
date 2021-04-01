using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Workflow.Action;
using CartaCore.Workflow.Selection;

namespace CartaCore.Workflow
{
    /// <summary>
    /// Represents a linear sequence of operations that can be performed on a graph object. These operations can be
    /// performed asynchronously.
    /// </summary>
    public class Workflow
    {
        /// <summary>
        /// Gets or sets the stored identifier for the workflow.
        /// </summary>
        /// <value>The workflow identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the workflow name.
        /// </summary>
        /// <value>A human-readable name for the workflow.</value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the workflow operations.
        /// </summary>
        /// <returns></returns>
        public List<WorkflowOperation> Operations { get; set; } = null;

        /// <summary>
        /// Applies the workflow to a finite graph and returns another graph with the operations of the workflow
        /// applied.
        /// </summary>
        /// <param name="graph">The graph to apply the workflow to.</param>
        /// <returns>The graph after being acted on by the workflow operations.</returns>
        public async Task<FiniteGraph> ApplyAsync(FiniteGraph graph)
        {
            IGraph underlying = graph.UnderlyingGraph;
            if (Operations is null) return graph;

            // Apply each workflow operation in sequence.
            foreach (WorkflowOperation operation in Operations)
            {
                ActionBase action = operation.Action;
                SelectorBase selector = operation.Selector ?? new SelectorAll();

                FiniteGraph transformedGraph = new FiniteGraph
                (
                    graph.Identifier,
                    graph.Properties,
                    graph.IsDirected,
                    graph.IsDynamic
                );

                await foreach (Edge edge in graph.Edges)
                    transformedGraph.AddEdge(edge);
                await foreach (IVertex vertex in graph.Vertices)
                {
                    if (action is null)
                        transformedGraph.AddVertex(vertex);
                    else
                    {
                        transformedGraph.AddVertex
                        (
                            (selector is not null && !selector.ContainsVertex(vertex)) ?
                            vertex : await action.ApplyToVertex(underlying, new Vertex
                            (
                                vertex.Identifier,
                                await vertex.Properties
                                .ToAsyncEnumerable()
                                .SelectAwait(async property =>
                                    (selector is not null && !selector.ContainsProperty(property)) ?
                                    property : await action.ApplyToProperty(new Property
                                    (
                                        property.Identifier,
                                        await property.Values
                                        .ToAsyncEnumerable()
                                        .SelectAwait(async value =>
                                            (selector is not null && !selector.ContainsValue(value)) ?
                                            value : await action.ApplyToValue(value)
                                        ).ToListAsync()
                                    )
                                    { Subproperties = property.Subproperties })
                                ).ToListAsync()
                            )
                            {
                                Label = vertex.Label,
                                Description = vertex.Description
                            })
                        );
                    }
                }
                graph = transformedGraph;
            }

            return graph;
        }
    }
}