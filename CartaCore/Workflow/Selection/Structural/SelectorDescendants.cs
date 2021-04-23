using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Selection
{
    public enum GraphTraversalType
    {
        Preorder,
        Postorder
    }

    [DiscriminantDerived("descendants")]
    public class SelectorDescendants : Selector
    {
        #region API Parameters
        /// <summary>
        /// The list of IDs to select the descendants of.
        /// </summary>
        public List<string> Ids { get; set; }
        /// <summary>
        /// Whether or not to include the vertices by the specified IDs. If true, the specified vertices will be
        /// included. If false, the specified vertices will be excluded. Defaults to true.
        /// </summary>
        public bool IncludeRoots { get; set; } = true;
        /// <summary>
        /// The depth of descendants to select. If set to zero, only the vertices specified by ID will be selected (if
        /// include roots is true). If set to one, only the vertices specified by ID and their children will be
        /// selected. If not specified, the entire descendant hierarchy will be traversed. Defaults to null. 
        /// </summary>
        public int? Depth { get; set; } = null;
        /// <summary>
        /// The type of traversal on the descendants to perform. This can be a standard preorder or postorder graph
        /// traversal. Defaults to preorder.
        /// </summary>
        public GraphTraversalType Traversal { get; set; } = GraphTraversalType.Preorder;
        #endregion

        #region Algorithm Storage
        private IDynamicOutGraph<IOutVertex> DynamicOutGraph;
        private IDynamicInGraph<IInVertex> DynamicInGraph;
        private HashSet<Identity> RetrievedIds;
        #endregion

        public SelectorDescendants()
        {
            // Setup the storage for unique vertex identities.
            DynamicOutGraph = null;
            DynamicInGraph = null;
            RetrievedIds = new HashSet<Identity>();
        }

        private async Task<bool> ContainsVertex(IInVertex vertex)
        {
            foreach (Edge inEdge in vertex.InEdges)
            {
                if (Ids.Any(id => Identity.Create(id).Equals(inEdge.Source)))
                    return true;
            }
            RetrievedIds.Add(vertex.Identifier);

            await foreach (IInVertex parentVertex in DynamicInGraph.GetParentVertices(vertex.Identifier))
            {
                if (!RetrievedIds.Contains(parentVertex.Identifier) && await ContainsVertex(parentVertex))
                    return true;
            }
            return false;
        }
        public override async Task<bool> ContainsVertex(IVertex vertex)
        {
            if (Ids.Any(id => Identity.Create(id).Equals(vertex.Identifier)))
                return true;

            if (Graph.TryProvide(out IDynamicInGraph<IInVertex> dynamic))
            {
                DynamicInGraph = dynamic;
                RetrievedIds.Clear();

                IInVertex inVertex = await dynamic.GetVertex(vertex.Identifier);
                return await ContainsVertex(inVertex);
            }
            return false;
        }

        private async IAsyncEnumerable<IOutVertex> TraverseDescendant(IOutVertex vertex, Identity id, int? depth)
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
                if (vertex is null)
                    vertex = await DynamicOutGraph.GetVertex(id);
                foreach (Identity childId in vertex.OutEdges.Select(edge => edge.Target))
                {
                    // Check if the parent vertex has already been retrieved before traversing it.
                    IOutVertex childVertex = await DynamicOutGraph.GetVertex(childId);
                    if (RetrievedIds.Contains(childVertex.Identifier)) continue;

                    await foreach (
                        IOutVertex descendantVertex in
                        TraverseDescendant(childVertex, childVertex.Identifier, depth - 1)
                    ) yield return descendantVertex;
                }
            }

            // Emit base vertex for postorder traversal.
            if (vertex is not null && Traversal == GraphTraversalType.Postorder)
                yield return vertex;
        }
        private async IAsyncEnumerable<IOutVertex> TraverseRoot(Identity id)
        {
            // Get the root element if it is supposed to be included.
            IOutVertex vertex = null;
            if (IncludeRoots && !RetrievedIds.Contains(id))
                vertex = await DynamicOutGraph.GetVertex(id);

            // Return the traversal of the the retrieved node.
            await foreach (IOutVertex descendantVertex in TraverseDescendant(vertex, id, Depth))
                yield return descendantVertex;
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<IVertex> GetVertices()
        {
            // Check that the graph is dynamic with out-edges.
            if (Graph.TryProvide(out IDynamicOutGraph<IOutVertex> dynamicOut))
            {
                // Clear the set of retrieved vertices and set our graph.
                DynamicOutGraph = dynamicOut;
                RetrievedIds.Clear();

                // Retrieve the vertices rooted at the specified IDs.
                // Make sure to check whether the IDs are specified first.
                if (Ids is null) throw new ArgumentNullException(nameof(Ids));
                foreach (string id in Ids)
                {
                    await foreach (IOutVertex vertex in TraverseRoot(Identity.Create(id)))
                        yield return vertex;
                }
            }
            else
            {
                await foreach (IVertex vertex in base.GetVertices())
                    yield return vertex;
            }
        }

        [DiscriminantAlias("children")]
        public static SelectorDescendants CreateChildrenSelector() => new SelectorDescendants
        {
            IncludeRoots = false,
            Depth = 1
        };
    }
}