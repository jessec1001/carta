using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using QuikGraph;

using CartaCore.Data;
using CartaCore.Integration.Hyperthought.Data;
using CartaCore.Utility;

namespace CartaCore.Integration.Hyperthought
{
    using FreeformGraph = IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge>;

    /// <summary>
    /// Represents a sampled graph generated from data contained in a HyperThought workflow.
    /// </summary>
    public class HyperthoughtWorkflowGraph : ISampledGraph
    {
        private readonly HyperthoughtApi Api;
        private readonly Guid Id;

        /// <summary>
        /// Initializes a new instance of the <see cref="HyperthoughtWorkflowGraph"/> class using a
        /// <see cref="HyperthoughtApi"/> instance and a workflow ID.
        /// </summary>
        /// <param name="api">The HyperThought API.</param>
        /// <param name="id">The ID of the workflow.</param>
        public HyperthoughtWorkflowGraph(HyperthoughtApi api, Guid id)
        {
            Api = api;
            Id = id;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="HyperthoughtWorkflowGraph"/> class using a
        /// <see cref="HyperthoughtApi"/> instance and a workflow.
        /// </summary>
        /// <param name="api">The HyperThought API.</param>
        /// <param name="workflow">The workflow.</param>
        public HyperthoughtWorkflowGraph(HyperthoughtApi api, HyperthoughtWorkflow workflow)
            : this(api, workflow.Content.PrimaryKey) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="HyperthoughtWorkflowGraph"/> class using a
        /// <see cref="HyperthoughtApi"/> instance and a workflow template.
        /// </summary>
        /// <param name="api">The HyperThought API.</param>
        /// <param name="template">The workflow template.</param>
        public HyperthoughtWorkflowGraph(HyperthoughtApi api, HyperthoughtWorkflowTemplate template)
            : this(api, template.PrimaryKey) { }

        private FreeformVertex VertexFromWorkflow(HyperthoughtWorkflow workflow)
        {
            SortedList<string, FreeformProperty> properties = new SortedList<string, FreeformProperty>();
            foreach (HyperthoughtMetadata metadata in workflow.Metadata)
            {
                HyperthoughtMetadataValue value = metadata.Value;
                properties.Add(metadata.Key, new FreeformProperty
                {
                    Type = value.Type.ToString().TypeDeserialize(),
                    Value = value.Link
                });
            }

            FreeformVertex vertex = new FreeformVertex(workflow.Content.PrimaryKey)
            {
                Label = workflow.Content.Name,
                Description = workflow.Content.Notes,
                Properties = properties
            };
            return vertex;
        }
        private IEnumerable<FreeformEdge> EdgesFromWorkflow(HyperthoughtWorkflow workflow)
        {
            FreeformVertex workflowVertex = new FreeformVertex(workflow.Content.PrimaryKey);
            int edge = 0;

            // Yield all children.
            foreach (Guid childId in workflow.Content.ChildrenIds.OrderBy(id => id))
            {
                yield return new FreeformEdge
                (
                    workflowVertex,
                    new FreeformVertex(childId),
                    edge++
                );
            }

            // Yield all successors.
            foreach (Guid successorId in workflow.Content.SuccessorIds.OrderBy(id => id))
            {
                yield return new FreeformEdge
                (
                    workflowVertex,
                    new FreeformVertex(successorId),
                    edge++
                );
            }
        }

        /// <inheritdoc />
        public bool IsFinite => false;
        /// <inheritdoc />
        public bool IsDirected => true;
        /// <inheritdoc />
        public Guid BaseId => Id;

        /// <inheritdoc />
        public FreeformGraph GetEntire() => throw new NotFiniteNumberException();

        /// <inheritdoc />
        public FreeformVertex GetProperties(Guid id)
        {
            // Get the workflow properties from an API call.
            HyperthoughtWorkflow workflow = Task.Run(() => Api.GetWorkflowAsync(id)).GetAwaiter().GetResult();
            return VertexFromWorkflow(workflow);
        }
        /// <inheritdoc />
        public IEnumerable<FreeformEdge> GetEdges(Guid id)
        {
            // Get the workflow from an API call and yield each of the children.
            HyperthoughtWorkflow workflow = Task.Run(() => Api.GetWorkflowAsync(id)).GetAwaiter().GetResult();
            FreeformVertex thisVertex = new FreeformVertex(id);
            int edge = 0;
            foreach (Guid childId in workflow.Content.ChildrenIds.OrderBy(id => id))
            {
                yield return new FreeformEdge(
                    thisVertex,
                    new FreeformVertex(childId),
                    edge++
                );
            }
        }
        /// <inheritdoc />
        public FreeformGraph GetChildren(Guid id)
        {
            // Get the child workflows from an API call and yield each by ID.
            IList<HyperthoughtWorkflow> workflows = Task.Run(() => Api.GetWorkflowChildrenAsync(id)).GetAwaiter().GetResult();

            // Create a graph with the workflow vertices.
            FreeformGraph graph = new AdjacencyGraph<FreeformVertex, FreeformEdge>(true);
            graph.AddVertexRange
            (
                workflows.Select(workflow => VertexFromWorkflow(workflow))
            );
            return graph;
        }
        /// <inheritdoc />
        public FreeformGraph GetChildrenWithEdges(Guid id)
        {
            // Get the child workflows from an API call and yield each by ID.
            IList<HyperthoughtWorkflow> workflows = Task.Run(() => Api.GetWorkflowChildrenAsync(id)).GetAwaiter().GetResult();

            // Create a graph.
            FreeformGraph graph = new AdjacencyGraph<FreeformVertex, FreeformEdge>(true);
            FreeformVertex vertex = new FreeformVertex(id);

            // Add parent and children vertices.
            graph.AddVertexRange
            (
                workflows.Select(workflow => VertexFromWorkflow(workflow))
            );

            // Add parent-to-children edges.
            int edges = 0;
            graph.AddVerticesAndEdgeRange
            (
                workflows
                    .OrderBy(workflow => workflow.Content.PrimaryKey)
                    .Select
                    (
                        workflow => new FreeformEdge
                        (
                            vertex,
                            new FreeformVertex(workflow.Content.PrimaryKey),
                            edges++
                        )
                    )
            );

            // Add children edges.
            graph.AddVerticesAndEdgeRange
            (
                workflows.SelectMany(workflow => EdgesFromWorkflow(workflow))
            );

            return graph;
        }
    }
}