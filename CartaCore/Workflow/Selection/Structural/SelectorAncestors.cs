using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

using NJsonSchema.Annotations;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Selects the ancestor vertices of a list of specified vertices to a certain depth.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("ancestors")]
    [DiscriminantSemantics(Name = "Select Ancestors", Group = "Structural")]
    public class SelectorAncestors : Selector
    {
        #region API Parameters
        /// <summary>
        /// The list of IDs to select the ancestors of.
        /// </summary>
        [DataMember(Name = "ids")]
        [Display(Name = "IDs")]
        [Required]
        public List<string> Ids { get; set; }
        /// <summary>
        /// Whether or not to include the vertices by the specified IDs. If true, the specified root vertices will be
        /// included. If false, the specified root vertices will be excluded. Defaults to true.
        /// </summary>
        [DataMember(Name = "includeRoots")]
        [Display(Name = "Include Roots")]
        [Required]
        public bool IncludeRoots { get; set; } = true;
        /// <summary>
        /// The depth of ancestors to select.
        /// - If set to zero, only the vertices specified by ID will be selected (if include roots is true).
        /// - If set to one, only the vertices specified by ID and their parents will be selected.
        /// - If set to null, the entire ancestor hierarchy will be traversed.
        /// 
        /// Defaults to null. 
        /// </summary>
        [DataMember(Name = "depth")]
        [Display(Name = "Depth")]
        [Range(0d, double.PositiveInfinity)]
        public int? Depth { get; set; } = null;
        /// <summary>
        /// The type of [tree traversal](https://en.wikipedia.org/wiki/Tree_traversal) on the ancestors to perform.
        /// This can be a standard preorder or postorder tree traversal. Defaults to preorder.
        /// </summary>
        [DataMember(Name = "traversal")]
        [Display(Name = "Traversal")]
        [Required]
        public GraphTraversalType Traversal { get; set; } = GraphTraversalType.Preorder;
        #endregion

        #region Algorithm Storage
        private IDynamicOutGraph<Vertex> DynamicOutGraph;
        private IDynamicInGraph<Vertex> DynamicInGraph;
        private readonly HashSet<Identity> RetrievedIds;
        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="SelectorAncestors"/> class to initialize algorithm variables.
        /// </summary>
        public SelectorAncestors()
        {
            // Setup the storage for unique vertex identities.
            DynamicInGraph = null;
            RetrievedIds = new HashSet<Identity>();
        }

        private async Task<bool> ContainsVertex(Vertex vertex)
        {
            foreach (Edge outEdge in vertex.OutEdges)
            {
                if (Ids.Any(id => Identity.Create(id).Equals(outEdge.Target)))
                    return true;
            }
            RetrievedIds.Add(vertex.Identifier);

            await foreach (Vertex childVertex in DynamicOutGraph.GetChildVertices(vertex.Identifier))
            {
                if (!RetrievedIds.Contains(childVertex.Identifier) && await ContainsVertex(childVertex))
                    return true;
            }
            return false;
        }
        /// <inheritdoc />
        public override async Task<bool> ContainsVertex(IVertex vertex)
        {
            if (Ids.Any(id => Identity.Create(id).Equals(vertex.Identifier)))
                return IncludeRoots;

            if (Graph.TryProvide(out IDynamicOutGraph<Vertex> dynamic))
            {
                DynamicOutGraph = dynamic;
                RetrievedIds.Clear();

                Vertex outVertex = await dynamic.GetVertex(vertex.Identifier);
                return await ContainsVertex(outVertex);
            }
            return false;
        }

        private async IAsyncEnumerable<Vertex> TraverseAncestor(Vertex vertex, Identity id, int? depth)
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
                await foreach (Vertex parentVertex in DynamicInGraph.GetParentVertices(id))
                {
                    // Check if the parent vertex has already been retrieved before traversing it.
                    if (RetrievedIds.Contains(parentVertex.Identifier)) continue;

                    await foreach (
                        Vertex ancestorVertex in
                        TraverseAncestor(parentVertex, parentVertex.Identifier, depth - 1)
                    ) yield return ancestorVertex;
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
                vertex = await DynamicInGraph.GetVertex(id);

            // Return the traversal of the the retrieved vertex.
            await foreach (Vertex ancestorVertex in TraverseAncestor(vertex, id, Depth))
                yield return ancestorVertex;
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<IVertex> GetVertices()
        {
            // Check that the graph is dynamic with in-edges.
            if (Graph.TryProvide(out IDynamicInGraph<Vertex> dynamicIn))
            {
                // Clear the set of retrieved vertices and set our graph.
                DynamicInGraph = dynamicIn;
                RetrievedIds.Clear();

                // Retrieve the vertices rooted at the specified IDs.
                // Make sure to check whether the IDs are specified first.
                if (Ids is null) throw new ArgumentNullException(nameof(Ids));
                foreach (string id in Ids)
                {
                    await foreach (Vertex vertex in TraverseRoot(Identity.Create(id)))
                        yield return vertex;
                }
            }
            else
            {
                await foreach (IVertex vertex in base.GetVertices())
                    yield return vertex;
            }
        }

        /// <summary>
        /// A ancestors selector that only selects the direct parents of the specified vertices.
        /// </summary>
        [DiscriminantAlias("parents")]
        public static SelectorAncestors CreateParentsSelector() => new()
        {
            Depth = 1,
            IncludeRoots = false
        };
    }
}