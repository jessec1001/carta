using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="FilterDescendantsOperation"/> operation.
    /// </summary>
    public struct FilterDescendantsOperationIn
    {
        /// <summary>
        /// The graph to filter vertices on.
        /// </summary>
        [OperationSelectorGraph]
        public Graph Graph { get; set; }
        /// <summary>
        /// The vertex to select the descendants of.
        /// </summary>
        public Vertex Vertex { get; set; }
        /// <summary>
        /// Whether or not to include the vertices by the specified IDs. If true, the specified root vertices will be
        /// included. If false, the specified root vertices will be excluded.
        /// </summary>
        /// <value></value>
        public bool IncludeRoots { get; set; }
        /// <summary>
        /// The depth of ancestors to select.
        /// - If set to zero, only the vertices specified by ID will be selected (if include roots is true).
        /// - If set to one, only the vertices specified by ID and their parents will be selected.
        /// - If unset, the entire ancestor hierarchy will be traversed.
        /// </summary>
        public int? Depth { get; set; }
        /// <summary>
        /// The type of [tree traversal](https://en.wikipedia.org/wiki/Tree_traversal) on the ancestors to perform.
        /// This can be a standard preorder or postorder tree traversal.
        /// </summary>
        public GraphTraversalType Traversal { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="FilterDescendantsOperation"/> operation.
    /// </summary>
    public struct FilterDescendantsOperationOut
    {
        /// <summary>
        /// The resulting graph containing descendant vertices.
        /// </summary>
        [OperationSelectorGraph]
        public Graph Graph { get; set; }
    }

    /// <summary>
    /// Filters the vertices of a graph to only include the descendants of a specified vertex.
    /// </summary>
    [OperationSelector("descendants")]
    public class FilterDescendantsOperation : TypedOperation
    <
        FilterDescendantsOperationIn,
        FilterDescendantsOperationOut
    >
    {
        /// <summary>
        /// A graph that represents the descendants of a vertex.
        /// </summary>
        private class DescendantsGraph : WrapperGraph, IEntireGraph
        {
            /// <summary>
            /// The base graph.
            /// </summary>
            public Graph Graph { get; private init; }

            /// <summary>
            /// The identifier to select the descendants of.
            /// </summary>
            public Identity Id { get; set; }
            /// <summary>
            /// Whether or not to include the vertices by the specified IDs. If true, the specified root vertices will be
            /// included. If false, the specified root vertices will be excluded. Defaults to true.
            /// </summary>
            public bool IncludeRoots { get; set; } = true;
            /// <summary>
            /// The depth of descendants to select.
            /// </summary>
            public int? Depth { get; set; } = null;
            /// <summary>
            /// The type of tree traversal to perform.
            /// </summary>
            public GraphTraversalType Traversal { get; set; } = GraphTraversalType.Preorder;

            /// <inheritdoc />
            protected override IGraph WrappedGraph => Graph;

            private readonly IDynamicOutGraph<Vertex> DynamicOutGraph;
            private readonly IDynamicInGraph<Vertex> DynamicInGraph;
            private readonly HashSet<Identity> RetrievedIds;

            /// <summary>
            /// Creates a new instance of the <see cref="DescendantsGraph"/> class to initialize algorithm variables.
            /// </summary>
            /// <param name="graph"></param>
            public DescendantsGraph(Graph graph)
                : base(graph.Identifier, graph.Properties)
            {
                // Try to get dynamic graph interfaces.
                if (((IGraph)graph).TryProvide(out IDynamicOutGraph<Vertex> dynamicOutGraph))
                    DynamicOutGraph = dynamicOutGraph;
                if (((IGraph)graph).TryProvide(out IDynamicInGraph<Vertex> dynamicInGraph))
                    DynamicInGraph = dynamicInGraph;

                // Setup the storage for unique vertex identities.
                RetrievedIds = new HashSet<Identity>();
            }

            private async Task<bool> ContainsVertex(Vertex vertex)
            {
                foreach (Edge inEdge in vertex.InEdges)
                {
                    if (Id.Equals(inEdge.Target))
                        return true;
                }
                RetrievedIds.Add(vertex.Identifier);

                await foreach (Vertex parentVertex in DynamicInGraph.GetParentVertices(vertex.Identifier))
                {
                    if (!RetrievedIds.Contains(parentVertex.Identifier) && await ContainsVertex(parentVertex))
                        return true;
                }
                return false;
            }
            /// <inheritdoc />
            public async Task<bool> ContainsVertex(IVertex vertex)
            {
                if (Id.Equals(vertex.Identifier))
                    return IncludeRoots;

                if (DynamicInGraph is not null)
                {
                    RetrievedIds.Clear();
                    Vertex inVertex = await DynamicInGraph.GetVertex(vertex.Identifier);
                    return await ContainsVertex(inVertex);
                }
                return false;
            }

            private async IAsyncEnumerable<Vertex> TraverseDescendant(Vertex vertex, Identity id, int? depth)
            {
                // Emit base vertex for preorder traversal.
                if (vertex is not null && Traversal == GraphTraversalType.Preorder)
                    yield return vertex;

                // Only continue traversal if we haven't already visited this vertex and we haven't surpassed the specified
                // depth.
                if (!RetrievedIds.Contains(id) && (depth is null || depth > 0))
                {
                    // Fetch the child vertices of the base vertex and enumerate their descendants.
                    RetrievedIds.Add(id);
                    await foreach (Vertex childVertex in DynamicOutGraph.GetChildVertices(id))
                    {
                        // Check if the child vertex has already been retrieved before traversing it.
                        if (RetrievedIds.Contains(childVertex.Identifier)) continue;

                        await foreach (
                            Vertex descendantVertex in
                            TraverseDescendant(childVertex, childVertex.Identifier, depth - 1)
                        ) yield return descendantVertex;
                    }
                }

                // Emit base vertex for postorder traversal.
                if (vertex is not null && Traversal == GraphTraversalType.Postorder)
                    yield return vertex;
            }
            private async IAsyncEnumerable<Vertex> TraverseRoot(Identity id)
            {
                // Get the root element if it is supposed to be included.
                Vertex vertex = null;
                if (IncludeRoots && !RetrievedIds.Contains(id))
                    vertex = await DynamicOutGraph.GetVertex(id);

                // Return the traversal of the the retrieved vertex.
                await foreach (Vertex descendantVertex in TraverseDescendant(vertex, id, Depth))
                    yield return descendantVertex;
            }

            /// <inheritdoc />
            public async IAsyncEnumerable<IVertex> GetVertices()
            {
                // Check that the graph is dynamic with out-edges.
                if (DynamicOutGraph is not null)
                {
                    // Retrieve the vertices rooted at the specified ID.
                    RetrievedIds.Clear();
                    await foreach (Vertex vertex in TraverseRoot(Id))
                        yield return vertex;
                }
                else
                {
                    if (((IGraph)Graph).TryProvide(out IEntireGraph entireGraph))
                    {
                        await foreach (IVertex vertex in entireGraph.GetVertices())
                            yield return vertex;
                    }
                }
            }
        }

        /// <inheritdoc />
        public override Task<FilterDescendantsOperationOut> Perform(FilterDescendantsOperationIn input)
        {
            // Create a new graph to store the descendants.
            return Task.FromResult(new FilterDescendantsOperationOut
            {
                Graph = new DescendantsGraph(input.Graph)
                {
                    Id = input.Vertex.Identifier,
                    IncludeRoots = input.IncludeRoots,
                    Depth = input.Depth,
                    Traversal = input.Traversal
                }
            });
        }
    }
}