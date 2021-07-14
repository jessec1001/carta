using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using CartaCore.Integration.Hyperthought.Data;

namespace CartaCore.Integration.Hyperthought.Api
{
    /// <summary>
    /// Represents the functionality of the Workflow module of the HyperThought API.
    /// </summary>
    public class HyperthoughtWorkflowApi
    {
        private HyperthoughtApi Api { get; init; }

        #region URIs
        /// <summary>
        /// Gets the workflow API URI at the HyperThought instance.
        /// </summary>
        protected Uri GetApiUri() => new Uri(Api.GetBaseUri(), "workflow/");
        /// <summary>
        /// Gets the workflow processes URI at the HyperThought instance.
        /// </summary>
        protected Uri GetProcessesUri() => new Uri(GetApiUri(), "process/");
        /// <summary>
        /// Gets a workflow process URI for a specified process ID at the HyperThought instance.
        /// </summary>
        protected Uri GetProcessUri(Guid processId) => new Uri(GetProcessesUri(), $"{processId}/");
        #endregion

        /// <summary>
        /// Initializes an instance of the <see cref="HyperthoughtWorkflowApi"/> class with the specified base API.
        /// </summary>
        /// <param name="api">The base HyperThought API.</param>
        public HyperthoughtWorkflowApi(HyperthoughtApi api)
        {
            Api = api;
        }

        #region Workflow Templates CRUD
        /// <summary>
        /// Obtains the list of HyperThought workflow templates that exist within a HyperThought project specified by
        /// project ID.
        /// </summary>
        /// <param name="projectId">The project ID.</param>
        /// <returns>The list of workflow templates obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtWorkflowTemplate>> GetWorkflowTemplatesAsync(Guid projectId)
        {
            Uri requestUri = new Uri(GetProcessesUri(), $"templates/?project={projectId}");
            return await Api.GetJsonObjectAsync<IList<HyperthoughtWorkflowTemplate>>(requestUri);
        }
        /// <summary>
        /// Obtains the list of HyperThought workflow templates that exist within a HyperThought project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns>The list of workflow templates obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtWorkflowTemplate>> GetWorkflowTemplatesAsync(HyperthoughtProject project)
        {
            return await GetWorkflowTemplatesAsync(project.Content.PrimaryKey);
        }
        #endregion

        #region Process CRUD
        /// <summary>
        /// Obtains a HyperThought process specified by process ID.
        /// </summary>
        /// <param name="processId">The process ID.</param>
        /// <returns>The process obtained from the HyperThought API.</returns>
        public async Task<HyperthoughtProcess> GetProcessAsync(Guid processId)
        {
            Uri requestUri = GetProcessUri(processId);
            return await Api.GetJsonObjectAsync<HyperthoughtProcess>(requestUri);
        }
        /// <summary>
        /// Obtains a HyperThought process specified by the workflow template.
        /// </summary>
        /// <param name="template">The workflow template.</param>
        /// <returns>The process obtained from the HyperThought API.</returns>
        public async Task<HyperthoughtProcess> GetProcessAsync(HyperthoughtWorkflowTemplate template)
        {
            return await GetProcessAsync(template.PrimaryKey);
        }
        /// <summary>
        /// Obtains a HyperThought process specified by a dot-separated path.
        /// </summary>
        /// <param name="path">
        /// The dot-separated path in the form of <c>"Project.Template.WorkflowA.WorkflowB.Process"</c> and so forth.
        /// </param>
        /// <returns>The process obtained from the HyperThought API.</returns>
        public async Task<HyperthoughtProcess> GetProcessAsync(string path)
        {
            Guid processId = await GetProcessIdFromPathAsync(path);
            return await GetProcessAsync(processId);
        }

        /// <summary>
        /// Deletes a HyperThought process. Will also attempt to remove the deleted process from predecessors,
        /// successors, and the parent.
        /// </summary>
        /// <param name="process">The process to delete.</param>
        /// <returns>Nothing.</returns>
        public async Task DeleteProcessAsync(HyperthoughtProcess process)
        {
            // Remove remaining connections to process.
            await RemoveProcessConnectionsAsync(process);

            // Make the delete request for the process.
            Uri requestUri = GetProcessUri(process.Content.PrimaryKey);
            await Api.DeleteAsync(requestUri);
        }
        /// <summary>
        /// Deletes a HyperThought process specified by process ID. Will also attempt to remove the deleted process from
        /// predecessors, successors, and the parent.
        /// </summary>
        /// <param name="processId">The process ID.</param>
        /// <returns>Nothing.</returns>
        public async Task DeleteProcessAsync(Guid processId)
        {
            HyperthoughtProcess process = await GetProcessAsync(processId);
            await DeleteProcessAsync(process);
        }
        /// <summary>
        /// Deletes a HyperThought process specified by a dot-separated path. Will also attempt to remove the deleted
        /// process from predecessors, successors, and the parent.
        /// </summary>
        /// <param name="path">
        /// The dot-separated path in the form of <c>"Project.Template.WorkflowA.WorkflowB.Process"</c> and so forth.
        /// </param>
        /// <returns>Nothing.</returns>
        public async Task DeleteProcessAsync(string path)
        {
            HyperthoughtProcess process = await GetProcessAsync(path);
            await DeleteProcessAsync(process);
        }

        /// <summary>
        /// Updates a HyperThought process on the HyperThought instance.
        /// </summary>
        /// <param name="process">The process to update.</param>
        /// <returns>Nothing.</returns>
        public async Task UpdateProcessAsync(HyperthoughtProcess process)
        {
            // Construct the DTO before patching.
            HyperthoughtProcessContent content = process.Content;
            HyperthoughtPatchProcess processDto = new HyperthoughtPatchProcess(content)
            {
                Metadata = process.Metadata
            };

            // Make the PATCH request to update.
            Uri requestUri = GetProcessUri(process.Content.PrimaryKey);
            await Api.PatchJsonObjectAsync<HyperthoughtProcess>(requestUri, process);

            // Append new connections.
            await AppendProcessConnectionsAsync(process);
        }

        /// <summary>
        /// Creates a HyperThought process on the HyperThought instance.
        /// </summary>
        /// <param name="process">The process to create.</param>
        /// <returns>The HyperThought process resource created on the HyperThought instance..</returns>
        public async Task<HyperthoughtProcess> CreateProcessAsync(HyperthoughtProcess process)
        {
            // Construct the DTO before posting.
            HyperthoughtProcessContent content = process.Content;
            HyperthoughtPostProcess processDto = new HyperthoughtPostProcess(content)
            {
                Permissions = process.Permissions
            };

            // Make the POST request to create.
            Uri requestUri = GetProcessesUri();
            HyperthoughtProcess newProcess = await Api
                .PostJsonObjectAsync<HyperthoughtPostProcess, HyperthoughtProcess>(requestUri, processDto);

            // If we have extra fields assigned on the process, we need to make a PATCH request to update.
            if (process.Metadata is not null)
            {
                newProcess.Metadata = process.Metadata;
                await UpdateProcessAsync(newProcess);
            }

            // Append new connections.
            await AppendProcessConnectionsAsync(newProcess);

            return newProcess;
        }

        private async Task RemoveProcessConnectionsAsync(HyperthoughtProcess process)
        {
            // Get the process ID for use in other API calls.
            Guid processId = process.Content.PrimaryKey;

            // Make sure to delete from predecessors.
            if (process.Content.PredecessorIds is not null)
            {
                foreach (Guid predecessorId in process.Content.PredecessorIds)
                {
                    HyperthoughtProcess predecessorProcess = await GetProcessAsync(predecessorId);
                    if (predecessorProcess.Content.SuccessorIds is not null)
                    {
                        predecessorProcess.Content.SuccessorIds = predecessorProcess.Content.SuccessorIds
                            .Where(successorId => successorId != processId)
                            .ToList();
                        await UpdateProcessAsync(predecessorProcess);
                    }
                }
            }

            // Make sure to delete from successors.
            if (process.Content.SuccessorIds is not null)
            {
                foreach (Guid successorId in process.Content.SuccessorIds)
                {
                    HyperthoughtProcess successorProcess = await GetProcessAsync(successorId);
                    if (successorProcess.Content.PredecessorIds is not null)
                    {
                        successorProcess.Content.PredecessorIds = successorProcess.Content.PredecessorIds
                            .Where(predecessorId => predecessorId != processId)
                            .ToList();
                        await UpdateProcessAsync(successorProcess);
                    }
                }
            }

            // Make sure to delete from parent.
            if (process.Content.ParentProcessId is not null)
            {
                Guid parentId = process.Content.ParentProcessId.Value;
                HyperthoughtProcess parentProcess = await GetProcessAsync(parentId);
                if (parentProcess.Content.ChildrenIds is not null)
                {
                    parentProcess.Content.ChildrenIds = parentProcess.Content.ChildrenIds
                        .Where(childId => childId != processId)
                        .ToList();
                    await UpdateProcessAsync(parentProcess);
                }
            }
        }
        private async Task AppendProcessConnectionsAsync(HyperthoughtProcess process)
        {
            // Get the process ID for use in other API calls.
            Guid processId = process.Content.PrimaryKey;

            // Make sure to append to predecessors.
            if (process.Content.PredecessorIds is not null)
            {
                foreach (Guid predecessorId in process.Content.PredecessorIds)
                {
                    HyperthoughtProcess predecessorProcess = await GetProcessAsync(predecessorId);
                    HyperthoughtProcessContent predecessorContent = predecessorProcess.Content;
                    if (predecessorContent.SuccessorIds is null)
                        predecessorContent.SuccessorIds = new List<Guid>();
                    if (!predecessorContent.SuccessorIds.Contains(processId))
                    {
                        predecessorContent.SuccessorIds.Add(processId);
                        await UpdateProcessAsync(predecessorProcess);
                    }
                }
            }

            // Make sure to delete from successors.
            if (process.Content.SuccessorIds is not null)
            {
                foreach (Guid successorId in process.Content.SuccessorIds)
                {
                    HyperthoughtProcess successorProcess = await GetProcessAsync(successorId);
                    HyperthoughtProcessContent successorContent = successorProcess.Content;
                    if (successorContent.PredecessorIds is null)
                        successorContent.PredecessorIds = new List<Guid>();
                    if (!successorContent.PredecessorIds.Contains(processId))
                    {
                        successorContent.PredecessorIds.Add(processId);
                        await UpdateProcessAsync(successorProcess);
                    }
                }
            }

            // Make sure to delete from parent.
            if (process.Content.ParentProcessId is not null)
            {
                Guid parentId = process.Content.ParentProcessId.Value;
                HyperthoughtProcess parentProcess = await GetProcessAsync(parentId);
                HyperthoughtProcessContent parentContent = parentProcess.Content;
                if (parentContent.ChildrenIds is null)
                    parentContent.ChildrenIds = new List<Guid>();
                if (!parentContent.ChildrenIds.Contains(processId))
                {
                    parentContent.ChildrenIds.Add(processId);
                    await UpdateProcessAsync(parentProcess);
                }
            }
        }
        #endregion

        #region Workflow Children CRUD
        /// <summary>
        /// Obtains the list of children HyperThought processes that exist within a HyperThought workflow specified by
        /// workflow ID.
        /// </summary>
        /// <param name="workflowId">The workflow ID.</param>
        /// <returns>The list of children processes obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtProcess>> GetWorkflowChildrenProcessesAsync(Guid workflowId)
        {
            Uri requestUri = new Uri(GetProcessUri(workflowId), $"children/");
            return await Api.GetJsonObjectAsync<IList<HyperthoughtProcess>>(requestUri);
        }
        /// <summary>
        /// Obtains the list of children HyperThought processes that exist within a HyperThought template specified by
        /// the workflow template.
        /// </summary>
        /// <param name="template">The workflow template.</param>
        /// <returns>The list of children processes obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtProcess>> GetWorkflowChildrenProcessesAsync(HyperthoughtWorkflowTemplate template)
        {
            return await GetWorkflowChildrenProcessesAsync(template.PrimaryKey);
        }
        /// <summary>
        /// Obtains the list of children HyperThought processes that exist within a HyperThought workflow specified by
        /// workflow.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <returns>The list of children processes obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtProcess>> GetWorkflowChildrenProcessesAsync(HyperthoughtProcess workflow)
        {
            return await GetWorkflowChildrenProcessesAsync(workflow.Content.PrimaryKey);
        }
        #endregion

        #region Path Matching
        /// <summary>
        /// Obtains a process specified by a delimeter-separated path.
        /// </summary>
        /// <param name="path">
        /// The delimiter-separated path in the form of <c>"Project.Template.WorkflowA.WorkflowB.Process"</c> and so
        /// forth.
        /// </param>
        /// <param name="delimiter">The text to specify a delimeter. Defaults to <c>"."</c>.</param>
        /// <returns>
        /// The process specified by a path if it exists. Otherwise, returns <c>null</c>.
        /// </returns>
        public async Task<HyperthoughtProcess> GetProcessFromPathAsync(string path, string delimiter = ".")
        {
            // Get the parts of the path.
            if (path is null) return null;
            string[] pathParts = path.Split(delimiter);

            // These path parts are required.
            string projectPath = pathParts.Length > 0 ? pathParts[0] : null;
            string templatePath = pathParts.Length > 1 ? pathParts[1] : null;
            if (projectPath is null || templatePath is null) return null;

            // Get the project.
            HyperthoughtProject project = (await Api.Projects.GetProjectsAsync())
                .Where(project => project.Content.Title.ToLower() == projectPath.ToLower())
                .FirstOrDefault();
            if (project is null) return null;

            // Get the workflow template.
            HyperthoughtWorkflowTemplate template = (await GetWorkflowTemplatesAsync(project))
                .Where(template => template.Title.ToLower() == templatePath.ToLower())
                .FirstOrDefault();
            if (template is null) return null;

            // Get processes from the remaining path parts.
            HyperthoughtProcess retrievedProcess = await GetProcessAsync(template);
            for (int k = 2; k < pathParts.Length; k++)
            {
                HyperthoughtProcess process = (await GetWorkflowChildrenProcessesAsync(retrievedProcess.Content.PrimaryKey))
                    .Where(process => process.Content.Name.ToLower() == pathParts[k].ToLower())
                    .FirstOrDefault();
                if (process is null) return null;
                retrievedProcess = process;
            }
            return retrievedProcess;
        }
        /// <summary>
        /// Obtains the UUID for a process specified by a delimeter-separated path.
        /// </summary>
        /// <param name="path">
        /// The delimiter-separated path in the form of <c>"Project.Template.WorkflowA.WorkflowB.Process"</c> and so
        /// forth.
        /// </param>
        /// <param name="delimiter">The text to specify a delimeter. Defaults to <c>"."</c>.</param>
        /// <returns>
        /// The UUID of the process specified by a path if it exists. Otherwise, returns <see cref="Guid.Empty"/>.
        /// </returns>
        public async Task<Guid> GetProcessIdFromPathAsync(string path, string delimiter = ".")
        {
            HyperthoughtProcess process = await GetProcessFromPathAsync(path, delimiter);
            if (process is null) return Guid.Empty;
            else return process.Content.PrimaryKey;
        }
        #endregion

        #region DTOs
        /// <summary>
        /// Represents a data-transfer object for posting a HyperThought process.
        /// </summary>
        private class HyperthoughtPostProcess : HyperthoughtProcessContent
        {
            /// <summary>
            /// Initializes an instance of the <see cref="HyperthoughtPostProcess"/> class from an existing content
            /// instance.
            /// </summary>
            /// <param name="content">The existing process content.</param>
            public HyperthoughtPostProcess(HyperthoughtProcessContent content)
                : base(content) { }

            /// <summary>
            /// The permissions for the HyperThought process.
            /// </summary>
            [JsonPropertyName("permissions")]
            public HyperthoughtPermissions Permissions { get; set; }
        }
        /// <summary>
        /// Represents a data-transfer object for patching a HyperThought process.
        /// </summary>
        private class HyperthoughtPatchProcess : HyperthoughtProcessContent
        {
            /// <summary>
            /// Initializes an instance of the <see cref="HyperthoughtPostProcess"/> class from an existing content
            /// instance.
            /// </summary>
            /// <param name="content">The existing process content.</param>
            public HyperthoughtPatchProcess(HyperthoughtProcessContent content)
                : base(content) { }

            /// <summary>
            /// The metadata for the HyperThought process.
            /// </summary>
            [JsonPropertyName("metadata")]
            public List<HyperthoughtMetadata> Metadata { get; set; }
        }
        #endregion
    }
}