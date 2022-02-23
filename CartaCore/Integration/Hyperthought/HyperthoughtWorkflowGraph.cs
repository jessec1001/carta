using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MorseCode.ITask;
using CartaCore.Data;
using CartaCore.Integration.Hyperthought.Data;
using CartaCore.Integration.Hyperthought.Api;

namespace CartaCore.Integration.Hyperthought
{
    /// <summary>
    /// Represents a sampled graph generated from data contained in a HyperThought workflow.
    /// </summary>
    public class HyperthoughtWorkflowGraph : Graph,
        IRootedGraph,
        IDynamicInGraph<Vertex>,
        IDynamicOutGraph<Vertex>
    {
        /// <summary>
        /// The connector to the HyperThought API. 
        /// </summary>
        private readonly HyperthoughtApi Api;
        /// <summary>
        /// The GUID of the HyperThought workflow object. 
        /// </summary>
        private readonly Guid WorkflowId;

        /// <summary>
        /// Initializes a new instance of the <see cref="HyperthoughtWorkflowGraph"/> class using a
        /// <see cref="HyperthoughtApi"/> instance and a workflow ID.
        /// </summary>
        /// <param name="api">The HyperThought API.</param>
        /// <param name="id">The ID of the workflow.</param>
        public HyperthoughtWorkflowGraph(HyperthoughtApi api, Guid id)
            : base($"{nameof(HyperthoughtWorkflowGraph)}:{id}")
        {
            Api = api;
            WorkflowId = id;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="HyperthoughtWorkflowGraph"/> class using a
        /// <see cref="HyperthoughtApi"/> instance and a workflow.
        /// </summary>
        /// <param name="api">The HyperThought API.</param>
        /// <param name="workflow">The workflow.</param>
        public HyperthoughtWorkflowGraph(HyperthoughtApi api, HyperthoughtProcess workflow)
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
        private static Vertex VertexFromWorkflow(HyperthoughtProcess workflow)
        {
            // Properties are compiled from the contents read from the data source.
            // This depends on the schema for this data source.
            HashSet<IProperty> subproperties = new();

            #region Triples
            List<object> subjects = new(workflow.Triples.Count);
            List<object> predicates = new(workflow.Triples.Count);
            List<object> objects = new(workflow.Triples.Count);
            foreach (HyperthoughtTriple triple in workflow.Triples)
            {
                subjects.Add(triple.Content.Subject.Data);
                predicates.Add(triple.Content.Predicate.Data);
                objects.Add(triple.Content.Object.Data);
            }
            Property triples = new("Triples")
            {
                Properties = new HashSet<IProperty>
                {
                    new Property("Subjects", subjects),
                    new Property("Predicates", predicates),
                    new Property("Objects", objects)
                }
            };
            subproperties.Add(triples);
            #endregion

            #region Headers
            Property headers = new("Headers")
            {
                Properties = new HashSet<IProperty>
                {
                    new Property("Canonical URI", workflow.Headers.CanonicalUri),
                    new Property("URI", workflow.Headers.Uri),
                    new Property("System Creation Time", workflow.Headers.CreationTime),
                    new Property("System Last Modification Time", workflow.Headers.LastModifiedTime),
                    new Property("Created By", workflow.Headers.CreatedBy),
                    new Property("Modified By", workflow.Headers.LastModifiedBy),
                    new Property("Process ID", workflow.Headers.ProcessId)
                }
            };
            subproperties.Add(headers);
            #endregion

            #region Permissions
            // Structure permissions so that each permission entry is a property of the appropriate permissions field.
            HashSet<IProperty> workspaceSubproperties = new();
            foreach (string key in workflow.Permissions.Workspaces.Keys)
                workspaceSubproperties.Add(new Property(key, workflow.Permissions.Workspaces[key]));
            workspaceSubproperties.TrimExcess();

            HashSet<IProperty> userSubproperties = new();
            foreach (string key in workflow.Permissions.Users.Keys)
                userSubproperties.Add(new Property(key, workflow.Permissions.Users[key]));
            userSubproperties.TrimExcess();

            Property workspacePermissions = new("Workspaces") { Properties = workspaceSubproperties };
            Property userPermissions = new("Users") { Properties = userSubproperties };

            Property permissions = new ("Permissions")
            {
                Properties = new HashSet<IProperty>
                {
                    workspacePermissions,
                    userPermissions
                }
            };
            subproperties.Add(permissions);
            #endregion

            #region Restrictions
            Property restrictions = new("Restrictions")
            {
                Properties = new HashSet<IProperty>
                {
                    new Property("Distribution", workflow.Restrictions.Distribution),
                    new Property("Export Control", workflow.Restrictions.ExportControl),
                    new Property("Security Marking", workflow.Restrictions.SecurityMarking)
                }
            };
            subproperties.Add(restrictions);
            #endregion

            #region Content
            HashSet<IProperty> content = new()
            {
                // ID, and name and description are excluded because they are included in the vertex structure itself.
                new Property("Process ID", workflow.Content.ProcessId),
                new Property("Parent Process", workflow.Content.ParentProcessId),
                new Property("Client ID", workflow.Content.ClientId),
                new Property("Successors", workflow.Content.SuccessorIds),
                new Property("Predecessors", workflow.Content.PredecessorIds),
                new Property("Children", workflow.Content.ChildrenIds),
                new Property("Process Type", workflow.Content.Type),
                new Property("Template", workflow.Content.Template),
                new Property("Created By", workflow.Content.CreatedBy),
                new Property("Creation Time", workflow.Content.CreatedTime),
                new Property("Last Modified By", workflow.Content.LastModifiedBy),
                new Property("Last Modification Time", workflow.Content.LastModifiedTime),
                new Property("Assignee", workflow.Content.Assignee),
                new Property("Status", workflow.Content.Status),
                new Property("Started", workflow.Content.StartedTime),
                new Property("Completed", workflow.Content.CompletedTime),
                new Property("XML", workflow.Content.Xml),
            };

            // Add any fields not explicitly part of the schema.
            foreach (string key in workflow.Content.Extensions.Keys)
                content.Add(new Property(key, workflow.Content.Extensions[key]));

            subproperties.Add(new Property("Content") { Properties = content });
            #endregion

            #region Process Metadata
            // We find the properties for the vertex from the metadata.
            HashSet<IProperty> metadataProperties = new(workflow.Metadata.Count);
            foreach (HyperthoughtMetadata metadata in workflow.Metadata)
            {
                // If properties end up having the same name, we need to make sure that the values are unique.
                // Thus, we ignore every value except the first.
                HyperthoughtMetadataValue value = metadata.Value;
                if (metadataProperties.Any(property => property.Id == metadata.Key)) continue;

                // Get extension data for this property.
                HashSet<IProperty> metadataSubproperties = new();
                foreach (string key in metadata.Extensions.Keys)
                    metadataSubproperties.Add(new Property(key, metadata.Extensions[key]));
                metadataSubproperties.TrimExcess();

                // We create a new property.
                Property property = new(metadata.Key, value.Link) { Properties = metadataSubproperties };
                metadataProperties.Add(property);
            }
            metadataProperties.Add(new Property("HyperThought Metadata") { Properties = subproperties });
            metadataProperties.TrimExcess();
            #endregion

            // Get the identifier of this vertex.
            string id = workflow.Content.PrimaryKey.ToString();

            // Get the edges for this vertex.
            int edgeCount = 1 + (
                workflow.Content.SuccessorIds.Count +
                workflow.Content.PredecessorIds.Count +
                workflow.Content.ChildrenIds.Count
            );
            IList<Edge> edges = new List<Edge>(capacity: edgeCount);

            // Add the parent and predecessors to in-edges.
            if (workflow.Content.ParentProcessId is not null)
                edges.Add(new Edge(workflow.Content.ParentProcessId.ToString(), id));
            foreach (Guid predecessorId in workflow.Content.PredecessorIds.OrderBy(id => id))
                edges.Add(new Edge(predecessorId.ToString(), id));

            // Add all children and successors to out-edges.
            foreach (Guid childId in workflow.Content.ChildrenIds.OrderBy(id => id))
                edges.Add(new Edge(id, childId.ToString()));
            foreach (Guid successorId in workflow.Content.SuccessorIds.OrderBy(id => id))
                edges.Add(new Edge(id, successorId.ToString()));

            // Create the vertex with the name and notes as labels and description respectively.
            return new(id, metadataProperties, edges)
            {
                Label = workflow.Content.Name,
                Description = workflow.Content.Notes
            };
        }

        /// <inheritdoc />
        public override GraphAttributes Attributes => new()
        {
            Dynamic = true,
            Finite = true
        };

        /// <inheritdoc />
        public async IAsyncEnumerable<string> Roots()
        {
            yield return await Task.FromResult(WorkflowId.ToString());
        }

        /// <inheritdoc />
        public async ITask<Vertex> GetVertex(string id)
        {
            // Check that the identifier is of the correct type first.
            if (!Guid.TryParse(id, out Guid guid)) throw new InvalidCastException();

            // Get the workflow asynchronously and return the vertex created from it.
            HyperthoughtProcess workflow = await Api.Workflow.GetProcessAsync(guid);
            return VertexFromWorkflow(workflow);
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<Vertex> GetVertices(IEnumerable<string> ids)
        {
            // Setup our data structures to store our identifiers and async tasks.
            List<string> idList = ids.ToList();
            Task<HyperthoughtProcess>[] processTasks = new Task<HyperthoughtProcess>[idList.Count];

            // Launch tasks to get all of the requested workflows.
            for (int k = 0; k < idList.Count; k++)
            {
                if (!Guid.TryParse(idList[k], out Guid guid)) throw new InvalidCastException();
                processTasks[k] = Api.Workflow.GetProcessAsync(guid);
            }

            // We speed this up by ignoring the ordering of vertices returned.
            for (int k = 0; k < idList.Count; k++)
            {
                Task<HyperthoughtProcess> processTask = await Task.WhenAny(processTasks);
                yield return VertexFromWorkflow(await processTask);
            }
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<Vertex> GetParentVertices(string id)
        {
            // Get the vertex asynchronously and return the parent vertex created from it.
            Vertex vertex = await GetVertex(id);
            foreach (Edge inEdge in vertex.InEdges)
                yield return await GetVertex(inEdge.Target);
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<Vertex> GetChildVertices(string id)
        {
            // This is an optimized co-version of get parent vertices because HyperThought supports this API.
            // Check that the identifier is of the correct type first.
            if (!Guid.TryParse(id, out Guid guid)) throw new InvalidCastException();

            // Get the workflows asynchronously and return the vertices created from them.
            IList<HyperthoughtProcess> workflows = await Api.Workflow.GetWorkflowChildrenProcessesAsync(guid);
            foreach (HyperthoughtProcess workflow in workflows)
                yield return VertexFromWorkflow(workflow);
        }

        /// <summary>
        /// Ensures that the graph is valid and authorized. Throws an exception if not.
        /// </summary>
        public async Task EnsureValidity()
        {
            await Api.Workflow.GetProcessAsync(WorkflowId);
        }
    }
}