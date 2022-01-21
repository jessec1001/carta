using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using CartaCore.Data;
using CartaCore.Integration.Hyperthought;
using CartaCore.Integration.Hyperthought.Api;
using CartaCore.Integration.Hyperthought.Data;
using CartaCore.Operations;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Represents a data resolver that retrieves HyperThought data from the controller route data.
    /// </summary>
    public class HyperthoughtDataResolver : IDataResolver
    {
        private static TimeSpan AliasExpiration = TimeSpan.FromMinutes(15.00);
        private static readonly int AliasCount = short.MaxValue;

        private SortedList<string, (DateTime, Guid)> Aliases;

        /// <summary>
        /// Creates a new instance of the <see cref="HyperthoughtDataResolver"/> class with a specified controller.
        /// </summary>
        public HyperthoughtDataResolver()
        {
            Aliases = new SortedList<string, (DateTime, Guid)>(AliasCount, StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public async Task<Graph> GenerateGraphAsync(ControllerBase controller, string resource)
        {
            // We check that an API key was specified.
            if (!controller.Request.Query.ContainsKey("api"))
                throw new HttpRequestException("HyperThought API key must be non-null.", null, HttpStatusCode.Unauthorized);

            // We need to find the UUID associated with a HyperThought resource if we are passed an alias.
            HyperthoughtApi api = new HyperthoughtApi(controller.Request.Query["api"].ToString());
            Guid uuid = Guid.Empty;
            if (!Guid.TryParse(resource, out uuid))
            {
                // Find the alias if we don't currently have it or it is outdated.
                if (!Aliases.TryGetValue(resource, out (DateTime timestamp, Guid uuid) alias) ||
                    (DateTime.Now - alias.timestamp) > AliasExpiration)
                {
                    // Get the alias.
                    uuid = await api.Workflow.GetProcessIdFromPathAsync(resource);
                    if (uuid == Guid.Empty) return null;

                    // Make sure we don't use too many aliases and add the new alias.
                    if (Aliases.Count >= AliasCount) Aliases.Clear();
                    Aliases.Remove(resource);
                    Aliases.Add(resource, (DateTime.Now, uuid));
                }
                else
                {
                    uuid = alias.uuid;
                }
            }
            HyperthoughtWorkflowGraph graph = new HyperthoughtWorkflowGraph(api, uuid);
            await graph.EnsureValidity();
            return graph;
        }

        public async Task<OperationTemplate> GenerateOperationAsync(ControllerBase controller, string resource)
        {
            // We check that an API key was specified.
            if (!controller.Request.Query.ContainsKey("api"))
                throw new HttpRequestException("HyperThought API key must be non-null.", null, HttpStatusCode.Unauthorized);

            // TODO: Use something more like `new HyperthoughtGraphOperation().Template(defaults)` where defaults is typed for typed operations.
            return new HyperthoughtGraphOperation().GetTemplate
            (
                new HyperthoughtGraphOperationIn()
                {
                    Path = resource
                }
            );
        }

        /// <inheritdoc />
        public async Task<IList<string>> FindResourcesAsync(ControllerBase controller)
        {
            // We check that an API key was specified.
            if (!controller.Request.Query.ContainsKey("api"))
                throw new HttpRequestException("HyperThought API key must be non-null.", null, HttpStatusCode.Unauthorized);

            // Get all of the workflow templates for all of the projects from the API.
            // Using the user's API key, this should only return resources accessible to the user.
            HyperthoughtApi api = new HyperthoughtApi(controller.Request.Query["api"].ToString());
            IList<HyperthoughtWorkspace> workspaces = await api.Workspaces.GetWorkspacesAsync();
            IList<Task<IList<HyperthoughtWorkflowTemplate>>> templateTasks = workspaces
                .Select(workspace => api.Workflow.GetWorkflowTemplatesAsync(workspace))
                .ToList();

            // Construct the resources list.
            // Each resource should be of the form "Project.Template"
            List<string> resources = new List<string>();
            for (int k = 0; k < workspaces.Count; k++)
            {
                foreach (HyperthoughtWorkflowTemplate template in await templateTasks[k])
                {
                    resources.Add($"{workspaces[k].Name}.{template.Title}");
                }
            }
            return resources;
        }
    }
}