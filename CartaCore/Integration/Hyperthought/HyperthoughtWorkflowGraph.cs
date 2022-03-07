using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MorseCode.ITask;
using CartaCore.Integration.Hyperthought.Data;
using CartaCore.Integration.Hyperthought.Api;
using CartaCore.Graphs.Components;
using CartaCore.Graphs;

namespace CartaCore.Integration.Hyperthought
{
    // TODO: Add support for optionally including HyperThought metadata.
    /// <summary>
    /// Represents a sampled graph generated from data contained in a HyperThought workflow.
    /// </summary>
    public class HyperthoughtWorkflowGraph : Graph,
        IRootedComponent,
        IDynamicLocalComponent<Vertex, Edge>,
        IDynamicInComponent<Vertex, Edge>,
        IDynamicOutComponent<Vertex, Edge>
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
            Dictionary<string, IProperty> metaProperties = new();

            #region Triples
            List<string> subjects = new(workflow.Triples.Count);
            List<string> predicates = new(workflow.Triples.Count);
            List<string> objects = new(workflow.Triples.Count);
            foreach (HyperthoughtTriple triple in workflow.Triples)
            {
                subjects.Add(triple.Content.Subject.Data);
                predicates.Add(triple.Content.Predicate.Data);
                objects.Add(triple.Content.Object.Data);
            }
            Property triples = new()
            {
                Properties = new Dictionary<string, IProperty>
                {
                    ["Subjects"] = new Property(subjects),
                    ["Predicates"] = new Property(predicates),
                    ["Objects"] = new Property(objects)
                }
            };
            metaProperties["Triples"] = triples;
            #endregion

            #region Headers
            Property headers = new()
            {
                Properties = new Dictionary<string, IProperty>
                {
                    ["Canonical URI"] = new Property(workflow.Headers.CanonicalUri),
                    ["URI"] = new Property(workflow.Headers.Uri),
                    ["System Creation Time"] = new Property(workflow.Headers.CreationTime),
                    ["System Last Modification Time"] = new Property(workflow.Headers.LastModifiedTime),
                    ["Created By"] = new Property(workflow.Headers.CreatedBy),
                    ["Modified By"] = new Property(workflow.Headers.LastModifiedBy),
                    ["Process ID"] = new Property(workflow.Headers.ProcessId)
                }
            };
            metaProperties["Headers"] = headers;
            #endregion

            #region Permissions
            // Structure permissions so that each permission entry is a property of the appropriate permissions field.
            Dictionary<string, IProperty> workspaceSubproperties = new();
            foreach (string key in workflow.Permissions.Workspaces.Keys)
                workspaceSubproperties[key] = new Property(workflow.Permissions.Workspaces[key]);
            workspaceSubproperties.TrimExcess();

            Dictionary<string, IProperty> userSubproperties = new();
            foreach (string key in workflow.Permissions.Users.Keys)
                userSubproperties[key] = new Property(workflow.Permissions.Users[key]);
            userSubproperties.TrimExcess();

            Property workspacePermissions = new() { Properties = workspaceSubproperties };
            Property userPermissions = new() { Properties = userSubproperties };
            Property permissions = new()
            {
                Properties = new Dictionary<string, IProperty>
                {
                    ["Workspaces"] = workspacePermissions,
                    ["Users"] = userPermissions
                }
            };
            metaProperties["Permissions"] = permissions;
            #endregion

            #region Restrictions
            Property restrictions = new()
            {
                Properties = new Dictionary<string, IProperty>
                {
                    ["Distribution"] = new Property(workflow.Restrictions.Distribution),
                    ["Export Control"] = new Property(workflow.Restrictions.ExportControl),
                    ["Security Marking"] = new Property(workflow.Restrictions.SecurityMarking)
                }
            };
            metaProperties["Restrictions"] = restrictions;
            #endregion

            #region Content
            Dictionary<string, IProperty> content = new()
            {
                // ID, and name and description are excluded because they are included in the vertex structure itself.
                ["Process ID"] = new Property(workflow.Content.ProcessId),
                ["Parent Process"] = new Property(workflow.Content.ParentProcessId),
                ["Client ID"] = new Property(workflow.Content.ClientId),
                ["Successors"] = new Property(workflow.Content.SuccessorIds),
                ["Predecessors"] = new Property(workflow.Content.PredecessorIds),
                ["Children"] = new Property(workflow.Content.ChildrenIds),
                ["Process Type"] = new Property(workflow.Content.Type),
                ["Template"] = new Property(workflow.Content.Template),
                ["Created By"] = new Property(workflow.Content.CreatedBy),
                ["Creation Time"] = new Property(workflow.Content.CreatedTime),
                ["Last Modified By"] = new Property(workflow.Content.LastModifiedBy),
                ["Last Modification Time"] = new Property(workflow.Content.LastModifiedTime),
                ["Assignee"] = new Property(workflow.Content.Assignee),
                ["Status"] = new Property(workflow.Content.Status),
                ["Started"] = new Property(workflow.Content.StartedTime),
                ["Completed"] = new Property(workflow.Content.CompletedTime),
                ["XML"] = new Property(workflow.Content.Xml),
            };

            // Add any fields not explicitly part of the schema.
            foreach (string key in workflow.Content.Extensions.Keys)
                content[key] = new Property(workflow.Content.Extensions[key]);

            metaProperties["Content"] = new Property() { Properties = content };
            #endregion

            #region Process Metadata
            // We find the properties for the vertex from the metadata.
            Dictionary<string, IProperty> standardProperties = new(workflow.Metadata.Count);
            foreach (HyperthoughtMetadata metadata in workflow.Metadata)
            {
                // Get extension data for this property.
                Dictionary<string, IProperty> suproperties = new();
                foreach (string key in metadata.Extensions.Keys)
                    suproperties[key] = new Property(metadata.Extensions[key]);
                suproperties.TrimExcess();

                // We create a new property.
                HyperthoughtMetadataValue value = metadata.Value;
                Property property = new(value.Link) { Properties = suproperties };
                standardProperties[metadata.Key] = property;
            }
            standardProperties["HyperThought Metadata"] = new Property() { Properties = metaProperties };
            standardProperties.TrimExcess();
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
            return new(id, standardProperties, edges)
            {
                Label = workflow.Content.Name,
                Description = workflow.Content.Notes
            };
        }

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