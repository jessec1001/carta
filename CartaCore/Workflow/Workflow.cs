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
        public async Task<IEntireGraph> ApplyAsync(IEntireGraph graph)
        {
            if (Operations is null) return graph;

            // Copy over the entire graph.
            Graph baseGraph = graph.Provide<Graph>();
            FiniteGraph prevGraph = new FiniteGraph
            (
                baseGraph.Identifier,
                baseGraph.Properties,
                baseGraph.IsDirected
            );
            await foreach (IVertex vertex in graph.GetVertices())
                prevGraph.AddVertex(vertex);

            // Apply each workflow operation in sequence.
            foreach (WorkflowOperation operation in Operations)
            {
                ActionBase action = operation.Action;
                Selector selector = operation.Selector ?? new SelectorAll();
                selector.Graph = graph;

                FiniteGraph nextGraph = new FiniteGraph
                (
                    prevGraph.Identifier,
                    prevGraph.Properties,
                    prevGraph.IsDirected
                );

                await foreach (Edge edge in ((IEntireGraph)prevGraph).GetEdges())
                    nextGraph.AddEdge(edge);
                await foreach (IVertex vertex in prevGraph.GetVertices())
                {
                    if (action is null)
                        nextGraph.AddVertex(vertex);
                    else
                    {
                        nextGraph.AddVertex
                        (
                            (selector is not null && !await selector.ContainsVertex(vertex)) ?
                            vertex : await action.ApplyToVertex(graph, new Vertex
                            (
                                vertex.Identifier,
                                await vertex.Properties
                                .ToAsyncEnumerable()
                                .SelectAwait(async property =>
                                    (selector is not null && !await selector.ContainsProperty(property)) ?
                                    property : await action.ApplyToProperty(new Property
                                    (
                                        property.Identifier,
                                        await property.Values
                                        .ToAsyncEnumerable()
                                        .SelectAwait(async value =>
                                            (selector is not null && !await selector.ContainsValue(value)) ?
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
                prevGraph = nextGraph;
            }

            return prevGraph;
        }
    }
}