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
            // Properties are compiled from the contents read from the data source.
            // This depends on the schema for this data source.
            List<Property> subproperties = new();

            #region triples
            Property triples = new Property(Identity.Create("Triples"));
            List<object> subjects = new List<object>(workflow.Triples.Count);
            List<object> predicates = new List<object>(workflow.Triples.Count);
            List<object> objects = new List<object>(workflow.Triples.Count);
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

            #region headers
            Property headers = new Property(Identity.Create("Headers"))
            {
                Subproperties = new Property[]
                {
                    new Property
                    (
                        Identity.Create("Canonical URI"),
                        new object[] { workflow.Headers.CanonicalUri }
                    ),
                    new Property
                    (
                        Identity.Create("URI"),
                        new object[] { workflow.Headers.Uri }
                    ),
                    new Property
                    (
                        Identity.Create("System Creation Time"),
                        new object[] { workflow.Headers.CreationTime }
                    ),
                    new Property
                    (
                        Identity.Create("System Last Modification Time"),
                        new object[] { workflow.Headers.LastModifiedTime }
                    ),
                    new Property
                    (
                        Identity.Create("Created By"),
                        new object[] { workflow.Headers.CreatedBy }
                    ),
                    new Property
                    (
                        Identity.Create("Modified By"),
                        new object[] { workflow.Headers.LastModifiedBy }
                    ),
                    new Property
                    (
                        Identity.Create("Process ID"),
                        new object[] { workflow.Headers.ProcessId }
                    )
                }
            };
            subproperties.Add(headers);
            #endregion

            #region permissions
            Property permissions = new Property(Identity.Create("Permissions"));
            Property projects = new Property(Identity.Create("Projects"));
            Property groups = new Property(Identity.Create("Groups"));
            Property users = new Property(Identity.Create("Users"));
            List<Property> projectSubproperties = new();
            foreach (string key in workflow.Permissions.Projects.Keys)
            {
                projectSubproperties.Add
                (
                    new Property
                    (
                        Identity.Create(key),
                        new object[] { workflow.Permissions.Projects[key] }
                    )
                );
            }
            projectSubproperties.TrimExcess();
            List<Property> groupSubproperties = new();
            foreach (string key in workflow.Permissions.Groups.Keys)
            {
                groupSubproperties.Add
                (
                    new Property
                    (
                        Identity.Create(key),
                        new object[] { workflow.Permissions.Groups[key] }
                    )
                );
            }
            groupSubproperties.TrimExcess();
            List<Property> userSubproperties = new();
            foreach (string key in workflow.Permissions.Users.Keys)
            {
                userSubproperties.Add
                (
                    new Property
                    (
                        Identity.Create(key),
                        new object[] { workflow.Permissions.Users[key] }
                    )
                );
            }
            userSubproperties.TrimExcess();
            permissions.Subproperties = new Property[]
            {
                projects,
                groups,
                users
            };
            subproperties.Add(permissions);
            #endregion

            #region restrictions
            Property restrictions = new Property(Identity.Create("Restrictions"))
            {
                Subproperties = new Property[]
                {
                    new Property
                    (
                        Identity.Create("Distribution"),
                        new object[] { workflow.Restrictions.Distribution }
                    ),
                    new Property
                    (
                        Identity.Create("Export Control"),
                        new object[] { workflow.Restrictions.ExportControl }
                    ),
                    new Property
                    (
                        Identity.Create("Security Marking"),
                        new object[] { workflow.Restrictions.SecurityMarking }
                    )
                }
            };
            subproperties.Add(restrictions);
            #endregion

            #region content
            List<Property> content = new List<Property>
            (
                new Property[]
                {
                    // Name is excluded because the name is used to name the Vertex itself.
                    new Property
                    (
                        Identity.Create("Process ID"),
                        new object[] { workflow.Content.ProcessId }
                    ),
                    new Property
                    (
                        Identity.Create("Parent Process"),
                        new object[] { workflow.Content.ParentProcessId }
                    ),
                    new Property
                    (
                        Identity.Create("Client ID"),
                        new object[] { workflow.Content.ClientId }
                    ),
                    new Property
                    (
                        Identity.Create("Successors"),
                        workflow.Content.SuccessorIds.Cast<object>().ToList()
                    ),
                    new Property
                    (
                        Identity.Create("Predecessors"),
                        workflow.Content.PredecessorIds.Cast<object>().ToList()
                    ),
                    new Property
                    (
                        Identity.Create("Children"),
                        workflow.Content.ChildrenIds.Cast<object>().ToList()
                    ),
                    new Property
                    (
                        Identity.Create("Primary Key"),
                        new object[] { workflow.Content.PrimaryKey }
                    ),
                    new Property
                    (
                        Identity.Create("Process Type"),
                        new object[] { workflow.Content.Type }
                    ),
                    new Property
                    (
                        Identity.Create("Template"),
                        new object[] { workflow.Content.Template }
                    ),
                    new Property
                    (
                        Identity.Create("Created By"),
                        new object[] { workflow.Content.CreatedBy }
                    ),
                    new Property
                    (
                        Identity.Create("Creation Time"),
                        new object[] { workflow.Content.CreatedTime }
                    ),
                    new Property
                    (
                        Identity.Create("Last Modified By"),
                        new object[] { workflow.Content.LastModifiedBy }
                    ),
                    new Property
                    (
                        Identity.Create("Last Modification Time"),
                        new object[] { workflow.Content.LastModifiedTime }
                    ),
                    new Property
                    (
                        Identity.Create("Assignee"),
                        new object[] { workflow.Content.Assignee }
                    ),
                    new Property
                    (
                        Identity.Create("Status"),
                        new object[] { workflow.Content.Status }
                    ),
                    new Property
                    (
                        Identity.Create("Started"),
                        new object[] { workflow.Content.StartedTime }
                    ),
                    new Property
                    (
                        Identity.Create("Completed"),
                        new object[] { workflow.Content.CompletedTime }
                    ),
                    new Property
                    (
                        Identity.Create("XML"),
                        new object[] { workflow.Content.Xml }
                    ),
                    new Property
                    (
                        Identity.Create("Notes"),
                        new object[] { workflow.Content.Notes }
                    )
                }
            );
            // Add any fields not explicitly part of the schema.
            foreach (string key in workflow.Content.Extensions.Keys)
            {
                content.Add
                (
                    new Property
                    (
                        Identity.Create(key),
                        new object[] { workflow.Content.Extensions[key] }
                    )
                );
            };
            subproperties.Add
            (
                new Property(Identity.Create("Content"))
                {
                    Subproperties = content
                }
            );
            #endregion

            subproperties.TrimExcess();
            // We find the properties for the vertex from the metadata.
            List<Property> properties = new(workflow.Metadata.Count);
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

                // Add metadata on this property
                List<Property> vertexSubproperties = new(subproperties);
                foreach (string key in metadata.Extensions.Keys)
                {
                    vertexSubproperties.Insert
                    (
                        0,
                        new Property
                        (
                            Identity.Create(key),
                            new object[] { metadata.Extensions[key] }
                        )
                    );
                };
                vertexSubproperties.TrimExcess();
                property.Subproperties = vertexSubproperties;
            }
            properties.TrimExcess();

            // include the vertex information as properties if no metadata is reported
            if (properties.Count == 0)
                properties = subproperties;

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
        public override bool IsFinite() => true;
        /// <inheritdoc />
        public override bool IsDirected() => true;
        /// <inheritdoc />
        public override bool IsDynamic() => true;

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