using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Selection
{
    [DiscriminantDerived("ancestors")]
    public class SelectorAncestors : Selector
    {
        #region API Parameters
        /// <summary>
        /// The list of IDs to select the ancestors of.
        /// </summary>
        public List<string> Ids { get; set; }
        /// <summary>
        /// Whether or not to include the vertices by the specified IDs. If true, the specified vertices will be
        /// included. If false, the specified vertices will be excluded. Defaults to true.
        /// </summary>
        public bool IncludeRoots { get; set; } = true;
        /// <summary>
        /// The depth of ancestors to select. If set to zero, only the vertices specified by ID will be selected (if
        /// include roots is true). If set to one, only the vertices specified by ID and their parents will be selected.
        /// If not specified, the entire ancestor hierarchy will be traversed. Defaults to null. 
        /// </summary>
        public int? Depth { get; set; } = null;
        /// <summary>
        /// The type of traversal on the ancestors to perform. This can be a standard preorder or postorder graph
        /// traversal. Defaults to preorder.
        /// </summary>
        public GraphTraversalType Traversal { get; set; } = GraphTraversalType.Preorder;
        #endregion

        #region Algorithm Storage
        private IDynamicOutGraph<IOutVertex> DynamicOutGraph;
        private IDynamicInGraph<IInVertex> DynamicInGraph;
        private HashSet<Identity> RetrievedIds;
        #endregion

        public SelectorAncestors()
        {
            // Setup the storage for unique vertex identities.
            DynamicInGraph = null;
            RetrievedIds = new HashSet<Identity>();
        }

        private async Task<bool> ContainsVertex(IOutVertex vertex)
        {
            foreach (Edge outEdge in vertex.OutEdges)
            {
                if (Ids.Any(id => Identity.Create(id).Equals(outEdge.Target)))
                    return true;
            }
            RetrievedIds.Add(vertex.Identifier);

            await foreach (IOutVertex childVertex in DynamicOutGraph.GetChildVertices(vertex.Identifier))
            {
                if (!RetrievedIds.Contains(childVertex.Identifier) && await ContainsVertex(childVertex))
                    return true;
            }
            return false;
        }
        public override async Task<bool> ContainsVertex(IVertex vertex)
        {
            if (Ids.Any(id => Identity.Create(id).Equals(vertex.Identifier)))
                return IncludeRoots;

            if (Graph.TryProvide(out IDynamicOutGraph<IOutVertex> dynamic))
            {
                DynamicOutGraph = dynamic;
                RetrievedIds.Clear();

                IOutVertex outVertex = await dynamic.GetVertex(vertex.Identifier);
                return await ContainsVertex(outVertex);
            }
            return false;
        }

        private async IAsyncEnumerable<IInVertex> TraverseAncestor(IInVertex vertex, Identity id, int? depth)
        {
            // Emit base vertex for preorder traversal.
            if (vertex is not null && Traversal == GraphTraversalType.Preorder)
                yield return vertex;

            // Only continue traversal if we haven't already visited this vertex and we haven't surpassed the specified
            // depth.
            if (!RetrievedIds.Contains(id) && (depth is null || depth > 0))
            {
                // Fetch the parent vertices of the base vertex and enumerate their ancestors.
                RetrievedIds.Add(id);
                foreach (Identity parentId in vertex.InEdges.Select(edge => edge.Source))
                {
                    // Check if the parent vertex has already been retrieved before traversing it.
                    IInVertex parentVertex = await DynamicInGraph.GetVertex(parentId);
                    if (RetrievedIds.Contains(parentVertex.Identifier)) continue;

                    await foreach (
                        IInVertex ancestorVertex in
                        TraverseAncestor(parentVertex, parentVertex.Identifier, depth - 1)
                    ) yield return ancestorVertex;
                }
            }

            // Emit base vertex for postorder traversal.
            if (vertex is not null && Traversal == GraphTraversalType.Postorder)
                yield return vertex;
        }
        private async IAsyncEnumerable<IInVertex> TraverseRoot(Identity id)
        {
            // Get the root element if it is supposed to be included.
            IInVertex vertex = null;
            if (IncludeRoots && !RetrievedIds.Contains(id))
                vertex = await DynamicInGraph.GetVertex(id);

            // Return the traversal of the the retrieved node.
            await foreach (IInVertex ancestorVertex in TraverseAncestor(vertex, id, Depth))
                yield return ancestorVertex;
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<IVertex> GetVertices()
        {
            // Check that the graph is dynamic with in-edges.
            if (Graph.TryProvide(out IDynamicInGraph<IInVertex> dynamicIn))
            {
                // Clear the set of retrieved vertices and set our graph.
                DynamicInGraph = dynamicIn;
                RetrievedIds.Clear();

                // Retrieve the vertices rooted at the specified IDs.
                // Make sure to check whether the IDs are specified first.
                if (Ids is null) throw new ArgumentNullException(nameof(Ids));
                foreach (string id in Ids)
                {
                    await foreach (IInVertex vertex in TraverseRoot(Identity.Create(id)))
                        yield return vertex;
                }
            }
            else
            {
                await foreach (IVertex vertex in base.GetVertices())
                    yield return vertex;
            }
        }

        [DiscriminantAlias("parents")]
        public static SelectorAncestors CreateParentsSelector() => new SelectorAncestors
        {
            Depth = 1,
            IncludeRoots = false
        };
    }
}