﻿using System;
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
        private static INoSqlDbContext _noSqlDbContext;

        /// <inheritdoc />
        public WorkspaceController(ILogger<WorkspaceController> logger, INoSqlDbContext noSqlDbContext)
        {
            _logger = logger;
            _noSqlDbContext = noSqlDbContext;

        }

        /// <summary>
        /// Returns a properly formatted user key.
        /// </summary>
        /// <param name="userId">A user identifier.</param>
        /// <returns>
        /// The key for the persisted item for the given user.
        /// </returns>
        protected static string GetUserKey(string userId)
        {
            return "USER#" + userId;
        }

        /// <summary>
        /// Returns a properly formatted workspace key.
        /// </summary>
        /// <param name="workspaceId">A workspace identifier.</param>
        /// <returns>
        /// The key for the persisted item for the given workspace.
        /// </returns>
        protected static string GetWorkspaceKey(string workspaceId)
        {
            return "WORKSPACE#" + workspaceId;
        }

        /// <summary>
        /// Helper method that returns the user identifier of the currently logged in user.
        /// </summary>
        /// <returns>
        /// The user identifier of the currently logged in user.
        /// </returns>
        protected string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Helper method that returns the user name of the currently logged in user.
        /// </summary>
        /// <returns>
        /// The user namer of the currently logged in user.
        /// </returns>
        protected string GetUserName()
        {
            return User.FindFirstValue("cognito:username");
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
                GetUserKey(userItem.Id),
                GetWorkspaceKey(""),
                json
            );

            userItem.DateAdded = DateTime.Now;
            json = JsonSerializer.Serialize<UserItem>(userItem, JsonOptions);
            await _noSqlDbContext.SaveDocumentStringAsync
            (
                GetWorkspaceKey(id),
                GetUserKey(userItem.Id),
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
            string partitionKey = GetUserKey(userId);
            string sortKeyPrefix = GetWorkspaceKey("");
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
            string partitionKey = GetUserKey(userId);
            string sortKey = GetWorkspaceKey(workspaceId);
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
            string partitionKey = GetWorkspaceKey(workspaceId);
            string sortKeyPrefix = GetUserKey("");
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
            string partitionKey = GetWorkspaceKey(workspaceId);
            string sortKey = GetUserKey(userId);
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
                GetUserKey(userItem.Id),
                GetWorkspaceKey(workspaceItem.Id),
                json
            );
            if (!updated)
            {
                _logger.LogWarning("Workspace item for user " + userItem.Id +
                    " and workspace " + workspaceItem.Id + " could not be found for update");
                return false;
            }

            json = JsonSerializer.Serialize<UserItem>(userItem, JsonOptions);
            updated = await _noSqlDbContext.UpdateDocumentStringAsync
            (
                GetWorkspaceKey(workspaceItem.Id),
                GetUserKey(userItem.Id),
                json
            );
            if (!updated) _logger.LogWarning("User item for user " + userItem.Id +
                   " and workspace " + workspaceItem.Id + " could not be found for update");
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
                GetUserKey(userItem.Id),
                GetWorkspaceKey(workspaceItem.Id),
                json
            );

            json = JsonSerializer.Serialize<UserItem>(userItem, JsonOptions);
            await _noSqlDbContext.SaveDocumentStringAsync
            (
                GetWorkspaceKey(workspaceItem.Id),
                GetUserKey(userItem.Id),
                json
            );
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
            string userId = GetUserId();
            string userName = GetUserName();
            UserItem userItem = new UserItem(userId, userName);
            userItem.AddedBy = userId;
            WorkspaceItem workspaceItem = new WorkspaceItem(name, userId);
            string id = await CreateWorkspaceAsync(userItem, workspaceItem);
            if (id is null) return Conflict();
            else
            {
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
            string userId = GetUserId();
            WorkspaceItem workspaceItem = await LoadWorkspaceItemAsync(userId, id);
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
            List<WorkspaceItem> allWorkspaceItems = await LoadWorkspaceItemsAsync(GetUserId());
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
            string userId = GetUserId();
            string userName = GetUserName();
            WorkspaceItem workspaceItem = await LoadWorkspaceItemAsync(userId, id);
            if (workspaceItem is null) return NotFound();
            UserItem userItem = await LoadUserItemAsync(id, userId);
            if (userItem is null) return NotFound();
            workspaceItem.Archived = archived;
            if (archived)
            {
                userItem.DateDeleted = DateTime.Now;
                userItem.DeletedBy = userId;
                workspaceItem.DateArchived = DateTime.Now;
            } else
            {
                userItem.DateAdded = DateTime.Now;
                userItem.AddedBy = userId;
                workspaceItem.DateUnarchived = DateTime.Now;
            }
            bool updated = await UpdateWorkspaceAsync(userItem, workspaceItem);
            if (updated) return Ok(workspaceItem);
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
        ///                "id":"userId1",
        ///                "name":"user1"
        ///            },
        ///            {   
        ///                "id":"userId2",
        ///                "name":"user2"
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
            string userId = GetUserId();
            WorkspaceItem workspaceItem = await LoadWorkspaceItemAsync(userId, id);
            if (workspaceItem is null) return NotFound();
            foreach (UserItem userItem in userItems)
            {
                userItem.DateAdded = DateTime.Now;
                userItem.AddedBy = userId;
                await SaveWorkspaceAsync(userItem, workspaceItem);
            }
            return Ok(userItems);
        }

        /// <summary>
        /// Deletes a user from a workspace. 
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <request name="Example">
        ///     <arg name="workspaceId">01F68ES7FSMMY1PYCG72B31759</arg>
        ///     <arg name="userId">943815bf-5695-4cca-bd3b-46313beecb31</arg>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.</returns>
        /// <returns status="403">Occurs when the workspace is not owned by the logged in user.</returns>
        /// <returns status="404">Occurs when the user or workspace record could not be found.</returns>
        [Authorize]
        [HttpDelete("{workspaceId}/users/{userId}")]
        public async Task<ActionResult> DeleteWorkspaceUser(
            [FromRoute] string workspaceId,
            [FromRoute] string userId
        )
        {
            // Load the workspace and user items
            WorkspaceItem workspaceItem = await LoadWorkspaceItemAsync(userId, workspaceId);
            if (workspaceItem is null) return NotFound();
            UserItem userItem = await LoadUserItemAsync(workspaceId, userId);
            if (userItem is null) return NotFound();
                
            // Delete the workspace for the given user if the workspace is owned by the logged in user
            if (workspaceItem.CreatedBy != GetUserId())
            {
                _logger.LogWarning("Workspace item for user " + userId +
                    " and workspace " + workspaceItem.Id + " not created by logged in user"); ;
                return Forbid();
            }      
            bool deleted = await _noSqlDbContext.DeleteDocumentStringAsync
            (
                GetUserKey(userId),
                GetWorkspaceKey(workspaceId)
            );
            if (!deleted)
            {
                _logger.LogWarning("Workspace item for user " + userId +
                    " and workspace " + workspaceItem.Id + " could not be deleted"); ;
                return NotFound();
            }

            // Update user item information to record the deletion
            userItem.DateDeleted = DateTime.Now;
            userItem.DeletedBy = GetUserId();
            string json = JsonSerializer.Serialize<UserItem>(userItem, JsonOptions);
            bool updated = await _noSqlDbContext.UpdateDocumentStringAsync
            (
                GetWorkspaceKey(workspaceId),
                GetUserKey(userId),
                json
            );
            if (!updated)
            {
                _logger.LogWarning("User item for user " + userId +
                    " and workspace " + workspaceItem.Id + " could not be found to update delete state"); 
                return NotFound();
            }

            // Return Ok if no errors occurred up to this point
            return Ok();
        }

    }
}
