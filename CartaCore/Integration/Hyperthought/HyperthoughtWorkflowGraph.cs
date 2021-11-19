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
        private readonly Guid Id;

        /// <summary>
        /// Initializes a new instance of the <see cref="HyperthoughtWorkflowGraph"/> class using a
        /// <see cref="HyperthoughtApi"/> instance and a workflow ID.
        /// </summary>
        /// <param name="api">The HyperThought API.</param>
        /// <param name="id">The ID of the workflow.</param>
        public HyperthoughtWorkflowGraph(HyperthoughtApi api, Guid id)
            : base(Identity.Create($"{nameof(HyperthoughtWorkflowGraph)}:{id}"))
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
            List<Property> subproperties = new();

            #region Triples
            Property triples = new(Identity.Create("Triples"));
            List<object> subjects = new(workflow.Triples.Count);
            List<object> predicates = new(workflow.Triples.Count);
            List<object> objects = new(workflow.Triples.Count);
            foreach (HyperthoughtTriple triple in workflow.Triples)
            {
                subjects.Add(triple.Content.Subject.Data);
                predicates.Add(triple.Content.Predicate.Data);
                objects.Add(triple.Content.Object.Data);
            }
            triples.Subproperties = new Property[]
            {
                new Property(Identity.Create("Subjects"), subjects),
                new Property(Identity.Create("Predicates"), predicates),
                new Property(Identity.Create("Objects"), objects)
            };
            subproperties.Add(triples);
            #endregion

            #region Headers
            Property headers = new(Identity.Create("Headers"))
            {
                Subproperties = new Property[]
                {
                    new Property(Identity.Create("Canonical URI"), workflow.Headers.CanonicalUri),
                    new Property(Identity.Create("URI"), workflow.Headers.Uri),
                    new Property(Identity.Create("System Creation Time"), workflow.Headers.CreationTime),
                    new Property(Identity.Create("System Last Modification Time"), workflow.Headers.LastModifiedTime),
                    new Property(Identity.Create("Created By"), workflow.Headers.CreatedBy),
                    new Property(Identity.Create("Modified By"), workflow.Headers.LastModifiedBy),
                    new Property(Identity.Create("Process ID"), workflow.Headers.ProcessId)
                }
            };
            subproperties.Add(headers);
            #endregion

            #region Permissions
            // Structure permissions so that each permission entry is a property of the appropriate permissions field.
            Property permissions = new(Identity.Create("Permissions"));
            Property workspacePermissions = new(Identity.Create("Workspaces"));
            Property userPermissions = new(Identity.Create("Users"));
            List<Property> workspaceSubproperties = new();
            List<Property> userSubproperties = new();

            foreach (string key in workflow.Permissions.Workspaces.Keys)
                workspaceSubproperties.Add(new Property(Identity.Create(key), workflow.Permissions.Workspaces[key]));
            foreach (string key in workflow.Permissions.Users.Keys)
                userSubproperties.Add(new Property(Identity.Create(key), workflow.Permissions.Users[key]));

            workspacePermissions.Subproperties = workspaceSubproperties;
            workspaceSubproperties.TrimExcess();
            userPermissions.Subproperties = userSubproperties;
            userSubproperties.TrimExcess();

            permissions.Subproperties = new Property[]
            {
                workspacePermissions,
                userPermissions
            };
            subproperties.Add(permissions);
            #endregion

            #region Restrictions
            Property restrictions = new(Identity.Create("Restrictions"))
            {
                Subproperties = new Property[]
                {
                    new Property(Identity.Create("Distribution"), workflow.Restrictions.Distribution),
                    new Property(Identity.Create("Export Control"), workflow.Restrictions.ExportControl),
                    new Property(Identity.Create("Security Marking"), workflow.Restrictions.SecurityMarking)
                }
            };
            subproperties.Add(restrictions);
            #endregion

            #region Content
            List<Property> content = new()
            {
                // ID, and name and description are excluded because they are included in the vertex structure itself.
                new Property(Identity.Create("Process ID"), workflow.Content.ProcessId),
                new Property(Identity.Create("Parent Process"), workflow.Content.ParentProcessId),
                new Property(Identity.Create("Client ID"), workflow.Content.ClientId),
                new Property(Identity.Create("Successors"), workflow.Content.SuccessorIds),
                new Property(Identity.Create("Predecessors"), workflow.Content.PredecessorIds),
                new Property(Identity.Create("Children"), workflow.Content.ChildrenIds),
                new Property(Identity.Create("Process Type"), workflow.Content.Type),
                new Property(Identity.Create("Template"), workflow.Content.Template),
                new Property(Identity.Create("Created By"), workflow.Content.CreatedBy),
                new Property(Identity.Create("Creation Time"), workflow.Content.CreatedTime),
                new Property(Identity.Create("Last Modified By"), workflow.Content.LastModifiedBy),
                new Property(Identity.Create("Last Modification Time"), workflow.Content.LastModifiedTime),
                new Property(Identity.Create("Assignee"), workflow.Content.Assignee),
                new Property(Identity.Create("Status"), workflow.Content.Status),
                new Property(Identity.Create("Started"), workflow.Content.StartedTime),
                new Property(Identity.Create("Completed"), workflow.Content.CompletedTime),
                new Property(Identity.Create("XML"), workflow.Content.Xml),
            };

            // Add any fields not explicitly part of the schema.
            foreach (string key in workflow.Content.Extensions.Keys)
                content.Add(new Property(Identity.Create(key), workflow.Content.Extensions[key]));

            subproperties.Add(new Property(Identity.Create("Content")) { Subproperties = content });
            #endregion

            #region Process Metadata
            // We check if the node is a non-workflow node to determine if we should add metadata to it.
            // If the node is a workflow node, we use its additional properties as the properties itself.
            List<Property> properties;
            if (workflow.Content.Type != HyperthoughtProcessType.Workflow)
            {
                // We find the properties for the vertex from the metadata.
                properties = new(workflow.Metadata.Count);
                foreach (HyperthoughtMetadata metadata in workflow.Metadata)
                {
                    // If properties end up having the same name, we need to make sure that the values are unique.
                    // Thus, we ignore every value except the first.
                    HyperthoughtMetadataValue value = metadata.Value;
                    if (properties.Any(property => property.Identifier.Equals(metadata.Key))) continue;

                    // We create a new property.
                    Property property = new(Identity.Create(metadata.Key), value.Link);
                    properties.Add(property);

                    // Add metadata on this property.
                    List<Property> vertexSubproperties = new(subproperties);
                    foreach (string key in metadata.Extensions.Keys)
                        vertexSubproperties.Insert(0, new Property(Identity.Create(key), metadata.Extensions[key]));
                    vertexSubproperties.TrimExcess();
                    property.Subproperties = vertexSubproperties;
                }
                properties.TrimExcess();
            }
            else
            {
                subproperties.TrimExcess();
                properties = subproperties;
            }
            #endregion

            // Get the identifier of this vertex.
            Identity id = Identity.Create(workflow.Content.PrimaryKey);

            // Add the parent and predecessors to in-edges.
            int inEdgeCount = 1 + workflow.Content.PredecessorIds.Count;
            IList<Edge> inEdges = new List<Edge>(capacity: inEdgeCount);
            if (workflow.Content.ParentProcessId is not null)
                inEdges.Add(new Edge(Identity.Create(workflow.Content.ParentProcessId.Value), id));
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
            return new(id, properties, Enumerable.Concat(inEdges, outEdges))
            {
                Label = workflow.Content.Name,
                Description = workflow.Content.Notes
            };
        }

        /// <inheritdoc />
        public override GraphProperties GetProperties()
        {
            return new GraphProperties
            {
                Directed = true,
                Dynamic = true,
                Finite = true
            };
        }

        /// <inheritdoc />
        public IEnumerable<Identity> GetRoots()
        {
            yield return Identity.Create(Id);
        }

        /// <inheritdoc />
        public async ITask<Vertex> GetVertex(Identity id)
        {
            // Check that the identifier is of the correct type first.
            if (!id.IsType(out Guid guid)) throw new InvalidCastException();

            // Get the workflow asynchronously and return the vertex created from it.
            HyperthoughtProcess workflow = await Api.Workflow.GetProcessAsync(guid);
            return VertexFromWorkflow(workflow);
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<Vertex> GetVertices(IEnumerable<Identity> ids)
        {
            // Setup our data structures to store our identifiers and async tasks.
            List<Identity> idList = ids.ToList();
            Task<HyperthoughtProcess>[] workflowTasks = new Task<HyperthoughtProcess>[idList.Count];

            // Launch tasks to get all of the requested workflows.
            for (int k = 0; k < idList.Count; k++)
            {
                if (!idList[k].IsType(out Guid guid)) throw new InvalidCastException();
                workflowTasks[k] = Api.Workflow.GetProcessAsync(guid);
            }

            // Return vertices for each workflow in the order they were requested.
            for (int k = 0; k < idList.Count; k++)
                yield return VertexFromWorkflow(await workflowTasks[k]);
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<Vertex> GetParentVertices(Identity id)
        {
            // Get the vertex asynchronously and return the parent vertex created from it.
            Vertex vertex = await GetVertex(id);
            foreach (Edge inEdge in vertex.InEdges)
                yield return await GetVertex(inEdge.Target);
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<Vertex> GetChildVertices(Identity id)
        {
            // This is an optimized co-version of get parent vertices because HyperThought supports this API.
            // Check that the identifier is of the correct type first.
            if (!id.IsType(out Guid guid)) throw new InvalidCastException();

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
            await Api.Workflow.GetProcessAsync(Id);
        }
    }
}