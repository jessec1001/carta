using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


using CartaCore.Serialization.Json;
using CartaCore.Persistence;
using CartaWeb.Models.Data;
using CartaWeb.Models.DocumentItem;

namespace CartaWeb.Controllers
{
    /// <summary>
    /// Manages creation, retrieval, and archiving of workspaces by the signed in user, and sharing of workspaces
    /// with other users.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WorkspaceController : ControllerBase
    {
        /// <summary>
        /// Options for serialization
        /// </summary>
        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        /// <summary>
        /// Static constructor for initializing JSON serialization/deserialization options
        /// </summary>
        static WorkspaceController()
        {
            JsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            JsonOptions.PropertyNameCaseInsensitive = false;
            JsonOptions.Converters.Insert(0, new JsonDiscriminantConverter());
        }

        /// <summary>
        /// The logger for this controller.
        /// </summary>
        private readonly ILogger<WorkspaceController> _logger;

        /// <summary>
        /// The NoSQL DB context for this controller
        /// </summary>
        private readonly INoSqlDbContext _noSqlDbContext;

        /// <inheritdoc />
        public WorkspaceController(ILogger<WorkspaceController> logger, INoSqlDbContext noSqlDbContext)
        {
            _logger = logger;
            _noSqlDbContext = noSqlDbContext;

        }

        /// <summary>
        /// Persists a new workspace, creating table items for the worskpace under the logged in user, and the user
        /// under the workspace.
        /// </summary>
        /// <param name="userItem">The user information.</param>
        /// <param name="workspaceItem">The workspace information.</param>
        /// <returns>
        /// A unique identifier for the workspace.
        /// </returns>
        protected async Task<string> CreateWorkspaceAsync(
            UserItem userItem,
            WorkspaceItem workspaceItem)
        {
            string json = JsonSerializer.Serialize<WorkspaceItem>(workspaceItem, JsonOptions);
            string id = await _noSqlDbContext.CreateDocumentStringAsync
            (
                Keys.GetUserKey(userItem.UserInformation.Id),
                Keys.GetWorkspaceKey(""),
                json
            );

            userItem.DocumentHistory.DateAdded = DateTime.Now;
            json = JsonSerializer.Serialize<UserItem>(userItem, JsonOptions);
            await _noSqlDbContext.SaveDocumentStringAsync
            (
                Keys.GetWorkspaceKey(id),
                Keys.GetUserKey(userItem.UserInformation.Id),
                json
            );

            return id;
        }

        /// <summary>
        /// Returns the information of all the workspaces the given user has access to.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        /// A list of workspace items.
        /// </returns>
        protected async Task<List<WorkspaceItem>> LoadWorkspaceItemsAsync(string userId)
        {
            string partitionKey = Keys.GetUserKey(userId);
            string sortKeyPrefix = Keys.GetWorkspaceKey("");
            List<string> jsonStrings = await _noSqlDbContext.LoadDocumentStringsAsync(partitionKey, sortKeyPrefix);
            List<WorkspaceItem> workspaceItems = new() { };
            foreach (string jsonString in jsonStrings)
            {
                workspaceItems.Add(JsonSerializer.Deserialize<WorkspaceItem>(jsonString, JsonOptions));
            }
            return workspaceItems;
        }

        /// <summary>
        /// Returns the workspace information for the given workspace identifier and user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <returns>
        /// A workspace item.
        /// </returns>
        protected async Task<WorkspaceItem> LoadWorkspaceItemAsync(string userId, string workspaceId)
        {
            string partitionKey = Keys.GetUserKey(userId);
            string sortKey = Keys.GetWorkspaceKey(workspaceId);
            string jsonString = await _noSqlDbContext.LoadDocumentStringAsync(partitionKey, sortKey);
            if (jsonString is null) return null;
            else return JsonSerializer.Deserialize<WorkspaceItem>(jsonString, JsonOptions);
        }

        /// <summary>
        /// Returns the information of all the users under the workspace.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <returns>
        /// A list of user items.
        /// </returns>
        protected async Task<List<UserItem>> LoadUserItemsAsync(string workspaceId)
        {
            string partitionKey = Keys.GetWorkspaceKey(workspaceId);
            string sortKeyPrefix = Keys.GetUserKey("");
            List<string> jsonStrings = await _noSqlDbContext.LoadDocumentStringsAsync(partitionKey, sortKeyPrefix);
            List<UserItem> userItems = new() { };
            foreach (string jsonString in jsonStrings)
            {
                userItems.Add(JsonSerializer.Deserialize<UserItem>(jsonString, JsonOptions));
            }
            return userItems;
        }

        /// <summary>
        /// Returns user information for the given workspace and user.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        /// The user item.
        /// </returns>
        protected async Task<UserItem> LoadUserItemAsync(string workspaceId, string userId)
        {
            string partitionKey = Keys.GetWorkspaceKey(workspaceId);
            string sortKey = Keys.GetUserKey(userId);
            string jsonString = await _noSqlDbContext.LoadDocumentStringAsync(partitionKey, sortKey);
            if (jsonString is null) return null;
            else return JsonSerializer.Deserialize<UserItem>(jsonString, JsonOptions);
        }

        /// <summary>
        /// Updates workspace and user information.
        /// </summary>
        /// <param name="userItem">The user information.</param>
        /// <param name="workspaceItem">The workspace information.</param>
        /// <returns>
        /// True if the update operations completed with no errors.
        /// </returns>
        protected async Task<bool> UpdateWorkspaceAsync(UserItem userItem, WorkspaceItem workspaceItem)
        {
            string json = JsonSerializer.Serialize<WorkspaceItem>(workspaceItem, JsonOptions);
            bool updated = await _noSqlDbContext.UpdateDocumentStringAsync
            (
                Keys.GetUserKey(userItem.UserInformation.Id),
                Keys.GetWorkspaceKey(workspaceItem.Id),
                json
            );
            if (!updated)
            {
                _logger.LogWarning($"Workspace item for user {userItem.UserInformation.Id} and workspace " +
                    $"{workspaceItem.Id} could not be found for update");
                return false;
            }

            json = JsonSerializer.Serialize<UserItem>(userItem, JsonOptions);
            updated = await _noSqlDbContext.UpdateDocumentStringAsync
            (
                Keys.GetWorkspaceKey(workspaceItem.Id),
                Keys.GetUserKey(userItem.UserInformation.Id),
                json
            );
            if (!updated) _logger.LogWarning($"User item for user {userItem.UserInformation.Id} and workspace " +
                $"{workspaceItem.Id} could not be found for update");
            return updated;
        }

        /// <summary>
        /// Adds a user to a workspace, adding table items for the workspace under the user, and the user
        /// under the workspace.
        /// </summary>
        /// <param name="userItem">The user information.</param>
        /// <param name="workspaceItem">The workspace information.</param>
        protected async Task SaveWorkspaceAsync(UserItem userItem, WorkspaceItem workspaceItem)
        {
            string json = JsonSerializer.Serialize<WorkspaceItem>(workspaceItem, JsonOptions);
            await _noSqlDbContext.SaveDocumentStringAsync
            (
                Keys.GetUserKey(userItem.UserInformation.Id),
                Keys.GetWorkspaceKey(workspaceItem.Id),
                json
            );

            json = JsonSerializer.Serialize<UserItem>(userItem, JsonOptions);
            await _noSqlDbContext.SaveDocumentStringAsync
            (
                Keys.GetWorkspaceKey(workspaceItem.Id),
                Keys.GetUserKey(userItem.UserInformation.Id),
                json
            );
        }

        /// <summary>
        /// Saves information on changes made to a workspace.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="workspaceChangeItem">The workspace change information.</param>
        protected async Task SaveWorkspaceChangeItemAsync(string workspaceId, WorkspaceChangeItem workspaceChangeItem)
        {
            string json = JsonSerializer.Serialize<WorkspaceChangeItem>(workspaceChangeItem, JsonOptions);
            await _noSqlDbContext.CreateDocumentStringAsync
            (
                Keys.GetWorkspaceKey(workspaceId),
                Keys.GetChangeKeyPrefix(workspaceChangeItem.ChangeType),
                json
            );
        }

        /// <summary>
        /// Loads data set information for the given workspace identifier and data set identifier
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="datasetId">The dataset identifier</param>
        protected async Task<DatasetItem> LoadWorkspaceDatasetAsync(
            string workspaceId,
            string datasetId
        )
        {
            string partitionKey = Keys.GetWorkspaceKey(workspaceId);
            string sortKey = Keys.GetDatasetKey(datasetId);
            string jsonString = await _noSqlDbContext.LoadDocumentStringAsync(partitionKey, sortKey);
            if (jsonString is null) return null;
            else return JsonSerializer.Deserialize<DatasetItem>(jsonString, JsonOptions);
        }

        /// <summary>Updates the workflow access information.</summary>
        /// <param name="id">The unique identifier for the workspace.</param>
        /// <param name="workflowId">The unique identifier for the workflow.</param>
        /// <param name="workflowName">The workflow name.</param>
        /// <param name="versionInformation">Version information for the workflow.</param>
        /// <returns>Workflow access information.</returns>
        protected async Task<WorkflowAccessItem> UpdateWorkflowAccessAsync(
            string id,
            string workflowId,
            string workflowName,
            VersionInformation versionInformation)
        {
            WorkflowAccessItem workflowAccessItem = new WorkflowAccessItem
            (
                workflowId,
                workflowName,
                versionInformation
            );
            workflowAccessItem.DocumentHistory = new DocumentHistory(new UserInformation(User));
            string jsonString = JsonSerializer.Serialize<WorkflowAccessItem>(workflowAccessItem, JsonOptions);
            await _noSqlDbContext.SaveDocumentStringAsync
            (
                Keys.GetWorkspaceKey(id),
                Keys.GetWorkflowAccessKey(workflowId),
                jsonString
            );
            return workflowAccessItem;
        }

        /// <summary>Updates the workflow access information.</summary>
        /// <param name="id">The unique identifier for the workspace.</param>
        /// <param name="workflowId">The unique identifier for the workflow.</param>
        /// <param name="workflowAccessItem">The workflow access item to persist.</param>
        protected async Task UpdateWorkflowAccessAsync(
            string id,
            string workflowId,
            WorkflowAccessItem workflowAccessItem)
        {
            string jsonString = JsonSerializer.Serialize<WorkflowAccessItem>(workflowAccessItem, JsonOptions);
            await _noSqlDbContext.SaveDocumentStringAsync
            (
                Keys.GetWorkspaceKey(id),
                Keys.GetWorkflowAccessKey(workflowId),
                jsonString
            );
        }

        /// <summary>
        /// Retrieves accees information on the specified workflow.
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <returns>Workflow access information.</returns>
        protected async Task<WorkflowAccessItem> LoadWorkflowAccessAsync(
            string id,
            string workflowId
        )
        {
            string partitionKey = Keys.GetWorkspaceKey(id);
            string sortKey = Keys.GetWorkflowAccessKey(workflowId);
            string jsonString = await _noSqlDbContext.LoadDocumentStringAsync(partitionKey, sortKey);
            if (jsonString is null) return null;
            else return (JsonSerializer.Deserialize<WorkflowAccessItem>(jsonString, JsonOptions));
        }

        /// <summary>
        /// Checks whether a specified workspace change date falls within from date and to date criteria.
        /// </summary>
        /// <param name="changeDate">The date and time of the workspace change.</param>
        /// <param name="dateFrom">From which date onwards should a change have occured in.</param>
        /// <param name="dateTo">Prior to which date should a change have occurred in.</param>
        /// <returns>True if the change date falls within the specified range, else false</returns>
        protected static bool IsDateInRange(DateTime changeDate, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if ((dateFrom is not null) & (changeDate < dateFrom)) return false;
            if ((dateTo is not null) & (changeDate > dateTo)) return false;
            return true;
        }

        /// <summary>
        /// Create a new workspace for the given user.
        /// </summary>
        /// <param name="name">The workspace name.</param>
        /// <request name="Example">
        ///     <arg name="name">MyWorkspace</arg>
        /// </request>
        /// <returns status="200">The workspace information will be attached to the returned object.</returns>
        /// <returns status="409">
        /// Occurs when the create operation fails unexpectedly due to a key conflict.
        /// </returns>
        [Authorize]
        [HttpPost("{name}")]
        public async Task<ActionResult<WorkspaceItem>> PostWorkspace(
            [FromRoute] string name
        )
        {
            UserInformation userInformation = new UserInformation(User);
            UserItem userItem = new UserItem(userInformation);
            userItem.DocumentHistory.AddedBy = userInformation;
            WorkspaceItem workspaceItem = new WorkspaceItem(name, userInformation);
            string id = await CreateWorkspaceAsync(userItem, workspaceItem);
            if (id is null) return Conflict();
            else
            {
                WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
                (
                    WorkspaceChangeEnumeration.Workspace,
                    id,
                    workspaceItem.Name,
                    WorkspaceActionEnumeration.Added,
                    userItem.UserInformation.Name
                );
                await SaveWorkspaceChangeItemAsync(id, workspaceChangeItem);
                workspaceItem.Id = id;
                return Ok(workspaceItem);
            }            
        }

        /// <summary>
        /// Get the workspace information for the given workspace identifier.
        /// </summary>
        /// <param name="id">A unique workspace identifier.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        /// </request>
        /// <returns status="200"> The workspace information will be attached to the returned object.
        /// </returns>
        /// <returns status="404">
        /// Occurs when the workspace item for the given ID cannot be found.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkspaceItem>> GetWorkspace(
            [FromRoute] string id
        )
        {
            WorkspaceItem workspaceItem = await LoadWorkspaceItemAsync(new UserInformation(User).Id, id);
            if (workspaceItem is null) return NotFound();
            else return Ok(workspaceItem);
        }

        /// <summary>
        /// Get the information of all the workspaces the user has access to.
        /// </summary>
        /// <param name="archived">A flag indicating whether archived workspaces should be returned.
        /// Set to true if archived workspaces should be returned, otherwise set to false.
        /// Defaults to false.</param>
        /// <request name="Retrieve Archived Workspaces">
        ///     <arg name="archived">true</arg>
        /// </request>
        /// <request name="Retrieve Active Workspaces">
        ///     <arg name="archived">false</arg>
        /// </request>
        /// <returns status="200">
        /// A list of workspace items will be attached to the returned object.
        /// </returns>
        /// <returns status="404">
        /// Occurs when no workspaces exist for the user.
        /// </returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<WorkspaceItem>>> GetWorkspaces(
            [FromQuery(Name = "archived")] bool? archived
        )
        {
            // If no archived query parameter has been passed, set it to false
            if (!archived.HasValue) archived = false;

            // Load the items and filter items according to archived flag
            List<WorkspaceItem> allWorkspaceItems = await LoadWorkspaceItemsAsync(new UserInformation(User).Id);
            if (allWorkspaceItems is null) return NotFound();
            else
            {
                List<WorkspaceItem> workspaceItems = new List<WorkspaceItem>();
                foreach (WorkspaceItem item in allWorkspaceItems)
                {
                    if (item.Archived == archived.Value) workspaceItems.Add(item);
                }

                return Ok(workspaceItems);
            }
        }

        /// <summary>
        /// Get a list of all the users that have access to a workspace
        /// </summary>
        /// <param name="id">The workspace identifier</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        /// </request>
        /// <returns status="200">
        /// A list of users will be attached to the returned object. 
        /// </returns>
        /// <returns status="404">
        /// Occurs when no users can be found. 
        /// </returns>
        [Authorize]
        [HttpGet("{id}/users")]
        public async Task<ActionResult<List<UserItem>>> GetWorkspaceUsers(
            [FromRoute] string id
        )
        {
            List<UserItem> userItems = await LoadUserItemsAsync(id);
            if (userItems is null) return NotFound();
            else return Ok(userItems);
        }

        /// <summary>
        /// Update the archived status of a workspace.
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="archived">Flag indicating whether the workspace should be archived (true)
        /// or not (false).</param>
        /// <request name="Archive a Workspace">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="archived">true</arg>
        /// </request>
        /// <request name="Unarchive a Workspace">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="archived">false</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.</returns>
        /// <returns status="404">
        /// Occurs when a workspace for the given identifier cannot be found. 
        /// </returns>
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<ActionResult<WorkspaceItem>> PatchWorkspace(
            [FromRoute] string id,
            [FromQuery(Name = "archived")] bool archived
        )
        {
            UserInformation userInformation = new UserInformation(User);
            WorkspaceItem workspaceItem = await LoadWorkspaceItemAsync(userInformation.Id, id);
            if (workspaceItem is null) return NotFound();
            UserItem userItem = await LoadUserItemAsync(id, userInformation.Id);
            if (userItem is null) return NotFound();
            workspaceItem.Archived = archived;
            WorkspaceChangeItem workspaceChangeItem = null;
            if (archived)
            {
                userItem.DocumentHistory.DateDeleted = DateTime.Now;
                userItem.DocumentHistory.DeletedBy = userInformation;
                workspaceItem.DocumentHistory.DateArchived = DateTime.Now;
                workspaceChangeItem = new WorkspaceChangeItem
                (
                    WorkspaceChangeEnumeration.User,
                    userItem.UserInformation.Id,
                    userItem.UserInformation.Name,
                    WorkspaceActionEnumeration.Removed,
                    userItem.UserInformation.Name
                );
            } else
            {
                userItem.DocumentHistory.DateAdded = DateTime.Now;
                userItem.DocumentHistory.AddedBy = userInformation;
                workspaceItem.DocumentHistory.DateUnarchived = DateTime.Now;
                workspaceChangeItem = new WorkspaceChangeItem
                (
                    WorkspaceChangeEnumeration.User,
                    userItem.UserInformation.Id,
                    userItem.UserInformation.Name,
                    WorkspaceActionEnumeration.Added,
                    userItem.UserInformation.Name
                );
            }
            bool updated = await UpdateWorkspaceAsync(userItem, workspaceItem);
            if (updated)
            {
                await SaveWorkspaceChangeItemAsync(id, workspaceChangeItem);
                return Ok(workspaceItem);
            }      
            else return NotFound();
        }

        /// <summary>
        /// Add users to the workspace. 
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="userItems">A list of users (identifier and user name) that should be added to the
        /// workspace.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <body>
        ///         [
        ///            {
        ///               "userInformation": {   
        ///                    "id":"userId1",
        ///                    "name":"user1"
        ///                }
        ///            },
        ///            {
        ///                "userInformation": {   
        ///                    "id":"userId2",
        ///                    "name":"user2"
        ///                }
        ///            }
        ///         ]
        ///     </body>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.</returns>
        [Authorize]
        [HttpPatch("{id}/users")]
        public async Task<ActionResult<List<UserItem>>> PatchWorkspaceUsers(
            [FromRoute] string id,
            [FromBody] List<UserItem> userItems
        )
        {
            UserInformation userInformation = new UserInformation(User);
            WorkspaceItem workspaceItem = await LoadWorkspaceItemAsync(userInformation.Id, id);
            if (workspaceItem is null) return NotFound();
            foreach (UserItem userItem in userItems)
            {
                userItem.DocumentHistory.DateAdded = DateTime.Now;
                userItem.DocumentHistory.AddedBy = userInformation;
                await SaveWorkspaceAsync(userItem, workspaceItem);
                WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
                (
                    WorkspaceChangeEnumeration.User,
                    userItem.UserInformation.Id,
                    userItem.UserInformation.Name,
                    WorkspaceActionEnumeration.Added,
                    userInformation.Name
                );
                await SaveWorkspaceChangeItemAsync(id, workspaceChangeItem);
            }
            return Ok(userItems);
        }

        /// <summary>
        /// Deletes a user from a workspace. 
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="users">A list of user identifiers.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="users">846817bf-5695-4cca-bd3b-64313beecb56</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.</returns>
        /// <returns status="403">Occurs when the workspace is not owned by the logged in user.</returns>
        /// <returns status="404">Occurs when the user or workspace record could not be found.</returns>
        [Authorize]
        [HttpDelete("{id}/users")]
        public async Task<ActionResult> DeleteWorkspaceUser(
            [FromRoute] string id,
            [FromQuery(Name = "users")] List<string> users
        )
        {
            foreach (string userId in users)
            {
                // Load the workspace and user items
                WorkspaceItem workspaceItem = await LoadWorkspaceItemAsync(userId, id);
                if (workspaceItem is null) return NotFound();
                UserItem userItem = await LoadUserItemAsync(id, userId);
                if (userItem is null) return NotFound();

                // Delete the workspace for the given user if the workspace is owned by the logged in user
                UserInformation currentUser = new UserInformation(User);
                if (workspaceItem.DocumentHistory.AddedBy.Id != currentUser.Id)
                {
                    _logger.LogWarning($"Workspace item for user {userId} and workspace {workspaceItem.Id} " +
                        $"not created by logged in user");
                    return Forbid();
                }
                bool deleted = await _noSqlDbContext.DeleteDocumentStringAsync
                (
                    Keys.GetUserKey(userId),
                    Keys.GetWorkspaceKey(id)
                );
                if (!deleted)
                {
                    _logger.LogWarning($"Workspace item for user {userId} and workspace {workspaceItem.Id} " +
                        $"could not be deleted");
                    return NotFound();
                }

                // Delete the user from the workspace
                deleted = await _noSqlDbContext.DeleteDocumentStringAsync
                (
                    Keys.GetWorkspaceKey(id),
                    Keys.GetUserKey(userId)
                );
                if (!deleted)
                {
                    _logger.LogWarning($"User item for user {userId} and workspace {workspaceItem.Id} " +
                        $"could not be found to update delete state");
                    return NotFound();
                }
                else
                {
                    WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
                    (
                        WorkspaceChangeEnumeration.User,
                        userItem.UserInformation.Id,
                        userItem.UserInformation.Name,
                        WorkspaceActionEnumeration.Removed,
                        currentUser.Name
                    );
                    await SaveWorkspaceChangeItemAsync(id, workspaceChangeItem);
                }
            }

            // Return Ok if no errors occurred up to this point
            return Ok();
        }

        /// <summary>
        /// Persist the data set under the given workspace. 
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="source">The data set source.</param>
        /// <param name="resource">The data set resource.</param>
        /// <param name="name">Optional name for the data set.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="source">Synthetic</arg>
        ///     <arg name="resource">Finite</arg>
        /// </request>
        /// <request name="Example with name">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="source">Synthetic</arg>
        ///     <arg name="resource">Finite</arg>
        ///     <arg name="name">MyDatasetName</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful. The data set information,
        /// including an unique identifier, will be attached to the response.</returns>
        [Authorize]
        [HttpPost("{id}/data/{source}/{resource}")]
        public async Task<ActionResult<DatasetItem>> PostWorkspaceData(
            [FromRoute] string id,
            [FromRoute] DataSource source,
            [FromRoute] string resource,
            [FromQuery(Name = "name")] string name
        )
        {
            UserInformation userInformation = new UserInformation(User);
            DatasetItem datasetItem = new DatasetItem(source, resource, userInformation);
            if (name is not null) datasetItem.Name = name;
            string json = JsonSerializer.Serialize<DatasetItem>(datasetItem, JsonOptions);
            string datasetId = await _noSqlDbContext.CreateDocumentStringAsync
            (
                Keys.GetWorkspaceKey(id),
                Keys.GetDatasetKey(""),
                json
            );
            if (datasetId is null) return Conflict();
            else
            {
                // Save history
                WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
                (
                    WorkspaceChangeEnumeration.Dataset,
                    datasetId,
                    $"{source}/{resource}",
                    WorkspaceActionEnumeration.Added,
                    userInformation.Name
                );
                workspaceChangeItem.WorkspaceChangeInformation = new WorkspaceChangeInformation();
                workspaceChangeItem.WorkspaceChangeInformation.DatasetName = name;
                await SaveWorkspaceChangeItemAsync(id, workspaceChangeItem);

                datasetItem.Id = datasetId;
                return Ok(datasetItem);
            }
        }

        /// <summary>
        /// Update the name or workflow of a dataset.
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="datasetId">The unique data set identifier.</param>
        /// <param name="name">Optional name for the data set.</param>
        /// <param name="workflowId">Optional workflow identifier to apply to the data set.</param>
        /// <param name="versionNumber">Optional workflow version number to apply to the data set.</param>
        /// <request name="Example to update name">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="datasetId">02F69ES5FSMMY1PYCG72B31531</arg>
        ///     <arg name="name">MyDataset</arg>
        /// </request>
        /// <request name="Example to update workflow">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="datasetId">02F69ES5FSMMY1PYCG72B31531</arg>
        ///     <arg name="workflow">01F6Q75NC7TEPXWSNSXJC18BDE</arg>
        /// </request>
        /// <request name="Example to update name, workflow and workflow version">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="datasetId">02F69ES5FSMMY1PYCG72B31531</arg>
        ///     <arg name="name">MyDatasetName</arg>
        ///     <arg name="workflowId">01F6Q75NC7TEPXWSNSXJC18BDE</arg>
        ///     <arg name="versionNumber">3</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful. The
        /// data set information will be attached to the response.</returns>
        /// <returns status="400">Occurs when a version number is specified but a workflow identifier is not</returns>
        /// <returns status="404">Occurs when the data set or workflow could not be found.</returns>
        [Authorize]
        [HttpPatch("{id}/data/{datasetId}")]
        public async Task<ActionResult<DatasetItem>> PatchWorkspaceData(
            [FromRoute] string id,
            [FromRoute] string datasetId,
            [FromQuery(Name = "name")] string name,
            [FromQuery(Name = "workflow")] string workflowId,
            [FromQuery(Name = "workflowVersion")] int? versionNumber
        )
        {
            if (versionNumber.HasValue & (workflowId is null)) return BadRequest();

            UserInformation userInformation = new UserInformation(User);
            DatasetItem datasetItem = await LoadWorkspaceDatasetAsync(id, datasetId);
            if (datasetItem is null) return NotFound();
            WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
            (
                WorkspaceChangeEnumeration.Dataset,
                datasetId,
                $"{datasetItem.Source}/{datasetItem.Resource}",
                WorkspaceActionEnumeration.Updated,
                userInformation.Name
            );
            workspaceChangeItem.WorkspaceChangeInformation = new WorkspaceChangeInformation();
            if (name is not null)
            {
                datasetItem.Name = name;
                workspaceChangeItem.WorkspaceChangeInformation.DatasetName = name;
            }
            if (workflowId is not null)
            {
                WorkflowAccessItem workflowAccessItem = await LoadWorkflowAccessAsync(id, workflowId);
                if (workflowAccessItem is null) return NotFound();
                datasetItem.WorkflowId = workflowId;
                workspaceChangeItem.WorkspaceChangeInformation.WorkflowId = workflowId;
                workspaceChangeItem.WorkspaceChangeInformation.WorkflowName = workflowAccessItem.Name;
                if (versionNumber.HasValue)
                {
                    datasetItem.VersionNumber = versionNumber.Value;
                    workspaceChangeItem.WorkspaceChangeInformation.WorkflowVersion = versionNumber.Value;
                }           
            }  
            string json = JsonSerializer.Serialize<DatasetItem>(datasetItem, JsonOptions);
            bool updated = await _noSqlDbContext.UpdateDocumentStringAsync
            (
                Keys.GetWorkspaceKey(id),
                Keys.GetDatasetKey(datasetId),
                json
            );
            if (!updated) return NotFound();
            else
            {
                // Save history
                await SaveWorkspaceChangeItemAsync(id, workspaceChangeItem);
                return Ok(datasetItem);
            }     
        }

        /// <summary>
        /// Retrieves all the data sets under the given workspace. 
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <request name="Example">
        ///     <arg name="workspaceId">01F68ES7FSMMY1PYCG72B31759</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.
        /// A list of data sets will be attached to the returned object.</returns>
        [Authorize]
        [HttpGet("{id}/data")]
        public async Task<ActionResult<List<DatasetItem>>> GetWorkspaceDatasets(
            [FromRoute] string id
        )
        {
            string partitionKey = Keys.GetWorkspaceKey(id);
            string sortKeyPrefix = Keys.GetDatasetKey("");
            List<string> jsonStrings = await _noSqlDbContext.LoadDocumentStringsAsync(partitionKey, sortKeyPrefix);
            List<DatasetItem> datasetItems = new() { };
            foreach (string jsonString in jsonStrings)
            {
                datasetItems.Add(JsonSerializer.Deserialize<DatasetItem>(jsonString, JsonOptions));
            }
            return Ok(datasetItems);
        }

        /// <summary>
        /// Retrieves data set information for the specified workspace and data set.
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="datasetId">The data set identifier.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="datasetId">02F69ES5FSMMY1PYCG72B31531</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.
        /// The data set information will be attached to the returned object.</returns>
        /// <returns status="404">Occurs when the data set could not be found.</returns>
        [Authorize]
        [HttpGet("{id}/data/{datasetId}")]
        public async Task<ActionResult<DatasetItem>> GetWorkspaceDataset(
            [FromRoute] string id,
            [FromRoute] string datasetId
        )
        {
            DatasetItem datasetItem = await LoadWorkspaceDatasetAsync(id, datasetId);
            if (datasetItem is null) return NotFound();
            else return Ok(datasetItem);
        }

        /// <summary>
        /// Delete the data set from the given workspace. 
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="datasetId">The data set identifier.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="datasetId">02F69ES5FSMMY1PYCG72B31531</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.</returns>
        /// <returns status="404">Occurs when the data set could not be found.</returns>
        [Authorize]
        [HttpDelete("{id}/data/{datasetId}")]
        public async Task<ActionResult> DeleteWorkspaceData(
            [FromRoute] string id,
            [FromRoute] string datasetId
        )
        {
            DatasetItem datasetItem = await LoadWorkspaceDatasetAsync(id, datasetId);
            if (datasetItem is null) return NotFound();
            bool deleted = await _noSqlDbContext.DeleteDocumentStringAsync
            (
                Keys.GetWorkspaceKey(id),
                Keys.GetDatasetKey(datasetId)
            );
            if (!deleted) return NotFound();
            else
            {
                // Save history
                WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
                (
                    WorkspaceChangeEnumeration.Dataset,
                    datasetId,
                    $"{datasetItem.Source}/{datasetItem.Resource}",
                    WorkspaceActionEnumeration.Removed,
                    new UserInformation(User).Name
                );
                workspaceChangeItem.WorkspaceChangeInformation = new WorkspaceChangeInformation();
                workspaceChangeItem.WorkspaceChangeInformation.DatasetName = datasetItem.Name;
                await SaveWorkspaceChangeItemAsync(id, workspaceChangeItem);
                return Ok();
            }      
        }

        /// <summary>
        /// Persists a workflow under the given workspace
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <param name="versionNumber">Optional workflow version number.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="workflowId">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <request name="Example with version number">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="workflowId">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="versionNumber">3</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful. The workflow information
        /// will be attached to the response object.</returns>
        /// <returns status="404">Occurs when a workflow with the specified identifier and version
        /// could not be found.</returns>
        [Authorize]
        [HttpPost("{id}/workflows/{workflowId}")]
        public async Task<ActionResult<WorkflowAccessItem>> PostWorkspaceWorkflow(
            [FromRoute] string id,
            [FromRoute] string workflowId,
            [FromQuery(Name = "workflowVersion")] int? versionNumber 
        )
        {
            // Set the version number
            int nr;
            if (versionNumber.HasValue) nr = versionNumber.Value;
            else nr = await WorkflowController.GetCurrentWorkflowVersionNumber
                (
                    new UserInformation(User).Id,
                    workflowId,
                    _noSqlDbContext
                );
                
            // Load workflow information
            WorkflowItem workflowItem = await WorkflowController.LoadWorkflowAsync(workflowId, nr, _noSqlDbContext);
            if (workflowItem is null) return NotFound();

            // Persist access information
            WorkflowAccessItem workflowAccessItem =  await UpdateWorkflowAccessAsync
            (
                id,
                workflowId,
                workflowItem.Workflow.Name,
                workflowItem.VersionInformation
            );

            // Save history
            UserInformation userInformation = new UserInformation(User);
            WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
            (
                WorkspaceChangeEnumeration.Workflow,
                workflowId,
                workflowItem.Workflow.Name,
                WorkspaceActionEnumeration.Added,
                userInformation.Name
            );
            workspaceChangeItem.WorkspaceChangeInformation = new WorkspaceChangeInformation();
            workspaceChangeItem.WorkspaceChangeInformation.WorkflowVersion = workflowItem.VersionInformation.Number;
            await SaveWorkspaceChangeItemAsync(id, workspaceChangeItem);

            // Return access information
            return Ok(workflowAccessItem);
        }

        /// <summary>
        /// Retrieves information on workflows that are accessible under a workspace.
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="archived">A flag indicating whether archived workspaces should be returned.
        /// Set to true if archived workspaces should be returned, otherwise set to false.
        /// Defaults to false.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        /// </request>
        /// <request name="Example to retrieve archived workflows">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="archived">true</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful. The workflow information
        /// will be attached to the response object.</returns>
        [Authorize]
        [HttpGet("{id}/workflows")]
        public async Task<ActionResult<List<WorkflowAccessItem>>> GetWorkspaceWorkflows(
            [FromRoute] string id,
            [FromQuery(Name = "archived")] bool? archived
        )
        {
            // If no archived query parameter has been passed, set it to false
            bool isArchived = false;
            if (archived.HasValue) isArchived = archived.Value;

            // Retrieve the workflow access items
            List<WorkflowAccessItem> workflowAccessItems = new() { };
            string partitionKey = Keys.GetWorkspaceKey(id);
            string sortKeyPrefix = Keys.GetWorkflowAccessKey("");
            List<string> jsonStrings = await _noSqlDbContext.LoadDocumentStringsAsync(partitionKey, sortKeyPrefix);
            foreach (string jsonString in jsonStrings)
            {
                WorkflowAccessItem workflowAccessItem =
                    JsonSerializer.Deserialize<WorkflowAccessItem>(jsonString, JsonOptions);
                if (workflowAccessItem.Archived == isArchived) workflowAccessItems.Add(workflowAccessItem);
            }

            // Return the list of items
            return Ok(workflowAccessItems);
        }

        /// <summary>
        /// Retrieves information on the specified workflow.
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="workflowId">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful. The workflow information
        /// will be attached to the response object.</returns>
        /// <returns status="404">Occurs when no workflow information could be found.</returns>
        [Authorize]
        [HttpGet("{id}/workflows/{workflowId}")]
        public async Task<ActionResult<WorkflowAccessItem>> GetWorkspaceWorkflow(
            [FromRoute] string id,
            [FromRoute] string workflowId
        )
        {
            WorkflowAccessItem workflowAccessItem = await LoadWorkflowAccessAsync(id, workflowId);
            if (workflowAccessItem is null) return NotFound();
            else return Ok(workflowAccessItem);
        }

        /// <summary>
        /// Archives/unarchives a workflow from the workspace, or updates the version of the workflow available
        /// under the workspace
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <param name="archived">Flag indicating whether the workflow should be archived (true)
        /// or not (false).</param>
        /// <param name="versionNumber">Optional workflow version number that should be accessible under the
        /// workspace.</param>
        /// <request name="Example to archive a workflow">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="workflowId">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="archived">true</arg>
        /// </request>
        /// <request name="Example to update version number">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="workflowId">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="versionNumber">3</arg>
        /// </request>
        /// <request name="Example to unarchive a workflow and set it to a specific version">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="workflowId">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        ///     <arg name="archived">false</arg>
        ///     <arg name="versionNumber">3</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful. The workflow access information
        /// will be attached to the response object.</returns>
        /// <returns status="404">Occurs when a workflow with the specified identifier 
        /// could not be found.</returns>
        [Authorize]
        [HttpPatch("{id}/workflows/{workflowId}")]
        public async Task<ActionResult<WorkflowAccessItem>> PatchWorkspaceWorkflow(
            [FromRoute] string id,
            [FromRoute] string workflowId,
            [FromQuery(Name = "archived")] bool? archived,
            [FromQuery(Name = "workflowVersion")] int? versionNumber
        )
        {
            // Load the workflow
            WorkflowAccessItem workflowAccessItem = await LoadWorkflowAccessAsync(id, workflowId);
            if (workflowAccessItem is null)
            {
                _logger.LogWarning($"Workflow access item for workspace id {id} and workflow {workflowId} could not be " +
                    $"found for patching");
                return NotFound();
            }

            // Update archive information
            WorkspaceChangeItem workspaceChangeItem = null;
            if (archived.HasValue)
            {
                workflowAccessItem.Archived = archived.Value;
                if (archived.Value)
                {
                    workflowAccessItem.DocumentHistory.ArchivedBy = new UserInformation(User);
                    workflowAccessItem.DocumentHistory.DateArchived = DateTime.Now;
                    workspaceChangeItem = new WorkspaceChangeItem
                    (
                        WorkspaceChangeEnumeration.Workflow,
                        workflowId,
                        workflowAccessItem.Name,
                        WorkspaceActionEnumeration.Removed,
                        workflowAccessItem.DocumentHistory.ArchivedBy.Name
                    );
                }
                else
                {
                    workflowAccessItem.DocumentHistory.UnarchivedBy = new UserInformation(User);
                    workflowAccessItem.DocumentHistory.DateUnarchived = DateTime.Now;
                    workspaceChangeItem = new WorkspaceChangeItem
                    (
                        WorkspaceChangeEnumeration.Workflow,
                        workflowId,
                        workflowAccessItem.Name,
                        WorkspaceActionEnumeration.Added,
                        workflowAccessItem.DocumentHistory.UnarchivedBy.Name
                    );
                }
            }

            // Update version information
            if (versionNumber.HasValue)
            {
                WorkflowItem workflowItem = await WorkflowController.LoadWorkflowAsync
                (
                    workflowId,
                    versionNumber.Value,
                    _noSqlDbContext
                );
                if (workflowItem is null)
                {
                    _logger.LogWarning($"Workflow item for workspace id {id} and workflow {workflowId} could not be " +
                        $"found for patching");
                    return NotFound();
                }
                workflowAccessItem.VersionInformation = workflowItem.VersionInformation;
                UserInformation userInformation = new UserInformation(User);
                workspaceChangeItem = new WorkspaceChangeItem
                (
                    WorkspaceChangeEnumeration.Workflow,
                    workflowId,
                    workflowItem.Workflow.Name,
                    WorkspaceActionEnumeration.Updated,
                    userInformation.Name
                );
                workspaceChangeItem.WorkspaceChangeInformation = new WorkspaceChangeInformation();
                workspaceChangeItem.WorkspaceChangeInformation.WorkflowVersion = workflowItem.VersionInformation.Number;
            }

            // Persist access information
            await UpdateWorkflowAccessAsync(id, workflowId, workflowAccessItem);

            // Save history
            await SaveWorkspaceChangeItemAsync(id, workspaceChangeItem);

            // Return access information
            return Ok(workflowAccessItem);
        }

        /// <summary>
        /// Deletes the specified workflow
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <request name="Example">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="workflowId">01F7S2CBC9WHE5YMBHX8FB2FAM</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.</returns>
        /// <returns status="403">Occurs when the workflow was not originally added by the logged in user.</returns>
        /// <returns status="404">Occurs when no workflow information could be found.</returns>
        [Authorize]
        [HttpDelete("{id}/workflows/{workflowId}")]
        public async Task<ActionResult<WorkflowAccessItem>> DeleteWorkspaceWorkflow(
            [FromRoute] string id,
            [FromRoute] string workflowId
        )
        {
            WorkflowAccessItem workflowAccessItem = await LoadWorkflowAccessAsync(id, workflowId);
            UserInformation currentUser = new UserInformation(User);
            if (workflowAccessItem is null) return NotFound();
            if (workflowAccessItem.DocumentHistory.AddedBy.Id != currentUser.Id) return Forbid();
            string partitionKey = Keys.GetWorkspaceKey(id);
            string sortKey = Keys.GetWorkflowAccessKey(workflowId);
            bool deleted = await _noSqlDbContext.DeleteDocumentStringAsync(partitionKey, sortKey);
            if (!deleted) return NotFound();
            else
            {
                // Save history
                WorkspaceChangeItem workspaceChangeItem = new WorkspaceChangeItem
                (
                    WorkspaceChangeEnumeration.Workflow,
                    workflowId,
                    workflowAccessItem.Name,
                    WorkspaceActionEnumeration.Removed,
                    currentUser.Name
                );
                await SaveWorkspaceChangeItemAsync(id, workspaceChangeItem);

                return Ok();
            }
        }

        /// <summary>
        /// Retrieve a summary of changes made to a workspace.
        /// </summary>
        /// <param name="id">A unique workspace identifier.</param>
        /// <param name="type">The type of workspace change to return - Workspace, User, Workflow or Dataset.
        /// If not specified, changes of all types will be returned.</param>
        /// <param name="dateFrom">An ISO-8601 formatted string specifying from which date onwards changes should be
        /// returned. If not specified, no from date filter will be applied.</param>
        /// <param name="dateTo">An ISO-8601 formatted string specifying the date prior to whichchanges should be
        /// returned. If not specified, no to date filter will be applied.</param>
        /// <request name="Example all changes">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        /// </request>
        /// <request name="Example all workspace changes">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="type">Workspace</arg>
        /// </request>
        /// <request name="Example all user changes">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="type">User</arg>
        /// </request>
        /// <request name="Example all workflow changes">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="type">Workflow</arg>
        /// </request>
        /// <request name="Example all dataset changes">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="type">Dataset</arg>
        /// </request>
        /// <request name="Example all changes from a specified date onwards">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="dateFrom">2021-01-01</arg>
        /// </request>
        /// <request name="Example all changes prior to a specified date">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="dateTo">2021-01-01</arg>
        /// </request>
        /// <request name="Example all changes between a specified date range">
        ///     <arg name="id">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="dateFrom">2021-01-01</arg>
        ///     <arg name="dateTo">2021-01-31</arg>
        /// </request>
        /// <returns status="200">A list of workspace changes will be attached to the returned object.
        /// The list will be empty if no changes corresponding to the specified criteria exist. 
        /// </returns>
        /// <returns status="400">
        /// Occurs when the specified <code>toDate</code> occurs before the specified <code>fromDate</code>
        /// </returns>
        [Authorize]
        [HttpGet("{id}/changes")]
        public async Task<ActionResult<List<WorkspaceChangeItem>>> GetWorkspaceChanges(
            [FromRoute] string id,
            [FromQuery(Name = "type")] WorkspaceChangeEnumeration? type,
            [FromQuery(Name = "dateFrom")] DateTime? dateFrom,
            [FromQuery(Name = "dateTo")] DateTime? dateTo
        )
        {
            // Check that dateTo does not precede dateFrom
            if (dateFrom.HasValue & dateTo.HasValue)
                if (dateFrom.Value > dateTo.Value)
                    return BadRequest();

            // Retrieve all changes in the workspace of the relevant type
            string partitionKey = Keys.GetWorkspaceKey(id);
            string sortKey = Keys.GetChangeKeyPrefix(); // Defaults to all changes
            if (type.HasValue) sortKey = Keys.GetChangeKeyPrefix(type.Value);
            List<string> jsonStrings = await _noSqlDbContext.LoadDocumentStringsAsync(partitionKey, sortKey);

            // Filter changes by date
            List<WorkspaceChangeItem> workspaceChangeItems = new() { };
            foreach (string jsonString in jsonStrings)
            {
                WorkspaceChangeItem workspaceChangeItem =
                    JsonSerializer.Deserialize<WorkspaceChangeItem>(jsonString, JsonOptions);
                if (IsDateInRange(workspaceChangeItem.WorkspaceAction.DateTime, dateFrom, dateTo))
                    workspaceChangeItems.Add(JsonSerializer.Deserialize<WorkspaceChangeItem>(jsonString, JsonOptions));
            }
        
            // Sort the list by date
            workspaceChangeItems.Sort((p, q) => p.WorkspaceAction.DateTime.CompareTo(q.WorkspaceAction.DateTime));
            workspaceChangeItems.Reverse();

            // Return changes
            return Ok(workspaceChangeItems);
        }

    }
}
