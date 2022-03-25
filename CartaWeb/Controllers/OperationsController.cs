using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using CartaCore.Extensions.Arrays;
using CartaCore.Extensions.Hashing;
using CartaCore.Extensions.String;
using CartaCore.Extensions.Typing;
using CartaCore.Graphs;
using CartaCore.Graphs.Components;
using CartaCore.Operations;
using CartaCore.Operations.Attributes;
using CartaCore.Persistence;
using CartaWeb.Errors;
using CartaWeb.Models.DocumentItem;
using CartaWeb.Models.Options;
using CartaWeb.Serialization;
using CartaWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NJsonSchema;
using NUlid;

namespace CartaWeb.Controllers
{
    // TODO: Make sure that operation defaults are added to the internal representation of operations once created.
    // TODO: Prevent overposting to the default fields of operations.
    // TODO: Add searching for workflows in search endpoint. Make it so that an optional workspace parameter can be
    //       passed to the endpoint that will restrict workflow findings to a relevant workspace.
    // TODO: Take into account the default settings for each operation into a hash of a workflow. This means that
    //       whenever operations inside a workflow are updated, future jobs will have a different hash.

    /// <summary>
    /// A controller that manages creating, updating, retrieving, and deleting operations that can be executed with
    /// inputs to produce a set of outputs as a result.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OperationsController : ControllerBase
    {
        // TODO: This singleton service should probably be interface-ified and should be given a better name.
        private readonly BackgroundJobQueue _jobQueue;
        private readonly ILogger<OperationsController> _logger;
        private readonly Persistence _persistence;

        /// <inheritdoc />
        public OperationsController(
            ILogger<OperationsController> logger,
            INoSqlDbContext noSqlDbContext,
            BackgroundJobQueue jobQueue,
            IOptions<AwsCdkOptions> options)
        {
            _jobQueue = jobQueue;
            _logger = logger;
            _persistence = new Persistence(noSqlDbContext, options.Value);
        }

        #region Persistence
        // TODO: Convert upload/download file persistence to use S3 or otherwise.
        private static readonly string JobsPath = @"jobs";

        /// <summary>
        /// Saves a job file to the file system.
        /// </summary>
        /// <param name="stream">The stream to write.</param>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <param name="jobId">The unique identifier of the job.</param>
        /// <param name="type">The type of file. For instance: "upload", "download", or "temporary".</param>
        /// <param name="field">The field/filename to store.</param>
        public static async Task SaveJobFileAsync(
            Stream stream,
            string operationId,
            string jobId,
            string type,
            string field
        )
        {
            // We compute the path to the appropriate directory.
            string path = Path.Combine(JobsPath, operationId, jobId, type, field);

            // We create the directory if it does not exist.
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path));

            // We write the file.
            using FileStream fileStream = new(path, FileMode.Create);
            await stream.CopyToAsync(fileStream);
        }
        /// <summary>
        /// Loads a job file from the file system.
        /// </summary>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <param name="jobId">The unique identifier of the job.</param>
        /// <param name="type">The type of file. For instance: "upload", "download", or "temporary".</param>
        /// <param name="field">The field/filename to retrieve.</param>
        /// <returns>The file stream if the file exists. Otherwise, <c>null</c>.</returns>
        public static Task<Stream> LoadJobFileAsync(
            string operationId,
            string jobId,
            string type,
            string field
        )
        {
            // We compute the path to the appropriate directory.
            string path = Path.Combine(JobsPath, operationId, jobId, type, field);

            // We check if the file exists.
            if (!System.IO.File.Exists(path))
            {
                return Task.FromResult<Stream>(null);
            }

            // We read the file.
            FileStream fileStream = new(path, FileMode.Open);
            return Task.FromResult<Stream>(fileStream);
        }

        /// <summary>
        /// Saves the specified operation to the database.
        /// </summary>
        /// <param name="operationItem">The operation item.</param>
        /// <param name="Persistence">A reference to the persistence service.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        public static async Task<bool> SaveOperationAsync(OperationItem operationItem, Persistence Persistence)
        {
            DbDocument document = operationItem.CreateDbDocument();
            bool isSaved = await Persistence.WriteDbDocumentAsync(document);
            return isSaved;
        }
        /// <summary>
        /// Updates the specified operation in the database.
        /// </summary>
        /// <param name="operationItem">The operation item.</param>
        /// <param name="Persistence">A reference to the persistence service.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        public static async Task<bool> UpdateOperationAsync(OperationItem operationItem, Persistence Persistence)
        {
            DbDocument document = operationItem.UpdateDbDocument();
            bool isSaved = await Persistence.WriteDbDocumentAsync(document);
            return isSaved;
        }
        /// <summary>
        /// Deletes the specified operation from the database.
        /// </summary>
        /// <param name="operationId">The identifier of the operation.</param>
        /// <param name="Persistence">A reference to the persistence service.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        public static async Task<bool> DeleteOperationAsync(string operationId, Persistence Persistence)
        {
            OperationItem operationItem = new(operationId);
            DbDocument document = operationItem.DeleteDbDocument();
            bool isDeleted = await Persistence.WriteDbDocumentAsync(document);
            return isDeleted;
        }
        /// <summary>
        /// Loads the specified operation from the database.
        /// </summary>
        /// <param name="operationId">The identifier of the operation.</param>
        /// <param name="Persistence">A reference to the persistence service.</param>
        /// <returns>The loaded operation item.</returns>
        public static async Task<OperationItem> LoadOperationAsync(string operationId, Persistence Persistence)
        {
            OperationItem operationItem = new(operationId);
            Item item = await Persistence.LoadItemAsync(operationItem);
            return (OperationItem)item;
        }

        /// <summary>
        /// Saves the specified job to the database.
        /// </summary>
        /// <param name="jobItem">The job item.</param>
        /// <param name="Persistence">A reference to the persistence service.</param>
        /// <param name="User">The authenticated user.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        public static async Task<bool> SaveJobAsync(JobItem jobItem, Persistence Persistence)
        {
            DbDocument document = jobItem.SaveDbDocument();
            bool isSaved = await Persistence.WriteDbDocumentAsync(document);
            return isSaved;
        }
        /// <summary>
        /// Deletes the specified job from the database.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="operationId">The operation identifier.</param>
        /// <param name="Persistence">A reference to the persistence service.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        public static Task<bool> DeleteJobAsync(string jobId, string operationId, Persistence Persistence)
        {
            JobItem jobItem = new(jobId, operationId);
            DbDocument document = jobItem.DeleteDbDocument();
            return Persistence.WriteDbDocumentAsync(document);
        }
        /// <summary>
        /// Loads the specified job from the database.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="operationId">The operation identifier.</param>
        /// <param name="Persistence">A reference to the persistence service.</param>
        /// <param name="User">The authenticated user.</param>
        /// <returns>The loaded job item.</returns>
        public static async Task<JobItem> LoadJobAsync(
            string jobId,
            string operationId,
            Persistence Persistence)
        {
            JobItem jobItem = new(jobId, operationId);
            Item item = await Persistence.LoadItemAsync(jobItem);
            return (JobItem)item;
        }
        /// <summary>
        /// Loads the jobs for the specified operation from the database.
        /// </summary>
        /// <param name="operationId">The operation identifier.</param>
        /// <param name="Persistence">A reference to the persistence service.</param>
        /// <param name="User">The authenticated user.</param>
        /// <returns>The loaded job items.</returns>
        public static async Task<List<JobItem>> LoadJobsAsync(
            string operationId,
            Persistence Persistence)
        {
            JobItem jobItem = new(null, operationId);
            IEnumerable<Item> items;
            items = await Persistence.LoadItemsAsync(jobItem);
            List<JobItem> jobItems = items.Cast<JobItem>().ToList();
            return jobItems;
        }

        /// <summary>
        /// Instantiates an instance of an operation by loading all relevant information from the database and
        /// constructing the operation objects.
        /// </summary>
        /// <param name="operationItem">The operation item to use as a constructor.</param>
        /// <param name="Persistence">A reference to the persistence service.</param>
        /// <returns>The instantiated operation.</returns>
        public static async Task<Operation> InstantiateOperation(OperationItem operationItem, Persistence Persistence)
        {
            Operation operation = null;
            if (operationItem.Type == "workflow")
            {
                WorkflowItem workflowItem = await WorkflowsController.LoadWorkflowAsync(operationItem.Subtype, Persistence);

                // Load each of the sub-operations of the workflow.
                Operation[] suboperations = new Operation[workflowItem.Operations.Length];
                for (int k = 0; k < suboperations.Length; k++)
                {
                    suboperations[k] = await InstantiateOperation(
                        await LoadOperationAsync(workflowItem.Operations[k], Persistence),
                        Persistence
                    );
                }

                // Load each of the connections of the workflow.
                WorkflowOperationConnection[] connections = new WorkflowOperationConnection[workflowItem.Connections.Length];
                for (int k = 0; k < connections.Length; k++)
                {
                    WorkflowConnection connectionItem = workflowItem.Connections[k];
                    connections[k] = new WorkflowOperationConnection()
                    {
                        Source = new WorkflowOperationConnectionPoint()
                        {
                            Operation = connectionItem.Source.Operation,
                            Field = connectionItem.Source.Field,
                            Multiplicity = connectionItem.Multiplex ? new (int, int?)[] { (0, null) } : Array.Empty<(int, int?)>()
                        },
                        Target = new WorkflowOperationConnectionPoint()
                        {
                            Operation = connectionItem.Target.Operation,
                            Field = connectionItem.Target.Field,
                            Multiplicity = Array.Empty<(int, int?)>()
                        },
                    };
                }

                // Construct the workflow operation.
                WorkflowOperation workflowOperation = new(suboperations, connections);
                await workflowOperation.StabilizeTypes(new OperationJob(workflowOperation, "temp"));
                operation = workflowOperation;
            }
            else
            {
                Type operationType = OperationHelper.FindOperationType(operationItem.Type, typeof(Operation).Assembly);
                operation = OperationHelper.Construct(operationType);
            }

            // Setup the operation identifier and defaults.
            Dictionary<string, object> defaults = new(StringComparer.OrdinalIgnoreCase);
            operation.Id = operationItem.Id;
            operation.Defaults = defaults;

            // Fetch defiend defaults.
            await foreach (OperationFieldDescriptor field in operation.GetInputFields(new OperationJob(operation, "temp")))
            {
                // Get the default value for each field.
                FieldDefaultAttribute defaultAttr = (FieldDefaultAttribute)field.Attributes.FirstOrDefault(attr => attr is FieldDefaultAttribute);
                if (defaultAttr is not null)
                    defaults.Add(field.Name, defaultAttr.Default);
            }
            // Merge in overriding defaults.
            if (operationItem.Default is not null)
            {
                foreach (KeyValuePair<string, object> entry in operationItem.Default)
                    defaults[entry.Key] = entry.Value;
            }

            return operation;
        }
        #endregion

        #region Endpoints (Operation Types)
        /// <summary>
        /// Retrieves a list of operation types that are available to the user. If no filters are specified, this list
        /// will contain all operations. If filters are specified, the operation types must match them.
        /// </summary>
        /// <param name="filterName">
        /// An optional name used to filter operations. If specified, only operations containing this name as a
        /// substring of its display name (case insensitive) will be returned.
        /// </param>
        /// <param name="filterTags">
        /// An optional set of tags used to filter operations. If specified, only operations containing at least one of
        /// the specified tags will be returned.
        /// </param>
        /// <param name="limit">
        /// A limit on the number of results that are returned. If not specified, all operations are returned.
        /// </param>
        /// <returns>
        /// A list of (optionally filtered) operation types with their type and display names, and a description.
        /// </returns>
        [HttpGet("types")]
        public async Task<ActionResult<OperationDescription[]>> GetOperationTypes(
            [FromQuery(Name = "name")] string filterName,
            [FromQuery(Name = "tags")] string[] filterTags,
            [FromQuery(Name = "workspace")] string workspaceId,
            [FromQuery(Name = "limit")] int? limit
        )
        {
            // TODO: (Permissions) This endpoint should be accessible to any user.
            //       For now, until we add permissions on the toolbox level, every operation type is available.

            // Get the descriptions for all of the operations.
            OperationDescription[] descriptionsSimple = OperationHelper.DescribeOperationTypes().ToArray();
            OperationDescription[] descriptionsWorkflow = await WorkflowsController.LoadWorkflowDescriptionsAsync(_persistence);
            OperationDescription[] descriptions = new OperationDescription
            [
                descriptionsSimple.Length + descriptionsWorkflow.Length
            ];
            descriptionsSimple.CopyTo(descriptions, 0);
            descriptionsWorkflow.CopyTo(descriptions, descriptionsSimple.Length);

            // If the tags filter was specified, we filter operations based on the union of tags specified.
            // That is, if any of the specified tags exist on the operation description, that operation is included.
            if (filterTags is not null && filterTags.Length > 0)
            {
                // We use lowercase tags in order to make the API more convenient for a manual user.
                filterTags = filterTags
                    .Select(tag => tag?.ToLower() ?? "")
                    .ToArray();
                descriptions = descriptions
                    .Where(description =>
                    {
                        IEnumerable<string> lowercaseTags = description.Tags.Select(tag => tag.ToLower());
                        return lowercaseTags.Intersect(filterTags).Any();
                    })
                    .ToArray();
            }

            // If the name filter was specified, we sort the names based on their string similarity distance.
            if (filterName is not null)
            {
                // We use the lowercase name in order to make the API more convenient for a manual user.
                filterName = filterName.ToLower();
                descriptions = descriptions
                    .OrderByDescending((description) =>
                    {
                        string source = filterName;
                        string target = description.Display?.ToLower() ?? "";

                        return StringExtensions.SorensenDiceSimilarity(source, target, n: 2);
                    })
                    .ToArray();
            }

            // If the limit is specified, limit the number of results to that number of types.
            if (limit.HasValue)
            {
                descriptions = descriptions
                    .Take(limit.Value)
                    .ToArray();
            }

            return Ok(descriptions);
        }
        #endregion

        #region Endpoints (Operation CRUD)
        /// <summary>
        /// Creates a new operation instance. The type of operation must be specified. The operation identifier cannot
        /// be set by the client.
        /// </summary>
        /// <returns status="200">
        /// The created operation as represented on the service after initialization.
        /// </returns>
        /// <returns status="400">
        /// Nothing if a required property was omitted or invalid or a non-mutable property was specified.
        /// </returns>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<OperationItem>> CreateOperation(
            [FromBody] OperationItem operationItem
        )
        {
            // TODO: (Permissions) This endpoint should be accessible to any user.
            //       For now, until we add permissions on the toolbox level, every operation type is available.

            // Create the operation instance.
            // The defaults for this instance should be automatically generated when constructed.
            Type operationType = OperationHelper.FindOperationType(operationItem.Type);
            OperationDescription description = OperationHelper.DescribeOperationType(operationType);

            // If we did not find an operation type, we return a bad request error.
            if (operationType is null)
            {
                return BadRequest(new
                {
                    Error = "Operation type could not be found.",
                    operationItem.Type
                });
            }

            // Check if the operation is a workflow.
            WorkflowItem workflowItem = null;
            if (operationType.IsAssignableTo(typeof(WorkflowOperation)))
            {
                workflowItem = await WorkflowsController.LoadWorkflowAsync(operationItem.Subtype, _persistence);
                if (workflowItem is null)
                {
                    return BadRequest(new
                    {
                        Error = "Operation workflow type could not be found.",
                        operationItem.Subtype
                    });
                }
            }

            // We make sure that the identifier has not already been set by the user otherwise, this is a bad request.
            if (operationItem.Id is not null)
            {
                return BadRequest(new
                {
                    Error = "Operation identifier should not be specified.",
                    Parameter = nameof(OperationItem.Id)
                });
            }
            // Make sure to set a default name if one was not specified.
            if (operationItem.Name is null)
            {
                operationItem.Name = workflowItem is null ? description.Display : workflowItem.Name;
            }

            // Set a new identifier and defaults for the operation.
            operationItem.Id = Ulid.NewUlid().ToString();
            operationItem.Default ??= new Dictionary<string, object>();
            Operation operation = await InstantiateOperation(operationItem, _persistence);
            operationItem.Id = operation.Id;
            operationItem.Default = operation.Defaults;

            // Save the created operation item and return its internal representation.
            _ = await SaveOperationAsync(operationItem, _persistence);
            return Ok(operationItem);
        }
        /// <summary>
        /// Updates an operation instance specified by its unique identifier. Specified properties will be overwritten
        /// on the resource. Non-specified properties will be left as-is. Neither the operation identifier nor the
        /// operation type can be modified.   
        /// </summary>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <param name="mergingOperationItem">The operation item to merge with the existing one.</param>
        /// <returns status="200">
        /// The updated operation as represented on the service after updating.
        /// </returns>
        /// <returns status="400">
        /// Nothing when the user tries to modify a non-mutable property.
        /// </returns>
        /// <returns status="404">
        /// Nothing when the identifier refers to an operation instance that does not exist.
        /// </returns>
        [Authorize]
        [HttpPatch("{operationId}")]
        public async Task<ActionResult<OperationItem>> UpdateOperation(
            [FromRoute] string operationId,
            [FromBody] OperationItem mergingOperationItem
        )
        {
            // TODO: (Permissions) This endpoint should only be accessible to someone who has "write" permissions to
            //       the referenced operation instance.

            // Get the existing operation instance.
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);
            if (operationItem is null)
            {
                return NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                });
            }

            // Check that no reserved fields have been set.
            if (mergingOperationItem.Id is not null && mergingOperationItem.Id != operationItem.Id)
            {
                return BadRequest(new
                {
                    Error = "Operation identifier cannot be modified.",
                    Parameter = nameof(OperationItem.Id)
                });
            }
            if (
                (mergingOperationItem.Type is not null && mergingOperationItem.Type != operationItem.Type) ||
                (mergingOperationItem.Subtype is not null && mergingOperationItem.Subtype != operationItem.Subtype)
            )
            {
                return BadRequest(new
                {
                    Error = "Operation type cannot be modified.",
                    Parameter = nameof(OperationItem.Type)
                });
            }

            // We merge together the existing and merging operation items.
            mergingOperationItem.Id = operationItem.Id;
            mergingOperationItem.Type = operationItem.Type;
            mergingOperationItem.Subtype = operationItem.Subtype;
            mergingOperationItem.Name ??= operationItem.Name;
            mergingOperationItem.Default ??= operationItem.Default;

            // TODO: Updating an operation may cause connections to become invalid (for instance in dynamic fields).
            //       We should validate if this occurs and remove erraneous connections in this case. 

            // Save the updated operation item and return its internal representation.
            _ = await UpdateOperationAsync(mergingOperationItem, _persistence);
            return Ok(mergingOperationItem);
        }
        /// <summary>
        /// Deletes an operation instance specified by its unique identifier.
        /// </summary>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <returns status="200">
        /// Nothing.
        /// </returns>
        /// <returns status="404">
        /// Nothing when the identifier refers to an operation instance that does not exist.
        /// </returns>
        [Authorize]
        [HttpDelete("{operationId}")]
        public async Task<ActionResult> DeleteOperation(
            [FromRoute] string operationId
        )
        {
            // TODO: (Permissions) This endpoint should only be accessible to someone who has "admin" permissions to
            //       the referenced operation instance.

            // Try to delete the operation item from store.
            bool success = await DeleteOperationAsync(operationId, _persistence);

            // TODO: Delete results associated with the operation.
            // TODO: Delete connections attached to the operation.

            // We return not found if the operation item does not exist.
            return !success
                ? NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                })
                : Ok();
        }
        /// <summary>
        /// Gets an operation instance specified by its unique identifier.
        /// </summary>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <returns status="200">
        /// The operation instance.
        /// </returns>
        /// <returns status="404">
        /// Nothing when the identifier refers to an operation instance that does not exist.
        /// </returns>
        [Authorize]
        [HttpGet("{operationId}")]
        public async Task<ActionResult<OperationItem>> GetOperation(
            [FromRoute] string operationId
        )
        {
            // TODO: (Permissions) This endpoint should only be accessible to someone who has "read" permissions to
            //       the referenced operation instance.

            // We get the operation item from store.
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);

            // We return not found if the operation item does not exist.
            return operationItem is null
                ? NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                })
                : Ok(operationItem);
        }
        #endregion

        #region Endpoints (Operation Execution)
        /// <summary>
        /// Executes an operation specified by its unique identifier on a particular input. 
        /// </summary>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <param name="input">The input to provide to the operation.</param>
        /// <returns status="200">
        /// A unique result identifier that should be used to retrieve the result once available.
        /// </returns>
        /// <returns status="404">
        /// Nothing when the identifier refers to an operation instance that does not exist.
        /// </returns>
        [Authorize]
        [HttpPost("{operationId}/jobs")]
        public async Task<ActionResult<JobItem>> ExecuteOperation(
            [FromRoute] string operationId,
            [FromBody] Dictionary<string, object> input
        )
        {
            // TODO: (Permissions) This endpoint should only be accessible to someone who has "execute" permissions to
            //       the referenced operation instance.

            // We get the operation item from store.
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);

            // We return not found if the operation item does not exist.
            if (operationItem is null)
            {
                return NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                });
            }

            // Get an job ID for the operation.
            byte[] hash = await input.ComputeByteArray().ComputeHashAsync();
            string jobId = hash.ToHexadecimalString();

            // We construct an actual operation instance from the operation item and construct an execution context.
            Operation operation = await InstantiateOperation(operationItem, _persistence);
            OperationJob job = new(operation, jobId, input, new Dictionary<string, object>());

            // TODO: This is temporary to transfer HyperThought authentication details to the operation.
            if (input.TryGetValue("authentication", out object auth) &&
                auth is Dictionary<string, object> authDict)
            {
                job.Authentication.TryAdd(operationId, new ConcurrentDictionary<string, object>(authDict));
                job.Input.TryRemove("authentication", out _);
            }

            // TODO: Implement memoization of results using the hash of the input.
            //       - Account for whether the operation is deterministic.
            //       - Account for file uploads in hashes.

            // TODO: Get the hash of these inputs with respect to the input type (the input type matters because it may
            //       have hashing-related annotations such as [UnsortedArray]).

            // TODO: Modifying any operation inside of a workflow means that cached results for the workflow should be
            //       invalid.


            // Construct a job item for the executing operation instance.
            JobItem jobItem = new(jobId, operationId)
            {
                Completed = false,
                Result = null,
                Tasks = new List<OperationTask>(),
            };

            // We save the job item to the store and push the job onto the job collection.
            // This queues the operation for execution on a separate thread so that we can return the job immediately.
            _ = await SaveJobAsync(jobItem, _persistence);
            _jobQueue.Push(job);

            return Ok(jobItem);
        }
        /// <summary>
        /// Gets all of the jobs that have been executed on the operation instance specified by its unique identifier.
        /// </summary>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <returns status="200">
        /// A list of jobs associated with the operation.
        /// </returns>
        /// <returns status="404">
        /// Nothing when the identifier refers to an operation instance that does not exist.
        /// </returns>
        [Authorize]
        [HttpGet("{operationId}/jobs")]
        public async Task<ActionResult<List<JobItem>>> RetrieveOperationJobs(
            [FromRoute] string operationId
        )
        {
            // TODO: (Permissions) This endpoint should only be accessible to someone who has "execute" permissions to
            //       the referenced operation instance.

            // We get the operation item from store.
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);
            if (operationItem is null)
            {
                return NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                });
            }

            // Get the jobs for the operation.
            List<JobItem> jobs = await LoadJobsAsync(operationId, _persistence);
            return jobs;
        }
        /// <summary>
        /// Gets a particular job that has been executed on the operation instance specified by its unique identifier.
        /// </summary>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <param name="jobId">The unique identifier of the job.</param>
        /// <param name="field">
        /// The specific field to return from the results. If not specified, all fields are returned.
        /// </param>
        /// <param name="selector">
        /// The selector to apply to a specific field. This is only applicable when the field is a graph structure.
        /// If not specified, no data is returned. Query parameters can be used to set parameters on the selector.
        /// </param>
        /// <returns status="200">
        /// A job associated with the operation with the specified result.
        /// </returns>
        /// <returns status="404">
        /// Nothing when the identifier refers to an operation instance that does not exist.
        /// </returns>
        [Authorize]
        [HttpGet("{operationId}/jobs/{jobId}/{field?}/{selector?}")]
        public async Task<ActionResult<JobItem>> RetrieveOperationJob(
            [FromRoute] string operationId,
            [FromRoute] string jobId,
            [FromRoute] string field,
            [FromRoute] string selector
        )
        {
            // TODO: (Permissions) This endpoint should only be accessible to someone who has "execute" permissions to
            //       the referenced operation instance.

            // We get the operation item from store.
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);
            if (operationItem is null)
            {
                return NotFound(new ItemNotFoundError(
                    "Operation with specified identifier could not be found.", operationId
                ));
            }

            // Get the result if it exists.
            JobItem jobItem = await LoadJobAsync(jobId, operationId, _persistence);
            if (jobItem is null)
            {
                return NotFound(new ItemNotFoundError(
                    "Job with specified identifier could not be found.", jobId
                ));
            }

            // Return only the specified field on request.
            if (field is not null)
            {
                if (jobItem.Result is Dictionary<string, object> resultDictionary &&
                    resultDictionary.TryGetValue(field, out object result))
                    jobItem.Result = result;
            }

            // Perform a selection on a graph result if possible.
            if (selector is not null)
            {
                // Try to construct the appropriate selector.
                Type selectorType = OperationHelper.FindSelectorType(selector);
                if (selectorType is null)
                {
                    return BadRequest(new
                    {
                        Error = "Invalid selector was specified.",
                        Selector = selector
                    });
                }
                object selectorObject = OperationHelper
                    .ConstructSelector(selectorType, out object selectorParameters);
                await TryUpdateModelAsync(selectorParameters, selectorParameters.GetType(), "selector");
                if (selectorObject is not ISelector<Graph, Graph> selectorOperation)
                {
                    return BadRequest(new
                    {
                        Error = "Invalid selector was specified.",
                        Selector = selector
                    });
                }

                // Fetch the type of the field to ensure it is a graph and get its type arguments.
                Operation operation = await InstantiateOperation(operationItem, _persistence);
                OperationJob job = jobItem.AsJob(operation);
                OperationFieldDescriptor fieldDescriptor = await operation
                    .GetOutputFields(job)
                    .FirstOrDefaultAsync(fieldDescriptor => fieldDescriptor.Name == field);

                if (fieldDescriptor is null ||
                   !fieldDescriptor.Type.ImplementsGenericInterface(typeof(IEnumerableComponent<,>), out Type[] genericArguments))
                {
                    return BadRequest(new
                    {
                        Error = "Invalid field was specified. Either field does not exist or is not selectable.",
                        Name = field
                    });
                }

                // Construct a file-based graph for this field.
                Type graphType = typeof(FileGraph<,>).MakeGenericType(genericArguments);
                string graphPath = Path.Join("jobs", operationId, jobId, field);
                Graph graph = (Graph)Activator.CreateInstance(graphType, new object[] { graphPath, field });
                Graph selectedGraph = await selectorOperation.Select(graph, selectorParameters);

                // Set the result to the selected graph for formatting.
                Type jsonGraphType = typeof(JsonGraph<,>).MakeGenericType(genericArguments);
                object jsonGraph = Activator.CreateInstance(jsonGraphType, Array.Empty<object>());
                await (Task)jsonGraphType
                    .GetMethod(nameof(JsonGraph<IVertex, IEdge>.Initialize))
                    .Invoke(jsonGraph, new object[] { selectedGraph });
                jobItem.Result = jsonGraph;
            }

            return jobItem;
        }
        [Authorize]
        [HttpPost("{operationId}/jobs/{jobId}/{field}/{selector}/prioritize")]
        public async Task<ActionResult> PrioritizeOperationJob(
            [FromRoute] string operationId,
            [FromRoute] string jobId,
            [FromRoute] string field,
            [FromRoute] string selector
        )
        {
            // TODO: (Permissions) This endpoint should only be accessible to someone who has "execute" permissions to
            //       the referenced operation instance.

            // Get the job if it exists.
            OperationJob job = _jobQueue.Seek(jobId);
            if (job is null)
            {
                return NotFound(new
                {
                    Error = "Job with specified identifier could not be found.",
                    Id = operationId
                });
            }

            // Compute the selector.
            Type selectorType = OperationHelper.FindSelectorType(selector);
            object selectorOperation = OperationHelper.ConstructSelector(selectorType, out object selectorParameters);
            await TryUpdateModelAsync(selectorParameters, selectorParameters.GetType(), "selector");

            // Add the selector to the priority queue of the context.
            if (!job.PriorityQueue.ContainsKey(field))
                job.PriorityQueue.TryAdd(field, new ConcurrentQueue<(object, object)>());

            job.PriorityQueue.TryGetValue(field, out ConcurrentQueue<(object, object)> queue);
            queue.Enqueue((selectorOperation, selectorParameters));

            return Ok();
        }

        // TODO: Review API URLs.
        /// <summary>
        /// Uploads a file to a particular job awaiting a file upload.
        /// </summary>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <param name="jobId">The unique identifier of the job.</param>
        /// <param name="field">The field to upload into.</param>
        /// <param name="file">The file to upload.</param>
        /// <returns status="200">The job after the upload task has been completed.</returns>
        /// <returns status="404">Nothing when either the operation or job could not be found.</returns>
        [Authorize]
        [HttpPost("{operationId}/jobs/{jobId}/{field}/upload")]
        public async Task<ActionResult<JobItem>> UploadFileToOperation(
            [FromRoute] string operationId,
            [FromRoute] string jobId,
            [FromRoute] string field,
            [FromForm] IFormFile file
        )
        {
            // TODO: (Permissions) This endpoint should only be accessible to someone who has "execute" permissions to
            //       the referenced operation instance.

            // Get the job if it exists.
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);
            if (operationItem is null)
            {
                return NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                });
            }

            // Get the job if it exists.
            JobItem jobItem = await LoadJobAsync(jobId, operationId, _persistence);
            if (jobItem is null)
            {
                return NotFound(new
                {
                    Error = "Job with specified identifier could not be found.",
                    Id = operationId
                });
            }

            // Save the job file as an upload.
            await SaveJobFileAsync(file.OpenReadStream(), operationId, jobId, "upload", field);

            // Construct the operation and executing context.
            Operation operation = await InstantiateOperation(operationItem, _persistence);
            OperationJob job = new(operation, jobItem.Id, jobItem.Value, new Dictionary<string, object>());

            // Add back all of the tasks except for the upload task just completed.
            OperationTask currentTask = jobItem.Tasks.Find((task) =>
            {
                return
                    task.Operation == operationId &&
                    task.Field == field &&
                    task.Type == OperationTaskType.File
                ;
            });
            _ = jobItem.Tasks.Remove(currentTask);
            foreach (OperationTask task in jobItem.Tasks)
            {
                // context.Tasks.Add(task);
            }

            // Save the job item and requeue the operation.
            _ = await SaveJobAsync(jobItem, _persistence);
            _jobQueue.Push(job);
            return Ok(jobItem);
        }
        /// <summary>
        /// Downloads a file from a particular job that output a file download.
        /// </summary>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <param name="jobId">The unique identifier of the job.</param>
        /// <param name="field">The field to download from.</param>
        /// <returns status="200">A stream containing the file download.</returns>
        /// <returns status="404">Nothing when either the operation or job could not be found.</returns>
        [Authorize]
        [HttpGet("{operationId}/jobs/{jobId}/{field}/download")]
        public async Task<ActionResult> DownloadFileFromOperation(
            [FromRoute] string operationId,
            [FromRoute] string jobId,
            [FromRoute] string field
        )
        {
            // TODO: (Permissions) This endpoint should only be accessible to someone who has "execute" permissions to
            //       the referenced operation instance.

            // Get the operation if it exists.
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);
            if (operationItem is null)
            {
                return NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                });
            }

            // Get the job if it exists.
            JobItem jobItem = await LoadJobAsync(jobId, operationId, _persistence);
            if (jobItem is null)
            {
                return NotFound(new
                {
                    Error = "Job with specified identifier could not be found.",
                    Id = operationId
                });
            }

            // Get the file from the job.
            Stream file = await LoadJobFileAsync(operationId, jobId, "download", field);
            return file is null ? StatusCode(423, new { Error = "File download is not ready." }) : File(file, "application/octet-stream", field);
        }
        /// <summary>
        /// Authenticates 
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="jobId"></param>
        /// <param name="authenticationType"></param>
        /// <param name="operations"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("{operationId}/jobs/{jobId}/authenticate/{authenticationType}")]
        public async Task<ActionResult> AuthenticateOperation(
            [FromRoute] string operationId,
            [FromRoute] string jobId,
            [FromRoute] string authenticationType,
            [FromQuery] string[] operations,
            [FromBody] object credentials
        )
        {
            // TODO: (Permissions) This endpoint should only be accessible to someone who has "execute" permissions to
            //       the referenced operation instance.

            // Get the operation if it exists.
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);
            if (operationItem is null)
            {
                return NotFound(new ItemNotFoundError(
                    "Operation with specified identifier could not be found.",
                    operationId
                ));
            }

            // Get the job if it exists.
            JobItem jobItem = await LoadJobAsync(jobId, operationId, _persistence);
            if (jobItem is null)
            {
                return NotFound(new ItemNotFoundError(
                    "Job with specified identifier could not be found.",
                    jobId
                ));
            }

            // If no operation ID was specified, then we add the authentication to the current operation.
            // This will be the case if the operation is not a workflow operation.
            if (operations is null || operations.Length == 0)
                operations = new[] { operationId };

            // TODO: Verify that if the operation is not a workflow, the only applicable operations are this one.
            // TODO: Verify that if the operation is a workflow, the only applicable operations are suboperations.
            // Add the authentication to all of the specified operations.
            foreach (string operation in operations)
            {
                // TODO: We should check to make sure that we are not overposting here.
                if (!jobItem.Authentication.ContainsKey(operation))
                    jobItem.Authentication.Add(operation, new());
                jobItem.Authentication[operation][authenticationType] = credentials;
            }

            return Ok();
        }
        #endregion

        #region Endpoints (Schema)
        /// <summary>
        /// Represents a schema for an operation.
        /// </summary>
        public struct OperationSchema
        {
            /// <summary>
            /// The schemas for each input field of the operation.
            /// </summary>
            public Dictionary<string, JsonSchema> Inputs { get; init; }
            /// <summary>
            /// The schemas for each output field of the operation.
            /// </summary>
            public Dictionary<string, JsonSchema> Outputs { get; init; }
        }

        /// <summary>
        /// Retrieves the JSON schema of the specified operation. The schemas for both the input and output fields are
        /// generated. Notice that the schemas may be dependent on the current default values of the operation.
        /// </summary>
        /// <param name="operationId">The unique identifier of the operation.</param>
        /// <returns status="200">
        /// The JSON schema of the specified operation sorted by input and output fields.
        /// </returns>
        /// <returns status="404">
        /// Nothing when the identifier refers to an operation instance that does not exist.
        /// </returns>
        [Authorize]
        [HttpGet("{operationId}/schema")]
        public async Task<ActionResult<OperationSchema>> RetrieveOperationSchema(
            [FromRoute] string operationId
        )
        {
            // TODO: (Permissions) This endpoint should only be accessible to someone who has "read" permissions to
            //       the referenced operation instance.

            // We get the operation item from store.
            OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);
            if (operationItem is null)
            {
                return NotFound(new ItemNotFoundError(
                    "Operation with specified identifier could not be found.", operationId
                ));
            }

            // Instantiate the operation and job.
            Operation operation = await InstantiateOperation(operationItem, _persistence);
            OperationJob job = new(operation, null);

            // Construct the schema for each input and output field found.
            Dictionary<string, JsonSchema> inputSchemas = new();
            await foreach (OperationFieldDescriptor descriptor in operation.GetInputFields(job))
                inputSchemas.Add(descriptor.Name, descriptor.Schema);
            Dictionary<string, JsonSchema> outputSchemas = new();
            await foreach (OperationFieldDescriptor descriptor in operation.GetOutputFields(job))
                outputSchemas.Add(descriptor.Name, descriptor.Schema);

            // Return the operation schema.
            OperationSchema schema = new()
            {
                Inputs = inputSchemas,
                Outputs = outputSchemas
            };
            return Ok(schema);
        }
        #endregion
    }
}