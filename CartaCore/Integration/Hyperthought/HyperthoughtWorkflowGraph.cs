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
    using FreeformGraph = IMutableVertexAndEdgeSet<FreeformVertex, Edge<FreeformVertex>>;

    public class HyperthoughtWorkflowGraph : ISampledGraph
    {
        private readonly HyperthoughtApi Api;
        private readonly Guid Id;

        public HyperthoughtWorkflowGraph(HyperthoughtApi api, Guid id)
        {
            Api = api;
            Id = id;
        }
        public HyperthoughtWorkflowGraph(HyperthoughtApi api, HyperthoughtWorkflow workflow)
            : this(api, workflow.Content.PrimaryKey) { }
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
                    Type = value.Type.ToString().ToFriendlyType(),
                    Value = value.Link
                });
            }

            FreeformVertex vertex = new FreeformVertex
            {
                Id = workflow.Content.PrimaryKey,
                Label = workflow.Content.Name,
                Description = workflow.Content.Notes,
                Properties = properties
            };
            return vertex;
        }

        /// <inheritdoc />
        public bool IsFinite => false;
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
        public IEnumerable<Edge<FreeformVertex>> GetEdges(Guid id)
        {
            // Get the workflow from an API call and yield each of the children.
            HyperthoughtWorkflow workflow = Task.Run(() => Api.GetWorkflowAsync(id)).GetAwaiter().GetResult();
            FreeformVertex thisVertex = new FreeformVertex { Id = id };
            foreach (Guid childId in workflow.Content.ChildrenIds)
            {
                yield return new Edge<FreeformVertex>(
                    thisVertex,
                    new FreeformVertex { Id = childId }
                );
            }
        }
        /// <inheritdoc />
        public IDictionary<Guid, FreeformVertex> GetChildren(Guid id)
        {
            // Get the child workflows from an API call and yield each by ID.
            IList<HyperthoughtWorkflow> workflows = Task.Run(() => Api.GetWorkflowChildrenAsync(id)).GetAwaiter().GetResult();
            return workflows.ToDictionary(
                workflow => workflow.Content.PrimaryKey,
                workflow => VertexFromWorkflow(workflow)
            );
        }
    }
}