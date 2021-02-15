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
    public class HyperthoughtWorkflowGraph : FreeformGraph
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
            IList<FreeformProperty> properties = new List<FreeformProperty>();
            foreach (HyperthoughtMetadata metadata in workflow.Metadata)
            {
                HyperthoughtMetadataValue value = metadata.Value;
                properties.Add(new FreeformProperty(FreeformIdentity.Create(metadata.Key))
                {
                    Observations = new FreeformObservation[]
                    {
                        new FreeformObservation
                        {
                            Type = value.Type.ToString(),
                            Value = value.Link
                        }
                    }
                });
            }

            FreeformVertex vertex = new FreeformVertex(FreeformIdentity.Create(workflow.Content.PrimaryKey))
            {
                Label = workflow.Content.Name,
                Description = workflow.Content.Notes,
                Properties = properties
            };
            return vertex;
        }
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
                    FreeformIdentity.Create(edge++)
                );
            }

            // Yield all successors.
            foreach (Guid successorId in workflow.Content.SuccessorIds.OrderBy(id => id))
            {
                yield return new FreeformEdge
                (
                    FreeformIdentity.Create(workflow.Content.PrimaryKey),
                    FreeformIdentity.Create(successorId),
                    FreeformIdentity.Create(edge++)
                );
            }
        }

        #region FreeformGraph
        /// <inheritdoc />
        public override bool IsFinite => false;
        /// <inheritdoc />
        public override bool IsDirected => true;
        /// <inheritdoc />
        public override bool AllowParallelEdges => false;

        /// <inheritdoc />
        public Guid BaseId => Id;

        /// <inheritdoc />
        public override bool IsVerticesEmpty => false;
        /// <inheritdoc />
        public override bool IsEdgesEmpty
        {
            get
            {
                HyperthoughtWorkflow workflow = Task.Run(() => Api.GetWorkflowAsync(BaseId)).GetAwaiter().GetResult();
                return EdgesFromWorkflow(workflow).Any();
            }
        }

        /// <inheritdoc />
        public override int VertexCount => Vertices.Count();
        /// <inheritdoc />
        public override int EdgeCount => Edges.Count();

        private IEnumerable<FreeformVertex> GetDescendantVertices(Guid id)
        {
            // Return the postorder traversal of the hierarchy.
            HyperthoughtWorkflow workflow = Task.Run(() => Api.GetWorkflowAsync(id)).GetAwaiter().GetResult();
            foreach (Guid childId in workflow.Content.ChildrenIds.OrderBy(id => id))
            {
                foreach (FreeformVertex vertex in GetDescendantVertices(childId))
                    yield return vertex;
            }
            yield return VertexFromWorkflow(workflow);
        }
        private IEnumerable<FreeformEdge> GetDescendantEdges(Guid id)
        {
            // Return the postorder traversal of the hierarchy.
            HyperthoughtWorkflow workflow = Task.Run(() => Api.GetWorkflowAsync(id)).GetAwaiter().GetResult();
            foreach (Guid childId in workflow.Content.ChildrenIds.OrderBy(id => id))
            {
                foreach (FreeformEdge edge in GetDescendantEdges(childId))
                    yield return edge;

            }
            foreach (FreeformEdge edge in EdgesFromWorkflow(workflow))
                yield return edge;
        }

        /// <inheritdoc />
        public override IEnumerable<FreeformVertex> Vertices => GetDescendantVertices(BaseId);
        /// <inheritdoc />
        public override IEnumerable<FreeformEdge> Edges => GetDescendantEdges(BaseId);

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
            int edge = 0;
            foreach (Guid childId in workflow.Content.ChildrenIds.OrderBy(id => id))
            {
                yield return new FreeformEdge(
                    FreeformIdentity.Create(id),
                    FreeformIdentity.Create(childId),
                    FreeformIdentity.Create(edge++)
                );
            }
        }

        /// <inheritdoc />
        public override bool ContainsVertex(FreeformVertex vertex)
        {
            try
            {
                if (FreeformIdentity.IsType(vertex.Identifier, out Guid id))
                {
                    HyperthoughtWorkflow workflow = Task.Run(() => Api.GetWorkflowAsync(id)).GetAwaiter().GetResult();
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
                if (FreeformIdentity.IsType(edge.Source.Identifier, out Guid sourceId) &&
                    FreeformIdentity.IsType(edge.Target.Identifier, out Guid targetId))
                {
                    HyperthoughtWorkflow workflow = Task.Run(() => Api.GetWorkflowAsync(sourceId)).GetAwaiter().GetResult();
                    return workflow.Content.ChildrenIds.Contains(targetId);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        #endregion FreeformGraph
    }
}