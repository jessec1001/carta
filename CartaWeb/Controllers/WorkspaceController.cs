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
        /// Key for looking up the Cognito user name
        /// </summary>
        const string CognitoUsername = "cognito:username";

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
        /// Persists a new workspace, creating table items for the worskpace under the logged in user, and the user
        /// under the workspace.
        /// </summary>
        /// <param name="userItem">The user information.</param>
        /// <param name="workspaceItem">The workspace information.</param>
        /// <returns>
        /// A unique identifier for the workspace.
        /// </returns>
        protected static async Task<string> CreateWorkspaceAsync(
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
        protected static async Task<WorkspaceItem> LoadWorkspaceItemAsync(string userId, string workspaceId)
        {
            string partitionKey = GetUserKey(userId);
            string sortKey = GetWorkspaceKey(workspaceId);
            string jsonString = await _noSqlDbContext.LoadDocumentStringAsync(partitionKey, sortKey);
            if (jsonString is null) return null;
            else return JsonSerializer.Deserialize<WorkspaceItem>(jsonString, JsonOptions);
        }

        /// <summary>
        /// Returns the information of all the users that have access to the given workspace,
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <returns>
        /// A list of user items.
        /// </returns>
        protected static async Task<List<UserItem>> LoadUserItemsAsync(string workspaceId)
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
        /// Updates the archived flag of a workspace, and deletes the user record for that workspace if the workspace
        /// is to be archived, or adds the user record if the workspace is to be unarchived.
        /// </summary>
        /// <param name="userItem">The user information.</param>
        /// <param name="workspaceItem">The workspace information.</param>
        /// <returns>
        /// True if the update operations completed with no errors.
        /// </returns>
        protected static async Task<bool> UpdateWorkspaceArchiveAsync(UserItem userItem, WorkspaceItem workspaceItem)
        {
            string json = JsonSerializer.Serialize<WorkspaceItem>(workspaceItem, JsonOptions);
            bool updated = await _noSqlDbContext.UpdateDocumentStringAsync
            (
                GetUserKey(userItem.Id),
                GetWorkspaceKey(workspaceItem.Id),
                json
            );
            if (!updated) return false;

            if (workspaceItem.Archived)
            {
                bool deleted = await _noSqlDbContext.DeleteDocumentStringAsync
                (
                    GetWorkspaceKey(workspaceItem.Id),
                    GetUserKey(userItem.Id)
                );
                if (!deleted) return false;
            } else
            {
                userItem.DateAdded = DateTime.Now;
                json = JsonSerializer.Serialize<UserItem>(userItem, JsonOptions);
                await _noSqlDbContext.SaveDocumentStringAsync
                (
                    GetWorkspaceKey(workspaceItem.Id),
                    GetUserKey(userItem.Id),
                    json
                );
            }
            return true;
        }

        /// <summary>
        /// Adds a user to a workspace, adding table items for the workspace under the user, and the user
        /// under the workspace.
        /// </summary>
        /// <param name="userItem">The user information.</param>
        /// <param name="workspaceItem">The workspace information.</param>
        protected static async Task SaveWorkspaceAsync(UserItem userItem, WorkspaceItem workspaceItem)
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
        /// <param name="workspaceItem">The workspace item.</param>
        /// <request name="Example">
        ///     <body>
        ///     {
        ///         "name":"MyWorkspaceName",
        ///         "dateCreated":"2021-05-17T18:25:38.548854-06:00",
        ///         "createdBy":"userX"
        ///     }
        ///     </body>
        /// </request>
        /// <returns status="200">A unique identifier generated for the workspace will be attached to the
        /// response.</returns>
        /// <returns status="409">
        /// Occurs when the create operation fails unexpectedly due to a key conflict.
        /// </returns>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<string>> PostWorkspace(
            [FromBody] WorkspaceItem workspaceItem
        )
        {
            UserItem userItem = new UserItem(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                User.FindFirstValue(CognitoUsername),
                User.FindFirstValue(ClaimTypes.Email));
            string id = await CreateWorkspaceAsync(userItem, workspaceItem);
            if (workspaceItem is null) return Conflict();
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
        /// <response name="Example">
        ///     <body>
        ///     {
        ///         "name":"MyWorkspaceName",
        ///         "dateCreated":"2021-05-17T18:25:38.548854-06:00",
        ///         "createdBy":"userX"
        ///     }
        ///     </body>
        /// </response>
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
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
        /// <response name="Example">
        ///     <body>
        ///     [
        ///           {
        ///                "id": "01F65EBPPYH3RJFS38028XTZY0",
        ///                "name": "workspaceA",
        ///                "dateCreated": "2021-05-17T18:25:38.548854-06:00",
        ///                "createdBy": "me",
        ///                "archived": false
        ///            },
        ///            {
        ///                "id": "01F65EC3C449NXK0VX2BKZVB0F",
        ///                "name": "workspaceB",
        ///                "dateCreated": "2021-05-17T18:25:38.548854-06:00",
        ///                "createdBy": "me",
        ///                "archived": false
        ///            }
        ///        ]
        ///     </body>
        /// </response>
        /// <returns status="200">
        /// A list of workspace items will be attached to the returned object.
        /// </returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<WorkspaceItem>>> GetWorkspaces(
            [FromQuery(Name = "archived")] bool? archived
        )
        {
            // If no archived query parameter has been passed, set it to false
            if (!archived.HasValue) archived = false;

            // Load the items
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Filter items according to archived flag
            List<WorkspaceItem> allWorkspaceItems = await LoadWorkspaceItemsAsync(userId);
            List<WorkspaceItem> workspaceItems = new List<WorkspaceItem>();
            foreach (WorkspaceItem item in allWorkspaceItems)
            {
               if (item.Archived == archived.Value) workspaceItems.Add(item);
            }

            return Ok(workspaceItems);
        }

        /// <summary>
        /// Update the archived status of a workspace.
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="archived">Flag indicating whether the workspace should be archived (true)
        /// or not (false).</param>
        /// <request name="Example"></request>
        /// <returns status="200">Occurs when the operation is successful.</returns>
        /// <returns status="404">
        /// Occurs when a workspace for the given identifier cannot be found. 
        /// </returns>
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<ActionResult> PatchWorkspace(
            [FromRoute] string id,
            [FromQuery(Name = "archived")] bool archived
        )
        {
            UserItem userItem = new UserItem(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                User.FindFirstValue(CognitoUsername),
                User.FindFirstValue(ClaimTypes.Email));
            WorkspaceItem workspaceItem = await LoadWorkspaceItemAsync(userItem.Id, id);
            workspaceItem.Archived = archived;
            bool updated = await UpdateWorkspaceArchiveAsync(userItem, workspaceItem);
            if (updated) return Ok();
            else return NotFound();
        }

        /// <summary>
        /// Add users to the workspace. 
        /// </summary>
        /// <param name="id">The workspace identifier.</param>
        /// <param name="userItems">A list of users that should be added to the workspace.</param>
        /// <request name="Example">
        ///     <body>
        ///         [
        ///            {   
        ///                "id":"userId1",
        ///                "name":"user1",
        ///                "email":"email1@domain.com",
        ///                "group":"UsersGroup"
        ///            },
        ///            {   
        ///                "id":"userId2",
        ///                "name":"user2",
        ///                "email":"email2@domain.com",
        ///                "group":"UsersGroup"
        ///            }
        ///         ]
        ///     </body>
        /// </request>
        /// <returns status="200">Occurs when the operation is successful.</returns>
        /// <returns status="404">
        /// Occurs when a workspace for the given identifier cannot be found. 
        /// </returns>
        [Authorize]
        [HttpPatch("{id}/users")]
        public async Task<ActionResult> PatchWorkspaceUsers(
            [FromRoute] string id,
            [FromBody] List<UserItem> userItems
        )
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            WorkspaceItem workspaceItem = await LoadWorkspaceItemAsync(userId, id);
            if (workspaceItem is null) return NotFound();

            foreach (UserItem userItem in userItems)
            {
               await SaveWorkspaceAsync(userItem, workspaceItem);
            }

            return Ok();
        }

        /// <summary>
        /// Get a list of all the users that have access to a workspace
        /// </summary>
        /// <param name="id">The workspace identifier</param>
        /// <response name="Example">
        ///     <body>
        ///         [
        ///            {   
        ///                "id":"userId1",
        ///                "name":"user1",
        ///                "email":"email1@domain.com",
        ///                "group":"UsersGroup"
        ///            },
        ///            {   
        ///                "id":"userId2",
        ///                "name":"user2",
        ///                "email":"email2@domain.com",
        ///                "group":"UsersGroup"
        ///            }
        ///         ]
        ///     </body>
        /// </response>
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
            if ((userItems is null) | (userItems.Count == 0) ) return NotFound();
            else return Ok(userItems);
        }

    }
}
