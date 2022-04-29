using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Static constructor for initializing JSON serialization/deserialization options
        /// </summary>
        static WorkspaceController()
        {
            JsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            JsonOptions.PropertyNameCaseInsensitive = false;
        }

        /// <summary>
        /// The logger for this controller.
        /// </summary>
        private readonly ILogger<WorkspaceController> _logger;

        /// <summary>
        /// The persistence object used to read and write from the database
        /// </summary>
        private readonly Persistence _persistence;

        /// <inheritdoc />
        public WorkspaceController(ILogger<WorkspaceController> logger, INoSqlDbContext noSqlDbContext)
        {
            _logger = logger;
            _persistence = new Persistence(noSqlDbContext);
        }

        /// <summary>
        /// Returns the workspace information for the given workspace identifier and user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <returns>
        /// A workspace item.
        /// </returns>
        protected async Task<WorkspaceItem> LoadWorkspaceAsync(string userId, string workspaceId)
        {

            WorkspaceItem workspaceItem = new(userId, workspaceId);
            Item item = await _persistence.LoadItemAsync(workspaceItem);
            return (WorkspaceItem)item;
        }

        /// <summary>
        /// Returns user information for the given workspace and user.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        /// The user item.
        /// </returns>
        protected async Task<UserItem> LoadUserAsync(string workspaceId, string userId)
        {
            UserItem userItem = new(workspaceId, userId);
            Item item = await _persistence.LoadItemAsync(userItem);
            return (UserItem)item;
        }

        /// <summary>
        /// Retrieves access information on the specified operation.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="operationId">The operation identifier.</param>
        /// <returns>Access information for the operation inside the workspace.</returns>
        protected async Task<OperationAccessItem> LoadOperationAccessAsync(string workspaceId, string operationId)
        {
            OperationAccessItem operationAccessItem = new(workspaceId, operationId);
            Item item = await _persistence.LoadItemAsync(operationAccessItem);
            return (OperationAccessItem)item;
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
            // TODO: (Permissions) This endpoint should be available to all users.

            // Create workspace item 
            UserInformation userInformation = new(User);
            WorkspaceItem workspaceItem = new(userInformation.Id, name, userInformation);
            DbDocument workspaceItemDbDocument = workspaceItem.CreateDbDocument();

            // Create user item 
            UserItem userItem = new(workspaceItem.Id, userInformation);
            userItem.DocumentHistory.AddedBy = userInformation;

            // Create workspace change item 
            WorkspaceChangeItem workspaceChangeItem = new(
                workspaceItem.Id,
                userItem.UserInformation.Name,
                WorkspaceActionEnumeration.Added,
                workspaceItem
            );

            // Persist items
            bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
            {
                workspaceItemDbDocument,
                userItem.SaveDbDocument(),
                workspaceChangeItem.CreateDbDocument()
            });
            if (isSaved)
            {
                return Ok(workspaceItem);
            }
            else
            {
                _logger.LogWarning($"Workspace {name} created by user {userInformation.Name} could not be saved");
                return Conflict();
            }
        }

        /// <summary>
        /// Deletes a workspace for the given user.
        /// </summary>
        /// <param name="id">The unique identifier of the workspace.</param>
        /// <returns status="200">Nothing.</returns>
        /// <returns status="404">
        /// Occurs when the workspace item for the given ID cannot be found.
        /// </returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWorkspace(
            [FromRoute] string id
        )
        {
            UserInformation userInformation = new(User);
            WorkspaceItem workspaceItem = await LoadWorkspaceAsync(userInformation.Id, id);
            if (workspaceItem is null) return NotFound();
            workspaceItem.SetPartitionKeyId(userInformation.Id);
            DbDocument deleteDocument = workspaceItem.DeleteDbDocument();
            bool isDeleted = await _persistence.WriteDbDocumentAsync(deleteDocument);
            if (!isDeleted) return Conflict();
            else return NoContent();
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
            // TODO: (Permissions) This endpoint should only be accessible to someone who has any permission connection
            //       to the workspace.

            WorkspaceItem workspaceItem = await LoadWorkspaceAsync(new UserInformation(User).Id, id);
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
            // TODO: (Permissions) This endpoint should be available to all users. However, the endpoint should be
            //       updated to retrieve workspaces based on the permissions of the user.

            // If no archived query parameter has been passed, set it to false
            if (!archived.HasValue) archived = false;

            // Load the items and filter items according to archived flag
            WorkspaceItem workspaceItem = new(new UserInformation(User).Id);
            IEnumerable<Item> allWorkspaceItems = await _persistence.LoadItemsAsync(workspaceItem);
            if (allWorkspaceItems is null) return NotFound();
            else
            {
                List<WorkspaceItem> workspaceItems = new();
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
            // TODO: (Permissions) This endpoint should only be accessible to someone who has the ability to read users
            //       of a group. Ideally, this should be based on visibility flags of users (see annotations in the user
            //       controller) where anyone with "admin" permissions over the workspace should have fixed visibility.
            //       For now, we can either use the "admin" flag or the "read" flag.

            UserItem userItem = new(id);
            IEnumerable<Item> readUserItems = await _persistence.LoadItemsAsync(userItem);
            List<UserItem> userItems = new() { };
            if (readUserItems is null) return NotFound();
            else
            {
                foreach (UserItem item in readUserItems) userItems.Add(item);
                return Ok(userItems);
            }
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
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to persist the workspace change.
        /// </returns>
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<ActionResult<WorkspaceItem>> PatchWorkspace(
            [FromRoute] string id,
            [FromQuery(Name = "archived")] bool archived
        )
        {
            // TODO: (Permissions) This endpoint should only be accessible to someone who has "admin" permissions over
            //       the referenced workspace.

            UserInformation userInformation = new(User);
            WorkspaceItem workspaceItem = await LoadWorkspaceAsync(userInformation.Id, id);
            if (workspaceItem is null) return NotFound();
            UserItem userItem = await LoadUserAsync(id, userInformation.Id);
            if (userItem is null) return NotFound();
            workspaceItem.Archived = archived;
            WorkspaceChangeItem workspaceChangeItem;

            if (archived)
            {
                userItem.DocumentHistory.DateDeleted = DateTime.Now;
                userItem.DocumentHistory.DeletedBy = userInformation;
                workspaceItem.DocumentHistory.DateArchived = DateTime.Now;
                workspaceChangeItem = new WorkspaceChangeItem
                (
                    id,
                    userItem.UserInformation.Name,
                    WorkspaceActionEnumeration.Removed,
                    userItem
                );
            }
            else
            {
                userItem.DocumentHistory.DateAdded = DateTime.Now;
                userItem.DocumentHistory.AddedBy = userInformation;
                workspaceItem.DocumentHistory.DateUnarchived = DateTime.Now;
                workspaceChangeItem = new WorkspaceChangeItem
                (
                    id,
                    userItem.UserInformation.Name,
                    WorkspaceActionEnumeration.Added,
                    userItem
                );
            }

            workspaceItem.SetPartitionKeyId(userInformation.Id);
            userItem.SetPartitionKeyId(id);
            bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
            {
                workspaceItem.UpdateDbDocument(),
                userItem.UpdateDbDocument(),
                workspaceChangeItem.CreateDbDocument()
            });
            if (isSaved)
            {
                return Ok(workspaceItem);
            }
            else
            {
                _logger.LogWarning($"Workspace {workspaceItem.Name} could not be archived/unarchived");
                return Conflict();
            }
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
        public async Task<ActionResult<List<UserItem>>> AddWorkspaceUsers(
            [FromRoute] string id,
            [FromBody] List<UserItem> userItems
        )
        {
            // TODO: (Permissions) This endpoint needs to be merged into a new permissions endpoint of the form:
            //        `PUT /workspace/{workspaceId}/permissions/user/{userId}`.

            UserInformation userInformation = new(User);
            WorkspaceItem workspaceItem = await LoadWorkspaceAsync(userInformation.Id, id);
            if (workspaceItem is null) return NotFound();

            List<UserItem> writeUserItems = new() { };
            foreach (UserItem userItem in userItems)
            {
                workspaceItem.SetPartitionKeyId(userItem.UserInformation.Id);

                UserItem writeUserItem = new(id, userItem.UserInformation);
                writeUserItem.DocumentHistory.DateAdded = DateTime.Now;
                writeUserItem.DocumentHistory.AddedBy = userInformation;
                writeUserItems.Add(writeUserItem);

                WorkspaceChangeItem workspaceChangeItem = new(
                    id,
                    userInformation.Name,
                    WorkspaceActionEnumeration.Added,
                    writeUserItem
                );

                bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
                {
                    workspaceItem.SaveDbDocument(),
                    writeUserItem.SaveDbDocument(),
                    workspaceChangeItem.CreateDbDocument()
                });
                if (!isSaved)
                {
                    _logger.LogWarning($"User {userInformation.Name} could not be added to workspace " +
                        $"{workspaceItem.Name}");
                    return Conflict();
                }
            }

            return Ok(writeUserItems);
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
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to persist the workflow. 
        /// </returns>
        [Authorize]
        [HttpDelete("{id}/users")]
        public async Task<ActionResult> RemoveWorkspaceUser(
            [FromRoute] string id,
            [FromQuery(Name = "users")] List<string> users
        )
        {
            // TODO: (Permissions) This endpoint needs to be merged into a new permissions endpoint of the form:
            //        `PUT /workspace/{workspaceId}/permissions/user/{userId}`.

            foreach (string userId in users)
            {
                // Load the workspace and user items
                WorkspaceItem workspaceItem = await LoadWorkspaceAsync(userId, id);
                if (workspaceItem is null) return NotFound();
                UserItem userItem = await LoadUserAsync(id, userId);
                if (userItem is null) return NotFound();

                // Delete the workspace for the given user if the workspace is owned by the logged in user
                UserInformation currentUser = new(User);
                if (workspaceItem.DocumentHistory.AddedBy.Id != currentUser.Id)
                {
                    _logger.LogWarning($"Workspace item for user {userId} and workspace {workspaceItem.Id} " +
                        $"not created by logged in user");
                    return Forbid();
                }

                // Create structure to record the change
                WorkspaceChangeItem workspaceChangeItem = new(
                    id,
                    currentUser.Name,
                    WorkspaceActionEnumeration.Removed,
                    userItem
                );

                // Perform the database operations
                workspaceItem.SetPartitionKeyId(userId);
                userItem.SetPartitionKeyId(id);
                bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
                {
                    workspaceItem.DeleteDbDocument(),
                    userItem.DeleteDbDocument(),
                    workspaceChangeItem.CreateDbDocument()
                });
                if (!isSaved)
                {
                    _logger.LogWarning($"User {userItem.UserInformation.Name} could not be deleted from workspace " +
                        $"{workspaceItem.Name}");
                    return Conflict();
                }
            }
            // Return Ok if no errors occurred up to this point
            return Ok();
        }

        /// <summary>
        /// Retrieves information on operations that are accessible by workspace members.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <returns status="200">
        /// Occurs when the operation is successful. The workflow information will be attached to the response object.
        /// </returns>
        [Authorize]
        [HttpGet("{workspaceId}/operations")]
        public async Task<ActionResult<List<OperationAccessItem>>> GetWorkspaceOperations(
            [FromRoute] string workspaceId
        )
        {
            // TODO: (Permissions) This endpoint needs to be modified to return any operations that have permissions
            //       connections to them for the given workspace. We should not discern based on the permissions of
            //       individual operations at this point and leave that to the operations controller. Thus, this
            //       endpoint should always return something equavalent to a list of identifiers and no more.      

            // Retrieve the operation access items.
            List<OperationAccessItem> operationAccessItems = new();
            OperationAccessItem readOperationAccessItem = new(workspaceId, null);
            IEnumerable<Item> readOperationAccessItems = await _persistence.LoadItemsAsync(readOperationAccessItem);
            foreach (OperationAccessItem operationAccessItem in readOperationAccessItems)
            {
                operationAccessItems.Add(operationAccessItem);
            }

            // Return the list of items.
            return Ok(operationAccessItems);
        }

        #region Operations CRUD
        /// <summary>
        /// Retrieves information on the specified operation that is accessible by workspace members..
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="operationId">The operation identifier.</param>
        /// <returns status="200">
        /// Occurs when the operation is successful. The operation information will be attached to the response object.
        /// </returns>
        /// <returns status="404">
        /// Occurs when no operation information could be found.
        /// </returns>
        [Authorize]
        [HttpGet("{workspaceId}/operations/{operationId}")]
        public async Task<ActionResult<OperationAccessItem>> GetWorkspaceOperation(
            [FromRoute] string workspaceId,
            [FromRoute] string operationId
        )
        {
            // TODO: (Permissions) This endpoint is practically useless since it should only return an identifier (see
            //       annotation in previous endpoint). We should remove this endpoint.

            OperationAccessItem operationAccessItem = await LoadOperationAccessAsync(workspaceId, operationId);
            if (operationAccessItem is null) return NotFound();
            else return Ok(operationAccessItem);
        }

        /// <summary>
        /// Allows members of a workspace to access a specified operation.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="operationId">The operation identifier.</param>
        /// <returns status="200">
        /// Occurs when the operation is successful. The operation information will be attached to the response object.
        /// </returns>
        /// <returns status="404">
        /// Occurs when an operation with the specified identifier could not be found.
        /// </returns>
        /// <returns status="409">
        /// Returned when an unexpected database conflict occured when trying to add the operation. 
        /// </returns>
        [Authorize]
        [HttpPost("{workspaceId}/operations/{operationId}")]
        public async Task<ActionResult<OperationAccessItem>> AddWorkspaceOperation(
            [FromRoute] string workspaceId,
            [FromRoute] string operationId
        )
        {
            // TODO: (Permissions) This endpoint should no longer exist here. We should instead use a new permissions
            //       endpoint of the operations controller to add permissions to operations. This will be of the form:
            //       `PUT /operations/{operationId}/permissions/group/{workspaceId}`.

            // Get user information.
            UserInformation userInformation = new(User);

            // Load operation information.
            OperationItem operationItem = await OperationsController.LoadOperationAsync(operationId, _persistence);
            if (operationItem is null) return NotFound();

            // Persist access information and record change history.
            OperationAccessItem operationAccessItem = new(workspaceId, operationId)
            {
                DocumentHistory = new(userInformation)
            };
            WorkspaceChangeItem workspaceChangeItem = new(
                workspaceId,
                userInformation.Name,
                WorkspaceActionEnumeration.Added,
                operationAccessItem
            )
            { Name = operationItem.Name };

            bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
            {
                operationAccessItem.SaveDbDocument(),
                workspaceChangeItem.CreateDbDocument()
            });
            if (isSaved) return Ok(operationAccessItem);
            else
            {
                _logger.LogWarning($"Workflow {operationItem.Id} could not be saved under workspace ID {workspaceId}");
                return Conflict();
            }
        }

        /// <summary>
        /// Disallows members of a workspace to access a specified operation.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="operationId">The operation identifier.</param>
        /// <returns status="200">
        /// Occurs when the operation is successful.
        /// </returns>
        /// <returns status="403"
        /// >Occurs when the workflow was not originally added by the logged in user.
        /// </returns>
        /// <returns status="404">
        /// Occurs when no workflow information could be found.
        /// </returns>
        [Authorize]
        [HttpDelete("{workspaceId}/operations/{operationId}")]
        public async Task<ActionResult<OperationAccessItem>> RemoveWorkspaceOperation(
            [FromRoute] string workspaceId,
            [FromRoute] string operationId
        )
        {
            // TODO: (Permissions) This endpoint should no longer exist here. We should instead use a new permissions
            //       endpoint of the operations controller to add permissions to operations. This will be of the form:
            //       `PUT /operations/{operationId}/permissions/group/{workspaceId}`.

            OperationAccessItem operationAccessItem = await LoadOperationAccessAsync(workspaceId, operationId);
            UserInformation currentUser = new(User);
            if (operationAccessItem is null) return NotFound();
            if (operationAccessItem.DocumentHistory.AddedBy.Id != currentUser.Id) return Forbid();
            OperationItem operationItem = await OperationsController.LoadOperationAsync(operationId, _persistence);
            WorkspaceChangeItem workspaceChangeItem = new(
                workspaceId,
                currentUser.Name,
                WorkspaceActionEnumeration.Removed,
                operationAccessItem
            )
            { Name = operationItem.Name };
            operationAccessItem = new OperationAccessItem(workspaceId, operationId);
            bool isSaved = await _persistence.WriteDbDocumentsAsync(new List<DbDocument>
            {
                operationAccessItem.DeleteDbDocument(),
                workspaceChangeItem.SaveDbDocument()
            });
            if (isSaved)
            {
                return Ok();
            }
            else
            {
                _logger.LogWarning($"Operation with ID {operationId} could not be deleted from workspace ID {workspaceId}");
                return Conflict();
            }
        }
        #endregion

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
            // TODO: (Permissions) This endpoint should take into consideration the previously noted permissions for
            //       each stored change. For instance, we should emit operation added/removed events for any operations
            //       that the workflow has access to and the current user is in the workspace and we should emit user
            //       added/removed events for which the current user has visibility permissions to.

            // Check that date-to does not precede date-from.
            if (dateFrom.HasValue & dateTo.HasValue)
                if (dateFrom.Value > dateTo.Value)
                    return BadRequest();

            // Retrieve all changes in the workspace of the relevant type
            WorkspaceChangeItem readWorkspaceChangeItem = null;
            if (type.HasValue) readWorkspaceChangeItem = new WorkspaceChangeItem(id, type.Value);
            else readWorkspaceChangeItem = new WorkspaceChangeItem(id);
            IEnumerable<Item> readWorkspaceChangeItems = await _persistence.LoadItemsAsync(readWorkspaceChangeItem);

            // Filter changes by date
            List<WorkspaceChangeItem> workspaceChangeItems = new() { };
            foreach (WorkspaceChangeItem workspaceChangeItem in readWorkspaceChangeItems)
            {
                if (IsDateInRange(workspaceChangeItem.WorkspaceAction.DateTime, dateFrom, dateTo))
                    workspaceChangeItems.Add(workspaceChangeItem);
            }

            // Sort the list by date
            workspaceChangeItems.Sort((q, p) => p.WorkspaceAction.DateTime.CompareTo(q.WorkspaceAction.DateTime));

            // Return changes
            return Ok(workspaceChangeItems);
        }
    }
}
