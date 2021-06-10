using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using CartaCore.Serialization.Json;
using CartaCore.Persistence;
using CartaCore.Workflow;

using CartaWeb.Models.Data;
using CartaWeb.Models.DocumentItem;

using NUlid;

namespace CartaWeb.Controllers
{
    /// <summary>
    /// Manages creation, retrieval, updating, and deletion of workflow and workflow operation objects. The workflow
    /// operations are selector-action pairs. More information on the structure of selectors and actions can be
    /// retrieved from the workflow API. Workflows have certain user permissions associated with them allowing only
    /// certain users access to specific workflows. Each workflow is automatically assigned an identifier by the server.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowController : ControllerBase
    {
        /// <summary>
        /// Options for serialization
        /// </summary>
        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        static WorkflowController()
        {
            JsonOptions.PropertyNameCaseInsensitive = false;
            JsonOptions.IgnoreNullValues = true;
            JsonOptions.Converters.Insert(0, new JsonDiscriminantConverter());
        }

        /// <summary>
        /// The logger for this controller.
        /// </summary>
        private readonly ILogger<WorkflowController> _logger;

        /// <summary>
        /// The NoSQL DB context for this controller
        /// </summary>
        private readonly INoSqlDbContext _noSqlDbContext;

        /// <inheritdoc />
        public WorkflowController(ILogger<WorkflowController> logger, INoSqlDbContext noSqlDbContext)
        {
            _logger = logger;
            _noSqlDbContext = noSqlDbContext;

        }

        /// <summary>
        /// Returns a workflow deserialized from a workflow item JSON string.
        /// </summary>
        /// <param name="workflowItemJsonString">A workflow item JSON string.</param>
        /// <returns>
        /// The workflow including version number and identifier.
        /// </returns>
        protected static WorkflowItem GetWorkflowItem(string workflowItemJsonString)
        {
            WorkflowItem workflowItem = JsonSerializer.Deserialize<WorkflowItem>(workflowItemJsonString, JsonOptions);
            if (workflowItem.Workflow.Id is null) workflowItem.Workflow.Id = workflowItem.Id;
            workflowItem.Workflow.VersionNumber = workflowItem.VersionInformation.Number;
            return workflowItem;
        }

        /// <summary>
        /// Returns the current version number of the workflow with the given identifier.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="id">The identifier of the workflow.</param>
        /// <returns>
        /// The current version number.
        /// </returns>
        protected static async Task<int> GetCurrentWorkflowVersionNumber(string userId, string id)
        {
            string jsonString = await _noSqlDbContext.LoadDocumentStringAsync
            (
                Keys.GetUserKey(userId),
                Keys.GetWorkflowAccessKey(id)
            );
            if (jsonString is null) return 0;
            WorkflowAccessItem workflowAccessItem =
                JsonSerializer.Deserialize<WorkflowAccessItem>(jsonString, JsonOptions);
            return workflowAccessItem.VersionInformation.Number;
        }

        /// <summary>
        /// Loads all of the stored workflows asynchronously.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <returns>A list of all stored workflows.</returns>
        protected static async Task<List<Workflow>> LoadWorkflowsAsync(string userId)
        {
            List<Workflow> workflows = new() { };

            // Retrieve temporary working versions of workflows
            string partitionKey = Keys.GetUserKey(userId);
            string sortKeyPrefix = Keys.GetWorkflowKey("");
            List<string> jsonStrings = await _noSqlDbContext.LoadDocumentStringsAsync(partitionKey, sortKeyPrefix);
            foreach (string jsonString in jsonStrings)
            {
                workflows.Add(GetWorkflowItem(jsonString).Workflow);
            }

            // Retrieve persisted workflows
            partitionKey = Keys.GetUserKey(userId);
            sortKeyPrefix = Keys.GetWorkflowAccessKey("");
            jsonStrings = await _noSqlDbContext.LoadDocumentStringsAsync(partitionKey, sortKeyPrefix);
            foreach (string jsonString in jsonStrings)
            {
                WorkflowAccessItem workflowAccessItem =
                    JsonSerializer.Deserialize<WorkflowAccessItem>(jsonString, JsonOptions);
                WorkflowItem workflowItem =
                    await LoadWorkflowAsync(workflowAccessItem.Id, workflowAccessItem.VersionInformation.Number);
                if (workflowItem != null) workflows.Add(workflowItem.Workflow);
            }

            return workflows;
        }

        /// <summary>
        /// Loads a single workflow asynchronously for the given workflow identifier.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="id">The identifier of the workflow to get.</param>
        /// <param name="versionNumber">The version number of the workflow to get. If not specified, the user's
        /// temporary working version of the workflow is returned if it exists. If no temporary working version exist,
        /// the current version of the workflow is returned.</param>
        /// <returns>
        /// The loaded workflow or <c>null</c> if there is no workflow corresponding to the specified identifier.
        /// </returns>
        public static async Task<WorkflowItem> LoadWorkflowAsync(string userId, string id, int? versionNumber)
        {
            if (versionNumber.HasValue)
            {
                return await LoadWorkflowAsync(id, versionNumber.Value);
            }
            else
            {
                WorkflowItem workflowItem = await LoadTemporaryWorkflowAsync(userId, id);
                if (workflowItem is not null)
                {
                    Workflow workflow = workflowItem.Workflow;
                    workflow.Id = workflowItem.Id;
                    workflow.VersionNumber = workflowItem.VersionInformation.Number;
                    return workflowItem;
                }
                else
                {
                    int nr = await GetCurrentWorkflowVersionNumber(userId, id);
                    return await LoadWorkflowAsync(id, nr);
                }
            }
        }

        /// <summary>
        /// Loads a single workflow item asynchronously for the specified workflow identifier.
        /// </summary>
        /// <param name="id">The identifier of the workflow to get.</param>
        /// <param name="versionNumber">The version number of the workflow to get.
        /// Defaults to the current version (0) if not set.</param>
        /// <returns>
        /// The loaded workflow or <c>null</c> if there is no workflow corresponding to the specified identifier.
        /// </returns>
        protected static async Task<WorkflowItem> LoadWorkflowAsync(string id, int versionNumber)
        {
            string partitionKey = Keys.GetWorkflowKey(id);
            string sortKey = Keys.GetVersionKey(versionNumber);
            string jsonString = await _noSqlDbContext.LoadDocumentStringAsync(partitionKey, sortKey);
            if (jsonString is null) return null;
            else return GetWorkflowItem(jsonString);
        }

        /// <summary>
        /// Loads the temporary working version of single workflow item.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="id">The identifier of the workflow to get.</param>
        /// <param name="versionNumber">The version number of the workflow to get. If not specified, the user's
        /// temporary working version of the workflow is returned if it exists. If no temporary working version exist,
        /// the current version of the workflow is returned.</param>
        /// <param name="noSqlDbContext">The database context driver.</param>
        /// <returns>
        /// The loaded workflow or <c>null</c> if there is no workflow corresponding to the specified identifier.
        /// </returns>
        public static async Task<WorkflowItem> LoadWorkflowAsync(
            string userId,
            string id,
            int? versionNumber,
            INoSqlDbContext noSqlDbContext
        )
        {
            if (versionNumber.HasValue)
            {
                return await LoadWorkflowAsync(id, versionNumber.Value, noSqlDbContext);
            }
            else
            {
                WorkflowItem workflowItem = await LoadTemporaryWorkflowAsync(userId, id, noSqlDbContext);
                if (workflowItem is not null)
                {
                    Workflow workflow = workflowItem.Workflow;
                    workflow.Id = workflowItem.Id;
                    workflow.VersionNumber = workflowItem.VersionInformation.Number;
                    return workflowItem;
                }
                else
                {
                    int nr = await GetCurrentWorkflowVersionNumber(userId, id, noSqlDbContext);
                    return await LoadWorkflowAsync(id, nr, noSqlDbContext);
                }
            }
        }

        /// <summary>
        /// Loads a single workflow item asynchronously for the specified workflow identifier.
        /// </summary>
        /// <param name="id">The identifier of the workflow to get.</param>
        /// <param name="versionNumber">The version number of the workflow to get.
        /// Defaults to the current version (0) if not set.</param>
        /// <param name="noSqlDbContext">The database context driver.</param>
        /// <returns>
        /// The loaded workflow or <c>null</c> if there is no workflow corresponding to the specified identifier.
        /// </returns>
        protected static async Task<WorkflowItem> LoadTemporaryWorkflowAsync(string userId, string id)
        {
            string partitionKey = Keys.GetUserKey(userId);
            string sortKey = Keys.GetWorkflowKey(id);
            string jsonString = await _noSqlDbContext.LoadDocumentStringAsync(partitionKey, sortKey);
            if (jsonString is null) return null;
            else return GetWorkflowItem(jsonString);
        }

        /// <summary>
        /// Retrieves version information of all versions available for the specified workflow.
        /// </summary>
        /// <param name="id">The identifier of the workflow</param>
        /// <returns>
        /// A list containing version information, ordered by decreasing version number
        /// </returns>
        protected static async Task<List<VersionInformation>> LoadWorkflowVersionsAsync(string id)
        {
            List<VersionInformation> list = new() { };

            // Retrieve version information
            string partitionKey = Keys.GetWorkflowKey(id);
            string sortKeyPrefix = Keys.GetVersionKeyPrefix();
            List<string> jsonStrings = await _noSqlDbContext.LoadDocumentStringsAsync(partitionKey, sortKeyPrefix);
            foreach (string jsonString in jsonStrings)
            {
                list.Add(JsonSerializer.Deserialize<WorkflowItem>(jsonString, JsonOptions).VersionInformation);
            }

            // Sort the list by version number (decreasing)
            list.Sort((p, q) => q.Number.CompareTo(p.Number));

            return list;
        }

        /// <summary>
        /// Saves a single workflow asynchronously under the given version
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="id">The unique identifier for the workflow.</param>
        /// <param name="workflow">The workflow object.</param>
        /// <param name="versionInformation">Workflow version information.</param>
        protected static async Task SaveWorkflowAsync(
            string userId,
            string id,
            Workflow workflow,
            VersionInformation versionInformation)
        {
            await _noSqlDbContext.SaveDocumentStringAsync
            (
                Keys.GetWorkflowKey(id),
                Keys.GetVersionKey(versionInformation.Number),
                JsonSerializer.Serialize<WorkflowItem>(new WorkflowItem(workflow, versionInformation), JsonOptions)
            );
        }

        /// <summary>
        /// Saves a single workflow asynchronously as the user's temporary working version.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="id">The unique identifier for the workflow.</param>
        /// <param name="workflow">The workflow object.</param>
        /// <param name="versionInformation">Workflow version information.</param>
        protected static async Task SaveTemporaryWorkflowAsync(
            string userId,
            string id,
            Workflow workflow,
            VersionInformation versionInformation)
        {
            // Make sure that the operations list exists.
            workflow.Operations = workflow.Operations ?? new List<WorkflowOperation>();

            // Write the item.
            await _noSqlDbContext.SaveDocumentStringAsync
            (
                Keys.GetUserKey(userId),
                Keys.GetWorkflowKey(id),
                JsonSerializer.Serialize<WorkflowItem>(new WorkflowItem(workflow, versionInformation), JsonOptions)
            );

            // Delete access through the workflow access item
            await DeleteWorkflowAsync(userId, id);
        }

        /// <summary>
        /// Updates a single workflow asynchronously specified by an identifier and version number by merging in another
        /// workflow and its properties.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="id">The identifier of the workflow to update.</param>
        /// <param name="workflow">The workflow whose properties should be merged in.</param>
        /// <param name="versionNumber">The version number of the workflow whose properties should be merged in.</param>
        /// <returns>The updated workflow.</returns>
        protected static async Task<Workflow> UpdateWorkflowAsync(
            string userId,
            string id,
            Workflow workflow,
            int? versionNumber)
        {
            // We get the stored workflow first so we can perform updates on it.
            WorkflowItem storedWorkflowItem = await LoadWorkflowAsync(userId, id, versionNumber);
            Workflow storedWorkflow = null;

            if (storedWorkflowItem is null)
                // The stored workflow does not exist so we just assign the entire object.
                storedWorkflow = workflow;
            else
            {
                // Copy over all the properties if specified on the updating object.
                storedWorkflow = storedWorkflowItem.Workflow;
                storedWorkflow.Name = workflow.Name ?? storedWorkflow.Name;
                storedWorkflow.Operations = workflow.Operations ?? storedWorkflow.Operations;
            }
            await SaveTemporaryWorkflowAsync(userId, id, workflow, storedWorkflowItem.VersionInformation);
            return storedWorkflow;
        }

        /// <summary>Updates the version that a user has access to.</summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="id">The unique identifier for the workflow.</param>
        /// <param name="workflowName">The workflow name.</param>
        /// <param name="versionInformation">Version information for the workflow.</param>
        protected static async Task UpdateWorkflowAccessAsync(
            string userId,
            string id,
            string workflowName,
            VersionInformation versionInformation)
        {
            // Persist that the user has access to the workflow
            WorkflowAccessItem workflowAccessItem = new WorkflowAccessItem
            (
                id,
                workflowName,
                versionInformation
            );
            string jsonString = JsonSerializer.Serialize<WorkflowAccessItem>(workflowAccessItem, JsonOptions);
            await _noSqlDbContext.SaveDocumentStringAsync
            (
                Keys.GetUserKey(userId),
                Keys.GetWorkflowAccessKey(id),
                jsonString
            );

            // Delete access through the temporary workflow item
            await DeleteTemporaryWorkflowAsync(userId, id);
        }

        /// <summary>
        /// Deletes a user's temporary workflow specified by an identifier.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="id">The identifier of the workflow to delete.</param>
        protected static async Task<bool> DeleteTemporaryWorkflowAsync(string userId, string id)
        {
            return await _noSqlDbContext.DeleteDocumentStringAsync(Keys.GetUserKey(userId), Keys.GetWorkflowKey(id));
        }


        /// <summary>
        /// Deletes a user's access to a workflow identified by an identifier.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="id">The identifier of the workflow to delete.</param>
        protected static async Task<bool> DeleteWorkflowAsync(string userId, string id)
        {
            return await _noSqlDbContext.DeleteDocumentStringAsync
            (
                Keys.GetUserKey(userId),
                Keys.GetWorkflowAccessKey(id)
            );
        }


        /// <summary>
        /// Gets a list of workflow objects that the user has access to. 
        /// </summary>
        /// <request name="Example"></request>
        /// <returns status="200">A list of accessible workflow objects.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<Workflow>>> GetWorkflows()
        {
            return Ok(await LoadWorkflowsAsync(new UserInformation(User).Id));
        }

        /// <summary>
        /// Gets a workflow object specified by an identifier and optional version number. 
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <param name="nr">The version number of the workflow. If not specified, the user's temporary working
        /// version of the workflow is returned if it exists. If no temporary working version exist, the current
        /// version of the workflow is returned.</param>
        /// <request name="Example">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <request name="Example with version number 3">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="nr">3</arg>
        /// </request>
        /// <returns status="200">The workflow object with the specified identifier.</returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier and version.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Workflow>> GetWorkflow(
            [FromRoute] string id,
            [FromQuery(Name = "workflowVersion")] int? nr
        )
        {
            WorkflowItem workflowItem = await LoadWorkflowAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), id, nr);
            if (workflowItem is null) return NotFound();
            else return Ok(workflowItem.Workflow);
        }

        /// <summary>
        /// Returns a list stucture containing version information of all available versions for the workflow with
        /// the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <request name="Example">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <returns status="200">A list structure containing version information is returned. The list is ordered
        /// by decreasing order of version number, except for the first element which will always contain
        /// the current version.
        /// </returns>
        ///  <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier and version.
        /// </returns>
        [Authorize]
        [HttpGet("{id}/versions")]
        public async Task<ActionResult<List<VersionInformation>>> GetWorkflowVersions(
            [FromRoute] string id
        )
        {
            // Retrieve version information (in decreasing order)
            List<VersionInformation> list = await LoadWorkflowVersionsAsync(id);

            // Get the current version number for the user
            int nr = await GetCurrentWorkflowVersionNumber(User.FindFirstValue(ClaimTypes.NameIdentifier), id);
            if (nr == 0) return NotFound();

            // Put the current version entry at the top of the list
            foreach (VersionInformation version in list)
            {
                if (version.Number == nr)
                {
                    list.Remove(version);
                    list.Insert(0, version);
                    break;
                } 
            }

            return list;
        }

        /// <summary>
        /// Creates a new workflow object. The user will initially have all permission access to this workflow. If
        /// properties of the workflow are not specified, they will be filled in with defaults.
        /// </summary>
        /// <param name="workflow">The workflow to create.</param>
        /// <request name="Empty">
        ///     <body>{}</body>
        /// </request>
        /// <request name="Named">
        ///     <body>
        ///     {
        ///         "name": "Example New Workflow"
        ///     }
        ///     </body>
        /// </request>
        /// <request name="With Operations">
        ///     <body>
        ///     {
        ///         "name": "Example New Workflow with Operations",
        ///         "operations":
        ///         [
        ///             {
        ///                 "selector": { "type": "all" },
        ///                 "action": { "type": "increment", "amount": 5 }
        ///             },
        ///             {
        ///                 "selector":
        ///                 {
        ///                     "type": "property range",
        ///                     "property": "foo",
        ///                     "maximum": 10
        ///                 },
        ///                 "action": { "type": "increment" }
        ///             }
        ///         ]
        ///     }    
        ///     </body>
        /// </request>
        /// <returns status="200">
        /// The workflow object that was created. An automatically generated identifier will be attached to the returned
        /// object.
        /// </returns>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Workflow>> PostWorkflow(
            [FromBody] Workflow workflow
        )
        {
            // Create the workflow item
            WorkflowItem workflowItem = new WorkflowItem
            (
                workflow,
                new VersionInformation(0, null, new UserInformation(User))
            );

            // Write the item.
            string json = JsonSerializer.Serialize<WorkflowItem>(workflowItem, JsonOptions);
            string id = await _noSqlDbContext.CreateDocumentStringAsync
            (
                Keys.GetUserKey(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                Keys.GetWorkflowKey(""),
                json
            );

            // Return the workflow with the created ID set
            workflow.Id = id;
            return Ok(workflow);
        }

        /// <summary>
        /// Saves the temporary working version of a workflow as a new version.
        /// </summary>
        /// <param name="id">The workflow identifier.</param>
        /// <param name="description">A text description of the version changes.</param>
        /// <request name="Example">
        ///     <body>"Changed selection criteria of graph"</body>
        /// </request>
        /// <returns status="200">
        /// Version information of the newly created version will be attached to the returned object.
        /// </returns>
        /// <returns status="404">
        /// Occurs if the workflow with the specified identifier has no temporary working version.
        /// </returns>
        [Authorize]
        [HttpPost("{id}/versions")]
        public async Task<ActionResult<VersionInformation>> PostWorkflowVersion(
            [FromRoute] string id,
            [FromBody] string description
        )
        {
            // Retrieve the temporary working version of a workflow
            WorkflowItem workflowItem = await LoadTemporaryWorkflowAsync
            (
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                id
            );
            if (workflowItem is null)
            {
                _logger.LogWarning($"Temporary workflow {id} could not be found for creating a new version");
                return NotFound();
            }

            // Get the maximum version number
            int maxVersionNumber = 0;
            List <VersionInformation> versionInformationList = await LoadWorkflowVersionsAsync(id);
            if (versionInformationList.Count > 0) maxVersionNumber = versionInformationList[0].Number;
                
            // Update version information
            workflowItem.VersionInformation.BaseNumber = workflowItem.VersionInformation.Number;
            workflowItem.VersionInformation.Number = ++maxVersionNumber;
            workflowItem.VersionInformation.Description = description;
            workflowItem.VersionInformation.CreatedBy = new UserInformation(User);
            workflowItem.VersionInformation.DateCreated = DateTime.Now;

            // Persist the new version
            await SaveWorkflowAsync
            (
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                id,
                workflowItem.Workflow,
                workflowItem.VersionInformation
            );

            // Persist that the user has access to that workflow and version
            await UpdateWorkflowAccessAsync
            (
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                id,
                workflowItem.Workflow.Name,
                workflowItem.VersionInformation
            );

            // Return the version information
            return Ok(workflowItem.VersionInformation);
        }

        /// <summary>
        /// Reverts a workflow to a previous version.
        /// </summary>
        /// <param name="id">The workflow identifier.</param>
        /// <param name="nr">The version number the workflow should be reverted to.</param>
        /// <request name="Example with version number 3">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="nr">3</arg>
        /// </request>
        /// <returns status="200">
        /// Returns version information of the version the workflow was reverted to.
        /// </returns>
        /// <returns status="403">
        /// Occurs if the version number is not a positive integer
        /// </returns>
        /// <returns status="404">
        /// Occurs if the workflow with the specified identifier and version number does not exist. 
        /// </returns>
        [Authorize]
        [HttpPatch("{id}/versions")]
        public async Task<ActionResult<VersionInformation>> PatchWorkflowVersion(
            [FromRoute] string id,
            [FromQuery(Name = "workflowVersion")] int nr
        )
        {
            // Check that the version number is a valid version
            if (nr <= 0) return BadRequest();

            // Get the workflow and version information of the specified version number
            WorkflowItem workflowItem = await LoadWorkflowAsync(id, nr);
            if (workflowItem is null) return NotFound();

            // Persist the version that the user has access to
            await UpdateWorkflowAccessAsync
            (
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                id,
                workflowItem.Workflow.Name,
                workflowItem.VersionInformation
            );

            // Return version information
            return workflowItem.VersionInformation;
        }

        /// <summary>
        /// Deletes an existing workflow object specified by an identifier from the user's view 
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <request name="Example">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <returns status="200">Nothing.</returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier.
        /// </returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWorkflow(
            [FromRoute] string id
        )
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool deleted = await DeleteTemporaryWorkflowAsync(userId, id);
            if (!deleted) deleted = await DeleteWorkflowAsync(userId, id);
            if (deleted) return Ok(); else return NotFound();
        }

        /// <summary>
        /// Gets the list of operations from a workflow object specified by an identifier and optional version number.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <param name="nr">The version number of the workflow. If not specified, the operations on the user's
        /// temporary working version of the workflow is returned if it exists. If no temporary working version exist,
        /// the operations on the current version of the workflow is returned.</param>
        /// <request name="Example">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <request name="Example with version number 3">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="nr">3</arg>
        /// </request>
        /// <returns status="200">The list of operations attached to the workflow.</returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier.
        /// </returns>
        [Authorize]
        [HttpGet("{id}/operations")]
        public async Task<ActionResult<List<WorkflowOperation>>> GetWorkflowOperations(
            [FromRoute] string id,
            [FromQuery(Name = "workflowVersion")] int? nr
        )
        {
            WorkflowItem workflowItem = await LoadWorkflowAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), id, nr);
            if (workflowItem is null) return NotFound();
            else return Ok(workflowItem.Workflow.Operations);
        }

        /// <summary>
        /// Gets an operation specified by index from a workflow object specified by an identifier and optional version
        /// number.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <param name="index">The index of the workflow operation.</param>
        /// <param name="nr">The version number of the workflow. If not specified, the operation on the user's
        /// temporary working version of the workflow is returned if it exists. If no temporary working version exist,
        /// the operation on the current version of the workflow is returned.</param>
        /// <request name="First">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="index">0</arg>
        /// </request>
        /// <request name="Last">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="index">-1</arg>
        /// </request>
        /// <request name="Example with version number 3">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="index">-1</arg>
        ///     <arg name="nr">3</arg>
        /// </request>
        /// <returns status="200">The specified operation attached to the workflow.</returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier and version or there is no
        /// workflow operation corresponding to the specified index.
        /// </returns>
        [Authorize]
        [HttpGet("{id}/operations/{index}")]
        public async Task<ActionResult<WorkflowOperation>> GetWorkflowOperation(
            [FromRoute] string id,
            [FromRoute] int index,
            [FromQuery(Name = "workflowVersion")] int? nr
        )
        {
            // Get the workflow.
            WorkflowItem workflowItem = await LoadWorkflowAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), id, nr);
            if (workflowItem is null) return NotFound();

            // Convert the index and get the operation.
            if (workflowItem.Workflow.Operations is null ||
                workflowItem.Workflow.Operations.Count == 0
            ) return NotFound();
            if (index < 0) index += workflowItem.Workflow.Operations.Count;
            if (index < 0 || index >= workflowItem.Workflow.Operations.Count) return NotFound();
            return Ok(workflowItem.Workflow.Operations[index]);
        }

        /// <summary>
        /// Inserts a new workflow operation at the optionally specified index into the list of operations of a workflow
        /// object specified by an identifier and optional version number. A new temporary working version
        /// of the workflow is created for the user.
        /// </summary>
        /// <param name="id">The identifier of the workflow</param>
        /// <param name="index">The index to insert the workflow operation at. If not specified, the
        /// operation appends the new operation at the end of the list of operations.</param>
        /// <param name="operation">The operation to insert. If not specified, they will be filled with defaults.
        /// </param>
        /// <param name="nr">The version number of the workflow. If not specified, the operation is inserted on the
        /// user's temporary working version of the workflow if it exists. If no temporary working version
        /// exist, the operation is inserted on the current version of the workflow.</param>
        /// <request name="Append">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <body>
        ///     {
        ///         "action":    
        ///         {
        ///             "type": "decrement"
        ///         }
        ///     }    
        ///     </body>
        /// </request>
        /// <request name="Insert Second">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="index">1</arg>
        ///     <body>
        ///     {
        ///         "name": "My New Operation",    
        ///         "selector":
        ///         {
        ///             "type": "vertex name",
        ///             "pattern": "bar"
        ///         },
        ///         "action":
        ///         {    
        ///             "type": "decrement",
        ///             "amount": 2
        ///         }
        ///     }
        ///     </body>
        /// </request>
        /// <request name="Append with version number 3">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="nr">3</arg>
        ///     <body>
        ///     {
        ///         "action":    
        ///         {
        ///             "type": "decrement"
        ///         }
        ///     }    
        ///     </body>
        /// </request>
        /// <returns status="200">
        /// The workflow operation object that was created.
        /// </returns>
        /// <returns status="400">
        /// Occurs when the index is out of range of the list of operations.
        /// </returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier.
        /// </returns>
        [Authorize]
        [HttpPost("{id}/operations/{index?}")]
        public async Task<ActionResult<WorkflowOperation>> InsertWorkflowOperation(
            [FromRoute] string id,
            [FromRoute] int? index,
            [FromBody] WorkflowOperation operation,
            [FromQuery(Name = "workflowVersion")] int? nr
        )
        {
            // Get the workflow.
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            WorkflowItem workflowItem = await LoadWorkflowAsync(userId, id, nr);
            if (workflowItem is null) return NotFound();

            // Try to perform the insertion.
            if (workflowItem.Workflow.Operations is null)
                workflowItem.Workflow.Operations = new List<WorkflowOperation>();
            if (index.HasValue)
            {
                if (index < 0) index += workflowItem.Workflow.Operations.Count;
                if (index < 0 || index > workflowItem.Workflow.Operations.Count) return BadRequest();
                workflowItem.Workflow.Operations.Insert(index.Value, operation);
            }
            else workflowItem.Workflow.Operations.Add(operation);
                
            // Save the changes to the workflow.
            await UpdateWorkflowAsync(userId, id, workflowItem.Workflow, nr);
            return Ok(operation);
        }

        /// <summary>
        /// Updates an existing workflow operation specified by an index in the list of operations of a workflow
        /// object specified by an identifier and optional version number. A new temporary working version
        /// of the workflow is created for the user. Any properties that are specified in the update will overwrite
        /// existing properties. If a property is not specified, it is not updated in the original object.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <param name="index">The index to update the workflow operation at.</param>
        /// <param name="operation">The operation to use to update the existing operation.</param>
        /// <param name="nr">The version number of the workflow. If not specified, the user's temporary working
        /// version of the workflow is updated. If no temporary working version exist, the current version of the
        /// workflow is updated.</param>
        /// <request name="Update First with Name">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="index">0</arg>
        ///     <body>
        ///     {
        ///         "name": "Updated Operation Name"
        ///     }
        ///     </body>
        /// </request>
        /// <request name="Update Last with Selector">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="index">-1</arg>
        ///     <body>
        ///     {
        ///         "selector": { "type": "none" }
        ///     }
        ///     </body>
        /// </request>
        /// <request name="Update with version number 3">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="index">0</arg>
        ///     <arg name="nr">3</arg>
        ///     <body>
        ///     {
        ///         "name": "Updated Operation Name"
        ///     }
        ///     </body>
        /// </request>
        /// <returns status="200">
        /// The workflow operation object after updates are applied.
        /// </returns>
        /// <returns status="400">
        /// Occurs when the index is out of range of the list of operations.
        /// </returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier.
        /// </returns>
        [Authorize]
        [HttpPatch("{id}/operations/{index}")]
        public async Task<ActionResult<WorkflowOperation>> PatchWorkflowOperation(
            [FromRoute] string id,
            [FromRoute] int index,
            [FromBody] WorkflowOperation operation,
            [FromQuery(Name = "workflowVersion")] int? nr
        )
        {
            // Get the workflow.
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            WorkflowItem workflowItem = await LoadWorkflowAsync(userId, id, nr);
            if (workflowItem is null) return NotFound();

            // Try to perform the update.
            if (workflowItem.Workflow.Operations is null) return BadRequest();
            if (index < 0) index += workflowItem.Workflow.Operations.Count;
            if (index < 0 || index >= workflowItem.Workflow.Operations.Count) return BadRequest();

            workflowItem.Workflow.Operations[index].Name =
                operation.Name ?? workflowItem.Workflow.Operations[index].Name;
            workflowItem.Workflow.Operations[index].Selector =
                operation.Selector ?? workflowItem.Workflow.Operations[index].Selector;
            workflowItem.Workflow.Operations[index].Actor =
                operation.Actor ?? workflowItem.Workflow.Operations[index].Actor;

            // Save the changes to the workflow.
            await UpdateWorkflowAsync(userId, id, workflowItem.Workflow, nr);
            return Ok(workflowItem.Workflow.Operations[index]);
        }

        /// <summary>
        /// Removes an operation optionally specified by index from a workflow object specified by an identifier and
        /// optional version number. If the index of the operation is not specified, the operation removes the last
        /// operation in the workflow.
        /// </summary>
        /// <param name="id">The identifier of the workflow.</param>
        /// <param name="index">The index of the workflow operation.</param>
        /// <param name="nr">The version number of the workflow. If not specified, the operation is deleted from the
        /// user's temporary working version of the workflow is returned if it exists. If no temporary working version
        /// exist, the operation is removed from the current version of the workflow.</param>
        /// <request name="Remove Last">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <request name="Remove Second">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="index">1</arg>
        /// </request>
        /// <request name="Remove with version number 3">
        ///     <arg name="id">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="nr">3</arg>
        /// </request>
        /// <returns status="200">Nothing.</returns>
        /// <returns status="404">
        /// Occurs when there is no workflow corresponding to the specified identifier or there is no workflow operation
        /// corresponding to the specified index.
        /// </returns>
        [Authorize]
        [HttpDelete("{id}/operations/{index?}")]
        public async Task<ActionResult> RemoveWorkflowOperation(
            [FromRoute] string id,
            [FromRoute] int? index,
            [FromQuery(Name = "workflowVersion")] int? nr
        )
        {
            // Get the workflow.
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            WorkflowItem workflowItem = await LoadWorkflowAsync(userId, id, nr);
            if (workflowItem is null) return NotFound();

            // Try to perform the deletion.
            if (workflowItem.Workflow.Operations is null) return BadRequest();
            if (index.HasValue)
            {
                if (index < 0) index += workflowItem.Workflow.Operations.Count;
                if (index < 0 || index >= workflowItem.Workflow.Operations.Count) return BadRequest();
                workflowItem.Workflow.Operations.RemoveAt(index.Value);
            }
            else if (workflowItem.Workflow.Operations.Count > 0)
                workflowItem.Workflow.Operations.RemoveAt(workflowItem.Workflow.Operations.Count - 1);

            // Save the changes to the workflow.
            await UpdateWorkflowAsync(userId, id, workflowItem.Workflow, nr);
            return Ok();
        }
    }
}
