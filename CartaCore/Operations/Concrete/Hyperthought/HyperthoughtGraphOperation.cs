using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Graphs.Components;
using CartaCore.Integration.Hyperthought;
using CartaCore.Integration.Hyperthought.Api;
using CartaCore.Operations.Attributes;
using CartaCore.Operations.Authentication;

namespace CartaCore.Operations
{
    // TODO: Add a parameter for whether to add metadata or not.
    // TODO: Try to allow the user to specify the path from an enumeration.
    /// <summary>
    /// The input for the <see cref="HyperthoughtGraphOperation" /> operation.
    /// </summary>
    public struct HyperthoughtGraphOperationIn
    {
        /// <summary>
        /// The reference to the authenticated HyperThought API. 
        /// </summary>
        [FieldAuthentication(HyperthoughtAuthentication.Key, typeof(HyperthoughtAuthentication))]
        public HyperthoughtAuthentication HyperthoughtAuth { get; set; }
        /// <summary>
        /// The dot-separated path to the HyperThought workflow.
        /// </summary>
        [FieldRequired]
        [FieldName("Workflow Path")]
        [FieldUiWidget(OperationUiWidgets.Strings.Resource)]
        [FieldUiExtension("filter", "source:hyperthought")]
        public string Path { get; set; }
    }
    /// <summary>
    /// The output of the <see cref="HyperthoughtGraphOperation" /> operation.
    /// </summary>
    public struct HyperthoughtGraphOperationOut
    {
        /// <summary>
        /// A graph representing the HyperThought workflow.
        /// </summary>
        [FieldName("Workflow Graph")]
        public HyperthoughtWorkflowGraph Graph { get; set; }
    }

    /// <summary>
    /// Loads a workflow from HyperThought as a graph structure.
    /// </summary>
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
        private class PrioritizeHyperthoughtWorkflowGraph : IEnumerableComponent<Vertex, Edge>
        {
            /// <summary>
            /// The base HyperThought graph to modify. 
            /// </summary>
            public Graph Graph { get; set; }
            /// <inheritdoc />
            public ComponentStack Components { get; set; }

            /// <summary>
            /// Contains a priority queue containing prioritized selectors for the HyperThought workflow graph.
            /// This may be null in which case, there are no prioritizations.
            /// </summary>
            private readonly ConcurrentQueue<(object, object)> _queue;

            /// <summary>
            /// Initializes a new instance of the <see cref="PrioritizeHyperthoughtWorkflowGraph"/> class.
            /// </summary>
            /// <param name="queue">The priority queue that should be used.</param>
            public PrioritizeHyperthoughtWorkflowGraph(ConcurrentQueue<(object, object)> queue) => _queue = queue;


            /// <inheritdoc />
            public async IAsyncEnumerable<Vertex> GetVertices()
            {
                // We make sure to not repeat any vertices.
                // We execute a traversal algorithm over the graph while constantly checking the priority queue.
                // - The completed IDs contains all vertices that have been loaded.
                // - The pending IDs contains all vertices that have not been loaded yet or have children that have not been loaded yet.
                HashSet<Guid> completedIds = new();
                Queue<Guid> pendingIds = new();

                // We get the roots from the rooted component.
                if (Components.TryFind(out IRootedComponent rootedComponent))
                {
                    await foreach (string rootId in rootedComponent.Roots())
                        pendingIds.Enqueue(Guid.Parse(rootId));
                }
                if (!Components.TryFind(out IDynamicLocalComponent<Vertex, Edge> dynamicLocalComponent))
                    yield break;
                if (!Components.TryFind(out IDynamicOutComponent<Vertex, Edge> dynamicOutComponent))
                    yield break;

                // Continue fetching data until we have completed all the nodes.
                while (pendingIds.Count > 0)
                {
                    // Check if there are any selectors in the priority queue.
                    while (_queue is not null && !_queue.IsEmpty)
                    {
                        // Get the next selector.
                        if (!_queue.TryDequeue(out (object selector, object parameter) item)) continue;
                        if (item.selector is not ISelector<Graph, Graph> selector) continue;

                        // Get the vertices in the selector.
                        Graph selectorGraph = await selector.Select(Graph, item.parameter);
                        if (selectorGraph.Components.TryFind(out IEnumerableComponent<Vertex, Edge> enumerableComponent))
                        {
                            await foreach (Vertex vertex in enumerableComponent.GetVertices())
                            {
                                Guid vertexId = Guid.Parse(vertex.Id);
                                if (completedIds.Contains(vertexId)) continue;
                                yield return vertex;
                            }
                        }
                    }

                    // Pop the next identifier.
                    Guid id = pendingIds.Dequeue();

                    // Check if we have already completed this node. If not, load the node.
                    if (!completedIds.Contains(id))
                    {
                        // Get the vertex.
                        string currentId = id.ToString();
                        Vertex vertex = await dynamicLocalComponent.GetVertex(currentId);
                        if (vertex is null) continue;

                        // Add the vertex to the completed list.
                        completedIds.Add(id);
                        yield return vertex;
                    }

                    // Get the vertex children.
                    await foreach (Vertex childVertex in dynamicOutComponent.GetChildVertices(id.ToString()))
                    {
                        // If the child is not already completed, add it to the pending list.
                        Guid childId = Guid.Parse(childVertex.Id);
                        if (!completedIds.Contains(childId))
                        {
                            pendingIds.Enqueue(childId);
                            completedIds.Add(childId);
                            yield return childVertex;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public override async Task<HyperthoughtGraphOperationOut> Perform(HyperthoughtGraphOperationIn input, OperationJob job)
        {
            // Get the UUID of the graph so that we may create the graph.
            HyperthoughtApi api = new(input.HyperthoughtAuth.ApiKey);
            Guid uuid = await api.Workflow.GetProcessIdFromPathAsync(input.Path);
            HyperthoughtWorkflowGraph graph = new(api, uuid);

            // Try to get the priority queue for the graph field.
            job.PriorityQueue.TryGetValue(
                nameof(HyperthoughtGraphOperationOut.Graph),
                out ConcurrentQueue<(object, object)> priorityQueue);

            // We need to patch the graph so that it has prioritization.
            graph.Components = graph.Components
                .Branch()
                .Append(new PrioritizeHyperthoughtWorkflowGraph(priorityQueue) { Graph = graph });

            return new HyperthoughtGraphOperationOut { Graph = graph };
        }
    }
}