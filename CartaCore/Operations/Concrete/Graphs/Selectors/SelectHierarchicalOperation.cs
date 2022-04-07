using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Graphs.Components;
using CartaCore.Operations.Attributes;
using CartaCore.Serialization.Json;
using MorseCode.ITask;

namespace CartaCore.Operations.Graphs
{
    /// <summary>
    /// The type of graph tree traversal we can perform over a set of hierarchical vertices.
    /// </summary>
    [JsonConverter(typeof(JsonFullStringEnumConverter))]
    public enum HierarchicalTraversalType
    {
        /// <summary>
        /// The tree is traversed in preorder fashion.
        /// </summary>
        [EnumMember(Value = "Preorder")]
        Preorder,
        /// <summary>
        /// The tree is traversed in postorder fashion.
        /// </summary>
        [EnumMember(Value = "Postorder")]
        Postorder
    }
    /// <summary>
    /// The type of hierarchical direction we perform to select vertices.
    /// </summary>
    [JsonConverter(typeof(JsonFullStringEnumConverter))]
    public enum HierarchicalDirectionType
    {
        /// <summary>
        /// The ancestor hierarchy is selected.
        /// </summary>
        [EnumMember(Value = "Ancestors")]
        Ancestors,
        /// <summary>
        /// The descendant hierarchy is selected.
        /// </summary>
        [EnumMember(Value = "Descendants")]
        Descendants
    }

    /// <summary>
    /// The parameters for the selector functionality of the <see cref="SelectHierarchicalOperation"/> operation.
    /// </summary>
    public struct SelectHierarchicalParameters
    {
        /// <summary>
        /// The identifier of the vertex to select the hierarchy of.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Whether or not to include the vertices by the specified IDs. If true, the specified root vertices will be
        /// included. If false, the specified root vertices will be excluded.
        /// </summary>
        public bool IncludeRoots { get; set; }
        /// <summary>
        /// The depth of hierarchy to select.
        /// - If set to zero, only the vertices specified by ID will be selected (if include roots is true).
        /// - If set to one, only the vertices specified by ID and their immediate hierarchy will be selected.
        /// - If unset, the entire hierarchy will be traversed.
        /// </summary>
        public int? Depth { get; set; }
        /// <summary>
        /// The type of [tree traversal](https://en.wikipedia.org/wiki/Tree_traversal) on the hierarchy to perform.
        /// This can be a standard preorder or postorder tree traversal.
        /// </summary>
        public HierarchicalTraversalType Traversal { get; set; }
    }
    /// <summary>
    /// The input for the <see cref="SelectHierarchicalOperation"/> operation.
    /// </summary>
    public struct SelectHierarchicalOperationIn
    {
        /// <summary>
        /// The graph to filter vertices on.
        /// </summary>
        [FieldName("Graph")]
        public Graph Graph { get; set; }
        /// <summary>
        /// The vertex to select the hierarchy of.
        /// </summary>
        [FieldName("Vertex")]
        public Vertex Vertex { get; set; }
        /// <summary>
        /// Whether or not to include the root vertices by the specified IDs. If true, the specified root vertices will
        /// be included. If false, the specified root vertices will be excluded.
        /// </summary>
        [FieldDefault(true)]
        [FieldName("Include Roots")]
        public bool IncludeRoots { get; set; }
        /// <summary>
        /// The depth of the hierarchy to select.
        /// - If set to zero, only the vertices specified by ID will be selected (if include roots is true).
        /// - If set to one, only the vertices specified by ID and their immediate hierarchy will be selected.
        /// - If unset, the entire hierarchy will be traversed.
        /// </summary>
        [FieldRange(Minimum = 0)]
        [FieldName("Depth")]
        public int? Depth { get; set; }
        /// <summary>
        /// The type of [tree traversal](https://en.wikipedia.org/wiki/Tree_traversal) on the hierarchy to perform.
        /// This can be a standard preorder or postorder tree traversal.
        /// </summary>
        [FieldName("Traversal")]
        public HierarchicalTraversalType Traversal { get; set; }
        /// <summary>
        /// The direction of the hierarchy to select.
        /// For ancestors, a vertex is selected if it is a parent, grandparent, etc. of the specified vertex.
        /// For descendants, a vertex is selected if it is a child, grandchild, etc. of the specified vertex.
        /// </summary>
        public HierarchicalDirectionType Direction { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="SelectHierarchicalOperation"/> operation.
    /// </summary>
    public struct SelectHierarchicalOperationOut
    {
        /// <summary>
        /// The resulting graph containing hierarchical vertices.
        /// </summary>
        [FieldName("Graph")]
        public Graph Graph { get; set; }
    }

    /// <summary>
    /// Filters the vertices of a graph to only include the hierarchical ancestors or descendants of a specified vertex.
    /// </summary>
    [OperationName(Display = "Select Hierarchical Vertices", Type = "selectHierarchical")]
    [OperationTag(OperationTags.Graph)]
    [OperationSelector("hierarchical")]
    public class SelectHierarchicalOperation : TypedOperation
    <
        SelectHierarchicalOperationIn,
        SelectHierarchicalOperationOut
    >,
    ISelector<Graph, Graph>
    {
        /// <inheritdoc />
        public Type ParameterType => typeof(SelectHierarchicalParameters);

        /// <summary>
        /// Provides overrided functionality for graphs based on this selector.
        /// </summary>
        private class HierarchicalComponent :
            IComponent,
            IEnumerableComponent<Vertex, Edge>,
            IDynamicLocalComponent<Vertex, Edge>,
            // TODO: IDynamicInComponent<IVertex, IEdge>,
            // TODO: IDynamicOutComponent<IVertex, IEdge>,
            IRootedComponent
        {
            /// <summary>
            /// Temporary storage for retrieved vertices.
            /// </summary>
            private readonly HashSet<string> _ids = new();

            /// <inheritdoc />
            public ComponentStack Components { get; set; }

            /// <summary>
            /// The identifier to select the hierarchy of.
            /// </summary>
            public Vertex Vertex { get; set; }
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
            public HierarchicalTraversalType Traversal { get; set; }
            /// <summary>
            /// The direction of the hierarchy to select.
            /// </summary>
            public HierarchicalDirectionType Direction { get; set; }

            /// <summary>
            /// Determines whether the specified vertex identifier is contained as a ancestor of the specified vertex.
            /// </summary>
            /// <param name="id">The vertex identifier to search for.</param>
            /// <param name="vertex">The specified root vertex.</param>
            /// <param name="depth">The current depth of the search.</param>
            /// <param name="dynamicInComponent">The component used to get in-vertices.</param>
            /// <returns><c>true</c> if the vertex identifier occurs as a ancestor; <c>false</c> otherwise.</returns>
            public async Task<bool> ContainsAncestorVertex(
                string id,
                Vertex vertex,
                int depth,
                IDynamicInComponent<Vertex, Edge> dynamicInComponent)
            {
                // Check if we have exceeded the depth.
                if (!Depth.HasValue || depth > Depth.Value) return false;

                // Check if the vertex we are considering is the root vertex.
                if (vertex.Id == id)
                    return depth != 0 || IncludeRoots;

                // Recursively check up the ancestor hierarchy for the specified vertex. 
                _ids.Add(vertex.Id);
                await foreach (Vertex parentVertex in dynamicInComponent.GetParentVertices(vertex.Id))
                {
                    if (!_ids.Contains(parentVertex.Id) &&
                        await ContainsAncestorVertex(id, parentVertex, depth + 1, dynamicInComponent)
                    ) return true;
                }
                return false;
            }
            /// <summary>
            /// Determines whether the specified vertex identifier is contained as a descendant of the specified vertex.
            /// </summary>
            /// <param name="id">The vertex identifier to search for.</param>
            /// <param name="vertex">The specified root vertex.</param>
            /// <param name="depth">The current depth of the search.</param>
            /// <param name="dynamicOutComponent">The component used to get out-vertices.</param>
            /// <returns><c>true</c> if the vertex identifier occurs as a descendant; <c>false</c> otherwise.</returns>
            public async Task<bool> ContainsDescendantVertex(
                string id,
                Vertex vertex,
                int depth,
                IDynamicOutComponent<Vertex, Edge> dynamicOutComponent)
            {
                // Check if we have exceeded the depth.
                if (!Depth.HasValue || depth > Depth.Value) return false;

                // Check if the vertex we are considering is the root vertex.
                if (vertex.Id == id)
                    return depth != 0 || IncludeRoots;

                // Recursively check down the descendant hierarchy for the specified vertex. 
                _ids.Add(vertex.Id);
                await foreach (Vertex childVertex in dynamicOutComponent.GetChildVertices(vertex.Id))
                {
                    if (!_ids.Contains(childVertex.Id) &&
                        await ContainsDescendantVertex(id, childVertex, depth + 1, dynamicOutComponent)
                    ) return true;
                }
                return false;
            }

            /// <summary>
            /// Gets all of the ancestors of the specified vertex up to a specified depth.
            /// </summary>
            /// <param name="vertex">The base vertex.</param>
            /// <param name="depth">The depth to fetch.</param>
            /// <param name="dynamicInComponent">The dynamic in component used to fetch parent vertices.</param>
            /// <returns>An enumeration of ancestor vertices.</returns>
            public async IAsyncEnumerable<Vertex> GetAncestors(
                Vertex vertex,
                int? depth,
                IDynamicInComponent<Vertex, Edge> dynamicInComponent)
            {
                // Emit base vertex for preorder traversal.
                if (vertex is not null && Traversal == HierarchicalTraversalType.Preorder)
                    if (Depth != depth || IncludeRoots)
                        yield return vertex;

                // Only continue traversal if we haven't already visited this vertex and we haven't surpassed the
                // specified depth.
                string id = vertex.Id;
                if (!_ids.Contains(id) && (depth is null || depth > 0))
                {
                    // Fetch the parent vertices of the base vertex and enumerate their ancestors.
                    _ids.Add(id);
                    await foreach (Vertex parentVertex in dynamicInComponent.GetParentVertices(id))
                    {
                        // Check if the parent vertex has already been retrieved before traversing it.
                        if (_ids.Contains(parentVertex.Id)) continue;

                        await foreach (
                            Vertex ancestorVertex in
                            GetAncestors(parentVertex, depth - 1, dynamicInComponent)
                        ) yield return ancestorVertex;
                    }
                }

                // Emit base vertex for postorder traversal.
                if (vertex is not null && Traversal == HierarchicalTraversalType.Postorder)
                    if (Depth != depth || IncludeRoots)
                        yield return vertex;
            }
            /// <summary>
            /// Gets all of the descendants of the specified vertex up to a specified depth.
            /// </summary>
            /// <param name="vertex">The base vertex.</param>
            /// <param name="depth">The depth to fetch.</param>
            /// <param name="dynamicOutComponent">The dynamic out component used to fetch child vertices.</param>
            /// <returns>An enumeration of descendant vertices.</returns>
            public async IAsyncEnumerable<Vertex> GetDescendants(
                Vertex vertex,
                int? depth,
                IDynamicOutComponent<Vertex, Edge> dynamicOutComponent)
            {
                // Emit base vertex for preorder traversal.
                if (vertex is not null && Traversal == HierarchicalTraversalType.Preorder)
                    if (Depth != depth || IncludeRoots)
                        yield return vertex;

                // Only continue traversal if we haven't already visited this vertex and we haven't surpassed the
                // specified depth.
                string id = vertex.Id;
                if (!_ids.Contains(id) && (depth is null || depth > 0))
                {
                    // Fetch the child vertices of the base vertex and enumerate their descendants.
                    _ids.Add(id);
                    await foreach (Vertex childVertex in dynamicOutComponent.GetChildVertices(id))
                    {
                        // Check if the child vertex has already been retrieved before traversing it.
                        if (_ids.Contains(childVertex.Id)) continue;

                        await foreach (
                            Vertex descendantVertex in
                            GetDescendants(childVertex, depth - 1, dynamicOutComponent)
                        ) yield return descendantVertex;
                    }
                }

                // Emit base vertex for postorder traversal.
                if (vertex is not null && Traversal == HierarchicalTraversalType.Postorder)
                    if (Depth != depth || IncludeRoots)
                        yield return vertex;
            }

            /// <inheritdoc />
            public async ITask<Vertex> GetVertex(string id)
            {
                // We make sure to clear out the algorithm storage at the start of any component method.
                _ids.Clear();
                if (Direction == HierarchicalDirectionType.Ancestors)
                {
                    if (Components.TryFind(out IDynamicInComponent<Vertex, Edge> dynamicInComponent) &&
                        await ContainsAncestorVertex(id, Vertex, 0, dynamicInComponent))
                    {
                        if (Components.TryFind(out IDynamicLocalComponent<Vertex, Edge> dynamicLocalComponent))
                            return await dynamicLocalComponent.GetVertex(id);
                    }
                }
                if (Direction == HierarchicalDirectionType.Descendants)
                {
                    if (Components.TryFind(out IDynamicOutComponent<Vertex, Edge> dynamicOutComponent) &&
                        await ContainsDescendantVertex(id, Vertex, 0, dynamicOutComponent))
                    {
                        if (Components.TryFind(out IDynamicLocalComponent<Vertex, Edge> dynamicLocalComponent))
                            return await dynamicLocalComponent.GetVertex(id);
                    }
                }
                return null;
            }

            /// <inheritdoc />
            public async IAsyncEnumerable<Vertex> GetVertices()
            {
                // We make sure to clear out the algorithm storage at the start of any component method.
                _ids.Clear();
                switch (Direction)
                {
                    case HierarchicalDirectionType.Ancestors:
                        if (Components.TryFind(out IDynamicInComponent<Vertex, Edge> dynamicInComponent))
                        {
                            await foreach (Vertex vertex in GetAncestors(Vertex, Depth, dynamicInComponent))
                                yield return vertex;
                        }
                        else
                            throw new InvalidCastException("Graph does not contain a dynamic in component.");
                        break;
                    case HierarchicalDirectionType.Descendants:
                        if (Components.TryFind(out IDynamicOutComponent<Vertex, Edge> dynamicOutComponent))
                        {
                            await foreach (Vertex vertex in GetDescendants(Vertex, Depth, dynamicOutComponent))
                                yield return vertex;
                        }
                        else
                            throw new InvalidCastException("Graph does not contain a dynamic out component.");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            /// <inheritdoc />
            public IAsyncEnumerable<string> Roots()
            {
                if (IncludeRoots)
                    return new[] { Vertex.Id }.ToAsyncEnumerable();
                else
                    throw new ArgumentException("Roots cannot be retrieved when excluded.");
            }
        }

        /// <inheritdoc />
        public override Task<SelectHierarchicalOperationOut> Perform(SelectHierarchicalOperationIn input)
        {
            // TODO: We need to do a simple clone of graphs between operations to avoid this modification affecting other operations.
            // Apply the new components to the graph.
            Graph graph = input.Graph;
            HierarchicalComponent component = new()
            {
                Vertex = input.Vertex,
                Depth = input.Depth,
                Direction = input.Direction,
                Traversal = input.Traversal,
                IncludeRoots = input.IncludeRoots
            };

            // Branch the component stack.
            graph.Components = graph.Components.Branch();

            // Rooted components.
            if (input.IncludeRoots) graph.Components.Append<IRootedComponent>(component);
            else graph.Components.Remove<IRootedComponent>();

            // Dynamic and enumerable components.
            if (input.Direction == HierarchicalDirectionType.Ancestors)
            {
                if (graph.Components.TryFind<IDynamicInComponent<Vertex, Edge>>(out _))
                {
                    graph.Components.Append<IEnumerableComponent<Vertex, Edge>>(component);
                    // TODO: graph.Components.AddTop<IDynamicInComponent<IVertex, IEdge>>(component);
                    if (graph.Components.TryFind<IDynamicLocalComponent<Vertex, Edge>>(out _))
                        graph.Components.Append<IDynamicLocalComponent<Vertex, Edge>>(component);
                }
            }
            if (input.Direction == HierarchicalDirectionType.Descendants)
            {
                if (graph.Components.TryFind<IDynamicOutComponent<Vertex, Edge>>(out _))
                {
                    graph.Components.Append<IEnumerableComponent<Vertex, Edge>>(component);
                    // TODO: graph.Components.AddTop<IDynamicOutComponent<IVertex, IEdge>>(component);
                    if (graph.Components.TryFind<IDynamicLocalComponent<Vertex, Edge>>(out _))
                        graph.Components.Append<IDynamicLocalComponent<Vertex, Edge>>(component);
                }
            }

            // Create a new graph to store the descendants.
            return Task.FromResult(new SelectHierarchicalOperationOut { Graph = graph });
        }

        /// <inheritdoc />
        public async Task<Graph> Select(Graph source, object parameters)
        {
            if (parameters is SelectHierarchicalParameters typedParameters)
            {
                // We need to get the vertex from the source graph.
                Vertex vertex;
                if (source.Components.TryFind(out IDynamicLocalComponent<Vertex, Edge> dynamicComponent))
                    vertex = await dynamicComponent.GetVertex(typedParameters.Id);
                else
                    throw new InvalidOperationException("The source graph does not support dynamic local vertices.");

                // We rely on the base operation functionality to perform the selection.
                return (await Perform(new SelectHierarchicalOperationIn
                {
                    Graph = source,
                    Vertex = vertex,
                    IncludeRoots = typedParameters.IncludeRoots,
                    Depth = typedParameters.Depth,
                    Traversal = typedParameters.Traversal
                })).Graph;
            }
            throw new InvalidCastException();
        }
    }
}