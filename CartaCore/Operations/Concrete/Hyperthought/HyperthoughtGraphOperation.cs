using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Integration.Hyperthought;
using CartaCore.Integration.Hyperthought.Api;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    public struct HyperthoughtGraphOperationIn
    {
        /// <summary>
        /// The reference to the authenticated HyperThought API. 
        /// </summary>
        public HyperthoughtApi Api { get; set; }
        /// <summary>
        /// The dot-separated path to the HyperThought workflow.
        /// </summary>
        public string Path { get; set; }
    }
    public struct HyperthoughtGraphOperationOut
    {
        /// <summary>
        /// The graph
        /// </summary>
        /// <value></value>
        public Graph Graph { get; set; }
    }

    [OperationName(Display = "HyperThought Workflow as Graph", Type = "hyperthoughtWorkflowGraph")]
    [OperationTag(OperationTags.Hyperthought)]
    [OperationTag(OperationTags.Graph)]
    [OperationTag(OperationTags.Loading)]
    public class HyperthoughtGraphOperation : TypedOperation
    <
        HyperthoughtGraphOperationIn,
        HyperthoughtGraphOperationOut
    >
    {
        private class HyperthoughtGraph : WrapperGraph, IEntireGraph
        {
            public HyperthoughtGraph(Identity id) : base(id)
            {
            }
            public HyperthoughtGraph(Identity id, IEnumerable<Property> properties) : base(id, properties)
            {
            }

            public OperationContext Context { get; set; }
            public Guid Root { get; set; }
            public HyperthoughtWorkflowGraph Graph { get; set; }
            protected override IGraph WrappedGraph => Graph;

            public async IAsyncEnumerable<IVertex> GetVertices()
            {
                // We execute a traversal algorithm over the graph while constantly checking the priority queue.
                HashSet<Guid> completedIds = new();
                Queue<Guid> pendingIds = new();

                // Add the root node to the pending list.
                pendingIds.Enqueue(Root);

                // Fetch the root node.
                Vertex rootVertex = await Graph.GetVertex(Identity.Create(Root));
                yield return rootVertex;

                // Keep going until we've completed all the nodes.
                while (pendingIds.Count > 0)
                {
                    // Check if there are any selectors in the priority queue.
                    while (!Context.Selectors.IsEmpty)
                    {
                        // Get the next selector.
                        if (!Context.Selectors.TryDequeue(out (Selector selector, object selectorInput) item)) continue;

                        // Get the vertices in the selector.
                        Graph selectorGraph = await item.selector.Select(Graph, item.selectorInput);
                        if (((IGraph)selectorGraph).TryProvide(out IEntireGraph entireGraph))
                        {
                            await foreach (Vertex vertex in entireGraph.GetVertices())
                            {
                                Guid vertexId = vertex.Identifier.AsType<Guid>();
                                if (completedIds.Contains(vertexId)) continue;
                                yield return vertex;
                            }
                        }
                    }

                    // Pop the next identifier.
                    Guid id = pendingIds.Dequeue();

                    // Get the vertex children.
                    await foreach (Vertex childVertex in Graph.GetChildVertices(Identity.Create(id)))
                    {
                        // If the child is not already completed, add it to the pending list.
                        Guid childId = childVertex.Identifier.AsType<Guid>();
                        if (!completedIds.Contains(childId))
                        {
                            pendingIds.Enqueue(childId);
                            yield return childVertex;
                        }
                    }
                }
            }
        }

        // TODO: Make it clear that operations need to be performed asynchronously.
        /// <inheritdoc />
        public override async Task<HyperthoughtGraphOperationOut> Perform(HyperthoughtGraphOperationIn input, OperationContext context)
        {
            // Get the UUID of the graph.
            Guid uuid = await input.Api.Workflow.GetProcessIdFromPathAsync(input.Path);

            // Create the graph.
            HyperthoughtWorkflowGraph graph = new HyperthoughtWorkflowGraph(input.Api, uuid);

            // Return the wrapped graph.
            HyperthoughtGraph wrappedGraph = new(graph.Identifier, graph.Properties)
            {
                Context = context,
                Graph = graph,
                Root = uuid
            };
            return new HyperthoughtGraphOperationOut { Graph = wrappedGraph };
        }
    }
}