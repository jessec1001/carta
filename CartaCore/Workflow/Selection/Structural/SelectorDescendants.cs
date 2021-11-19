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
    /// Selects the descendant vertices of a list of specified vertices to a certain depth.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("descendants")]
    [DiscriminantSemantics(Name = "Select Descendants", Group = "Structural")]
    public class SelectorDescendants : Selector
    {
        #region API Parameters
        /// <summary>
        /// The list of IDs to select the descendants of.
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
        /// The depth of descendants to select.
        /// - If set to zero, only the vertices specified by ID will be selected (if include roots is true).
        /// - If set to one, only the vertices specified by ID and their children will be selected.
        /// - If set to null, the entire descendant hierarchy will be traversed.
        /// 
        /// Defaults to null. 
        /// </summary>
        [DataMember(Name = "depth")]
        [Display(Name = "Depth")]
        [Range(0d, double.PositiveInfinity)]
        public int? Depth { get; set; } = null;
        /// <summary>
        /// The type of [tree traversal](https://en.wikipedia.org/wiki/Tree_traversal) on the descendants to perform.
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
        /// Creates a new instance of the <see cref="SelectorDescendants"/> class to initialize algorithm variables.
        /// </summary>
        public SelectorDescendants()
        {
            // Setup the storage for unique vertex identities.
            DynamicOutGraph = null;
            DynamicInGraph = null;
            RetrievedIds = new HashSet<Identity>();
        }

        private async Task<bool> ContainsVertex(Vertex vertex)
        {
            foreach (Edge inEdge in vertex.InEdges)
            {
                if (Ids.Any(id => Identity.Create(id).Equals(inEdge.Source)))
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
        public override async Task<bool> ContainsVertex(IVertex vertex)
        {
            if (Ids.Any(id => Identity.Create(id).Equals(vertex.Identifier)))
                return IncludeRoots;

            if (Graph.TryProvide(out IDynamicInGraph<Vertex> dynamic))
            {
                DynamicInGraph = dynamic;
                RetrievedIds.Clear();

                Vertex inVertex = await dynamic.GetVertex(vertex.Identifier);
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
                    // Check if the parent vertex has already been retrieved before traversing it.
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
        public override async IAsyncEnumerable<IVertex> GetVertices()
        {
            // Check that the graph is dynamic with out-edges.
            if (Graph.TryProvide(out IDynamicOutGraph<Vertex> dynamicOut))
            {
                // Clear the set of retrieved vertices and set our graph.
                DynamicOutGraph = dynamicOut;
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
        /// A descendants selector that only selects the direct children of the specified vertices.
        /// </summary>
        [DiscriminantAlias("children")]
        public static SelectorDescendants CreateChildrenSelector() => new()
        {
            IncludeRoots = false,
            Depth = 1
        };
    }
}