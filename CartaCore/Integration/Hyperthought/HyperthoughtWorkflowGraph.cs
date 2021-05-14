using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MorseCode.ITask;

using CartaCore.Data;
using CartaCore.Integration.Hyperthought.Data;

namespace CartaCore.Integration.Hyperthought
{
    /// <summary>
    /// Represents a sampled graph generated from data contained in a HyperThought workflow.
    /// </summary>
    public class HyperthoughtWorkflowGraph : Graph,
        IRootedGraph,
        IDynamicInGraph<InOutVertex>,
        IDynamicOutGraph<InOutVertex>
    {
        /// <summary>
        /// The connector to the HyperThought API. 
        /// </summary>
        private readonly HyperthoughtApi Api;
        /// <summary>
        /// The GUID of the HyperThought workflow object. 
        /// </summary>
        private readonly Guid Id;

        /// <summary>
        /// Initializes a new instance of the <see cref="HyperthoughtWorkflowGraph"/> class using a
        /// <see cref="HyperthoughtApi"/> instance and a workflow ID.
        /// </summary>
        /// <param name="api">The HyperThought API.</param>
        /// <param name="id">The ID of the workflow.</param>
        public HyperthoughtWorkflowGraph(HyperthoughtApi api, Guid id)
            : base(Identity.Create(nameof(HyperthoughtWorkflowGraph)))
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

        /// <summary>
        /// Converts a HyperThought workflow object into a vertex.
        /// </summary>
        /// <param name="workflow">The workflow object.</param>
        /// <returns>The converted vertex.</returns>
        private InOutVertex VertexFromWorkflow(HyperthoughtWorkflow workflow)
        {
            // We find the properties for the vertex from the metadata.
            List<Property> properties = new List<Property>(workflow.Metadata.Count);
            foreach (HyperthoughtMetadata metadata in workflow.Metadata)
            {
                // Properties may have multiple observations which we need to account for.
                // We search for the property by key in our current properties.
                HyperthoughtMetadataValue value = metadata.Value;
                Property property = null;
                property = properties.FirstOrDefault
                (
                    prop => (prop.Identifier.IsType(out string typedId) && typedId == metadata.Key)
                );

                // We create a new property if necessary.
                if (property is null)
                {
                    property = new Property(Identity.Create(metadata.Key), new List<object>());
                    properties.Add(property);
                }

                // We add the observation afterwards.
                List<object> values = property.Values as List<object>;
                values.Add(value.Link);
            }
            properties.TrimExcess();

            // Get the identifier of this vertex.
            Identity id = Identity.Create(workflow.Content.PrimaryKey);

            // Add the parent and predecessors to in-edges.
            int inEdgeCount = 1 + workflow.Content.PredecessorIds.Count;
            IList<Edge> inEdges = new List<Edge>(capacity: inEdgeCount);

            inEdges.Add(new Edge(Identity.Create(workflow.Content.ParentProcessId ?? Guid.Empty), id));
            foreach (Guid predecessorId in workflow.Content.PredecessorIds.OrderBy(id => id))
                inEdges.Add(new Edge(Identity.Create(predecessorId), id));

            // Add all children and successors to out-edges.
            int outEdgeCount = workflow.Content.ChildrenIds.Count + workflow.Content.SuccessorIds.Count;
            IList<Edge> outEdges = new List<Edge>(capacity: outEdgeCount);

            foreach (Guid childId in workflow.Content.ChildrenIds.OrderBy(id => id))
                outEdges.Add(new Edge(id, Identity.Create(childId)));
            foreach (Guid successorId in workflow.Content.SuccessorIds.OrderBy(id => id))
                outEdges.Add(new Edge(id, Identity.Create(successorId)));

            // Create the vertex with the name and notes as labels and description respectively.
            InOutVertex vertex = new InOutVertex
            (
                Identity.Create(workflow.Content.PrimaryKey),
                properties, inEdges, outEdges
            )
            {
                Label = workflow.Content.Name,
                Description = workflow.Content.Notes
            };
            return vertex;
        }

        /// <inheritdoc />
        public override bool IsFinite => true;
        /// <inheritdoc />
        public override bool IsDirected => true;
        /// <inheritdoc />
        public override bool IsDynamic => true;

        /// <inheritdoc />
        public IEnumerable<Identity> GetRoots()
        {
            yield return Identity.Create(Id);
        }

        /// <inheritdoc />
        public async ITask<InOutVertex> GetVertex(Identity id)
        {
            // Check that the identifier is of the correct type first.
            if (!id.IsType(out Guid guid)) throw new InvalidCastException();

            // Get the workflow asynchronously and return the vertex created from it.
            HyperthoughtWorkflow workflow = await Api.GetWorkflowAsync(guid);
            return VertexFromWorkflow(workflow);
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<InOutVertex> GetVertices(IEnumerable<Identity> ids)
        {
            // Setup our data structures to store our identifiers and async tasks.
            List<Identity> idList = ids.ToList();
            Task<HyperthoughtWorkflow>[] workflowTasks = new Task<HyperthoughtWorkflow>[idList.Count];

            // Launch tasks to get all of the requested workflows.
            for (int k = 0; k < idList.Count; k++)
            {
                if (!idList[k].IsType(out Guid guid)) throw new InvalidCastException();
                workflowTasks[k] = Api.GetWorkflowAsync(guid);
            }

            // Return vertices for each workflow in the order they were requested.
            for (int k = 0; k < idList.Count; k++)
                yield return VertexFromWorkflow(await workflowTasks[k]);
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<InOutVertex> GetParentVertices(Identity id)
        {
            InOutVertex vertex = await GetVertex(id);
            foreach (Edge inEdge in vertex.InEdges)
                yield return await GetVertex(inEdge.Target);
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<InOutVertex> GetChildVertices(Identity id)
        {
            // Check that the identifier is of the correct type first.
            if (!id.IsType(out Guid guid)) throw new InvalidCastException();

            // Get the workflows asynchronously and return the vertices created from them.
            IList<HyperthoughtWorkflow> workflows = await Api.GetWorkflowChildrenAsync(guid);
            foreach (HyperthoughtWorkflow workflow in workflows)
                yield return VertexFromWorkflow(workflow);
        }

        /// <summary>
        /// Ensures that the graph is valid and authorized. Throws an exception if not.
        /// </summary>
        public async Task EnsureValidity()
        {
            await Api.GetWorkflowAsync(Id);
        }
    }
}