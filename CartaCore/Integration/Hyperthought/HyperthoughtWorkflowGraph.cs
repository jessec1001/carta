using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CartaCore.Data.Freeform;
using CartaCore.Integration.Hyperthought.Data;

namespace CartaCore.Integration.Hyperthought
{
    /// <summary>
    /// Represents a sampled graph generated from data contained in a HyperThought workflow.
    /// </summary>
    public class HyperthoughtWorkflowGraph : FreeformDynamicGraph<Guid>
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
        /// Gets a HyperThought workflow synchronously.
        /// </summary>
        /// <param name="id">The identifier for the HyperThought workflow.</param>
        /// <returns>The HyperThought workflow.</returns>
        private HyperthoughtWorkflow GetWorkflowSync(Guid id)
        {
            return Task.Run(() => Api.GetWorkflowAsync(id)).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Gets the children workflows of a HyperThought workflow synchronously.
        /// </summary>
        /// <param name="id">The identifier for the HyperThought parent workflow.</param>
        /// <returns>The HyperThought children workflows.</returns>
        private IEnumerable<HyperthoughtWorkflow> GetWorkflowChildrenSync(Guid id)
        {
            return Task.Run(() => Api.GetWorkflowChildrenAsync(id)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Converts a HyperThought workflow object into a vertex.
        /// </summary>
        /// <param name="workflow">The workflow object.</param>
        /// <returns>The converted vertex.</returns>
        private FreeformVertex VertexFromWorkflow(HyperthoughtWorkflow workflow)
        {
            // We find the properties for the vertex from the metadata.
            IList<FreeformProperty> properties = new List<FreeformProperty>();
            foreach (HyperthoughtMetadata metadata in workflow.Metadata)
            {
                // Properties may have multiple observations which we need to account for.
                // We search for the property by key in our current properties.
                HyperthoughtMetadataValue value = metadata.Value;
                FreeformProperty property = null;
                property = properties.First
                (
                    prop => (prop.Identifier.IsType(out string typedId) && typedId == metadata.Key)
                );

                // We create a new property if necessary.
                if (property is null)
                {
                    property = new FreeformProperty(FreeformIdentity.Create(metadata.Key))
                    {
                        Observations = new List<FreeformObservation>()
                    };
                    properties.Add(property);
                }

                // We add the observation afterwards.
                List<FreeformObservation> observations = property.Observations as List<FreeformObservation>;
                observations.Add
                (
                    new FreeformObservation
                    {
                        Type = value.Type.ToString(),
                        Value = value.Link
                    }
                );
            }

            // Create the vertex with the name and notes as labels and description respectively.
            FreeformVertex vertex = new FreeformVertex(FreeformIdentity.Create(workflow.Content.PrimaryKey))
            {
                Label = workflow.Content.Name,
                Description = workflow.Content.Notes,
                Properties = properties
            };
            return vertex;
        }
        /// <summary>
        /// Converts a HyperThought workflow object into an enumerable of edges.
        /// </summary>
        /// <param name="workflow">The workflow object.</param>
        /// <returns>The converted enumerable of edges.</returns>
        private IEnumerable<FreeformEdge> EdgesFromWorkflow(HyperthoughtWorkflow workflow)
        {
            // Yield all children.
            int edge = 0;
            foreach (Guid childId in workflow.Content.ChildrenIds.OrderBy(id => id))
            {
                yield return new FreeformEdge
                (
                    FreeformIdentity.Create(workflow.Content.PrimaryKey),
                    FreeformIdentity.Create(childId),
                    edge++
                );
            }

            // Yield all successors.
            foreach (Guid successorId in workflow.Content.SuccessorIds.OrderBy(id => id))
            {
                yield return new FreeformEdge
                (
                    FreeformIdentity.Create(workflow.Content.PrimaryKey),
                    FreeformIdentity.Create(successorId),
                    edge++
                );
            }
        }

        #region FreeformDynamicGraph
        /// <inheritdoc />
        public override bool IsFinite => true;
        /// <inheritdoc />
        public override bool IsDirected => true;
        /// <inheritdoc />
        public override FreeformIdentity BaseId => FreeformIdentity.Create(Id);
        /// <inheritdoc />
        public override bool AllowParallelEdges => false;

        /// <inheritdoc />
        public override bool IsVerticesEmpty => false;
        /// <inheritdoc />
        public override bool IsEdgesEmpty => EdgesFromWorkflow(GetWorkflowSync(Id)).Any();

        /// <inheritdoc />
        public override int VertexCount => Vertices.Count();
        /// <inheritdoc />
        public override int EdgeCount => Edges.Count();

        /// <inheritdoc />
        public override bool ContainsVertex(FreeformVertex vertex)
        {
            try
            {
                if (vertex.Identifier.IsType(out Guid id))
                {
                    HyperthoughtWorkflow workflow = GetWorkflowSync(id);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        /// <inheritdoc />
        public override bool ContainsEdge(FreeformEdge edge)
        {
            try
            {
                if (edge.Source.Identifier.IsType(out Guid sourceId) &&
                    edge.Target.Identifier.IsType(out Guid targetId))
                {
                    HyperthoughtWorkflow workflow = GetWorkflowSync(sourceId);
                    return workflow.Content.ChildrenIds.Contains(targetId);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override FreeformVertex GetVertex(Guid id)
        {
            // Get the workflow properties from an API call.
            return VertexFromWorkflow(GetWorkflowSync(id));
        }
        /// <inheritdoc />
        public override IEnumerable<FreeformEdge> GetEdges(Guid id)
        {
            // Get the workflow from an API call and yield each of the children.
            return EdgesFromWorkflow(GetWorkflowSync(id));
        }
        /// <inheritdoc />
        public override (FreeformVertex, IEnumerable<FreeformEdge>) GetVertexWithEdges(Guid id)
        {
            // This is an optimization over separate calls to get vertex and get edges.
            HyperthoughtWorkflow workflow = GetWorkflowSync(id);
            return (VertexFromWorkflow(workflow), EdgesFromWorkflow(workflow));
        }

        /// <inheritdoc />
        public override IEnumerable<FreeformVertex> GetChildVertices(Guid id)
        {
            foreach (HyperthoughtWorkflow workflow in GetWorkflowChildrenSync(id))
                yield return VertexFromWorkflow(workflow);
        }
        /// <inheritdoc />
        public override IEnumerable<FreeformEdge> GetChildEdges(Guid id)
        {
            foreach (HyperthoughtWorkflow workflow in GetWorkflowChildrenSync(id))
                foreach (FreeformEdge edge in EdgesFromWorkflow(workflow))
                    yield return edge;
        }
        /// <inheritdoc />
        public override (IEnumerable<FreeformVertex>, IEnumerable<FreeformEdge>) GetChildVerticesWithEdges(Guid id)
        {
            IEnumerable<HyperthoughtWorkflow> workflows = GetWorkflowChildrenSync(id);
            return
            (
                workflows.Select(workflow => VertexFromWorkflow(workflow)),
                workflows.SelectMany(workflow => EdgesFromWorkflow(workflow))
            );
        }

        /// <summary>
        /// Traverses the child vertices of the workflow graph in preorder fashion.
        /// </summary>
        /// <param name="id">The identifier of the common ancestor vertex.</param>
        /// <returns>A preorder enumerable of child vertices.</returns>
        public override IEnumerable<FreeformVertex> TraversePreorderVertices(Guid id)
        {
            // Return the postorder traversal of the hierarchy.
            HyperthoughtWorkflow workflow = GetWorkflowSync(id);
            yield return VertexFromWorkflow(workflow);
            foreach (Guid childId in workflow.Content.ChildrenIds.OrderBy(id => id))
            {
                foreach (FreeformVertex vertex in TraversePreorderVertices(childId))
                    yield return vertex;
            }
        }
        /// <summary>
        /// Traverses the child vertices of the workflow graph in postorder fashion.
        /// </summary>
        /// <param name="id">The identifier of the common ancestor vertex.</param>
        /// <returns>A postorder enumerable of child vertices.</returns>
        public override IEnumerable<FreeformVertex> TraversePostorderVertices(Guid id)
        {
            // Return the postorder traversal of the hierarchy.
            HyperthoughtWorkflow workflow = GetWorkflowSync(id);
            foreach (Guid childId in workflow.Content.ChildrenIds.OrderBy(id => id))
            {
                foreach (FreeformVertex vertex in TraversePostorderVertices(childId))
                    yield return vertex;
            }
            yield return VertexFromWorkflow(workflow);
        }
        /// <summary>
        /// Traverses the child edges of the workflow graph in preorder fashion.
        /// </summary>
        /// <param name="id">The identifier of the common ancestor vertex.</param>
        /// <returns>A preorder enumerable of the child edges.</returns>
        public override IEnumerable<FreeformEdge> TraversePreorderEdges(Guid id)
        {
            // Return the postorder traversal of the hierarchy.
            HyperthoughtWorkflow workflow = GetWorkflowSync(id);
            foreach (FreeformEdge edge in EdgesFromWorkflow(workflow))
                yield return edge;
            foreach (Guid childId in workflow.Content.ChildrenIds.OrderBy(id => id))
            {
                foreach (FreeformEdge edge in TraversePreorderEdges(childId))
                    yield return edge;

            }
        }
        /// <summary>
        /// Traverses the child edges of the workflow graph in postorder fashion.
        /// </summary>
        /// <param name="id">The identifier of the common ancestor vertex.</param>
        /// <returns>A postorder enumerable of the child edges.</returns>
        public override IEnumerable<FreeformEdge> TraversePostorderEdges(Guid id)
        {
            // Return the postorder traversal of the hierarchy.
            HyperthoughtWorkflow workflow = GetWorkflowSync(id);
            foreach (Guid childId in workflow.Content.ChildrenIds.OrderBy(id => id))
            {
                foreach (FreeformEdge edge in TraversePostorderEdges(childId))
                    yield return edge;

            }
            foreach (FreeformEdge edge in EdgesFromWorkflow(workflow))
                yield return edge;
        }
        #endregion
    }
}