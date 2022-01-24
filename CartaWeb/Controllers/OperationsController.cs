using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartaCore.Extensions.Array;
using CartaCore.Extensions.Hashing;
using CartaCore.Extensions.String;
using CartaCore.Operations;
using CartaCore.Persistence;
using CartaWeb.Models.DocumentItem;
using CartaWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NUlid;

namespace CartaWeb.Controllers
{
    // TODO: Make sure that operation defaults are added to the internal representation of operations once created.
    // TODO: Prevent overposting to the default fields of operations.
    // TODO: Add searching for workflows in search endpoint
    // TODO: Implement destroying results records manually.
    // TODO: Updating an operation causes the containing workflow to become cache invalid.

    /// <summary>
    /// A controller that manages creating, updating, retrieving, and deleting operations that can be executed with
    /// inputs to produce a set of outputs as a result.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OperationsController : ControllerBase
    {
        // TODO: This singleton service should probably be interface-ified and should be given a better name.
        private readonly OperationJobCollection _jobCollection;
        private readonly ILogger<OperationsController> _logger;
        private readonly Persistence _persistence;

        /// <inheritdoc />
        public OperationsController(
            ILogger<OperationsController> logger,
            INoSqlDbContext noSqlDbContext,
            OperationJobCollection taskCollection)
        {
            _jobCollection = taskCollection;
            _logger = logger;
            _persistence = new Persistence(noSqlDbContext);
        }

        #region Persistence
        /// <summary>
        /// Saves the specified operation to the database.
        /// </summary>
        /// <param name="operationItem">The operation item.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        public static async Task<bool> SaveOperationAsync(OperationItem operationItem, Persistence _persistence)
        {
            DbDocument document = operationItem.CreateDbDocument();
            bool isSaved = await _persistence.WriteDbDocumentAsync(document);
            return isSaved;
        }
        /// <summary>
        /// Updates the specified operation in the database.
        /// </summary>
        /// <param name="operationItem">The operation item.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        public static async Task<bool> UpdateOperationAsync(OperationItem operationItem, Persistence _persistence)
        {
            DbDocument document = operationItem.UpdateDbDocument();
            bool isSaved = await _persistence.WriteDbDocumentAsync(document);
            return isSaved;
        }
        /// <summary>
        /// Deletes the specified operation from the database.
        /// </summary>
        /// <param name="operationId">The identifier of the operation.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        public static async Task<bool> DeleteOperationAsync(string operationId, Persistence _persistence)
        {
            OperationItem operationItem = new(operationId);
            DbDocument document = operationItem.DeleteDbDocument();
            bool isDeleted = await _persistence.WriteDbDocumentAsync(document);
            return isDeleted;
        }
        /// <summary>
        /// Loads the specified operation from the database.
        /// </summary>
        /// <param name="operationId">The identifier of the operation.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>The loaded operation item.</returns>
        public static async Task<OperationItem> LoadOperationAsync(string operationId, Persistence _persistence)
        {
            OperationItem operationItem = new(operationId);
            Item item = await _persistence.LoadItemAsync(operationItem);
            return (OperationItem)item;
        }

        /// <summary>
        /// Saves the specified job to the database.
        /// </summary>
        /// <param name="jobItem">The job item.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        public static async Task<bool> SaveJobAsync(JobItem jobItem, Persistence _persistence)
        {
            DbDocument document = jobItem.CreateDbDocument();
            bool isSaved = await _persistence.WriteDbDocumentAsync(document);
            return isSaved;
        }
        /// <summary>
        /// Updates the specified job in the database.
        /// </summary>
        /// <param name="jobItem">The job item.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>Whether the operation was succesfful or not.</returns>
        public static async Task<bool> UpdateJobAsync(JobItem jobItem, Persistence _persistence)
        {
            DbDocument document = jobItem.UpdateDbDocument();
            bool isSaved = await _persistence.WriteDbDocumentAsync(document);
            return isSaved;
        }
        /// <summary>
        /// Deletes the specified job from the database.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="operationId">The operation identifier.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>Whether the operation was successful or not.</returns>
        public static Task<bool> DeleteJobAsync(string jobId, string operationId, Persistence _persistence)
        {
            JobItem jobItem = new(jobId, operationId);
            DbDocument document = jobItem.DeleteDbDocument();
            return _persistence.WriteDbDocumentAsync(document);
        }
        /// <summary>
        /// Loads the specified job from the database.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="operationId">The operation identifier.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>The loaded job item.</returns>
        public static async Task<JobItem> LoadJobAsync(string jobId, string operationId, Persistence _persistence)
        {
            JobItem jobItem = new(jobId, operationId);
            Item item = await _persistence.LoadItemAsync(jobItem);
            return (JobItem)item;
        }
        /// <summary>
        /// Loads the jobs for the specified operation from the database.
        /// </summary>
        /// <param name="operationId">The operation identifier.</param>
        /// <param name="_persistence">A reference to the persistence service.</param>
        /// <returns>The loaded job items.</returns>
        public static async Task<List<JobItem>> LoadJobsAsync(string operationId, Persistence _persistence)
        {
            JobItem jobItem = new(null, operationId);
            IEnumerable<Item> items = await _persistence.LoadItemsAsync(jobItem);
            List<JobItem> jobItems = items.Cast<JobItem>().ToList();
            return jobItems;
        }

        public static async Task<Operation> InstantiateOperation(OperationItem operationItem, Persistence _persistence)
        {
            if (operationItem.Type == "workflow")
            {
                WorkflowItem workflowItem = await WorkflowsController.LoadWorkflowAsync(operationItem.Subtype, _persistence);
                List<Operation> suboperations = new(workflowItem.Operations.Length);
                foreach (string suboperationId in workflowItem.Operations)
                {
                    suboperations.Add(
                        await InstantiateOperation(
                            await LoadOperationAsync(suboperationId.ToString(), _persistence),
                            _persistence
                        )
                    );
                }
                WorkflowOperationConnection[] connections = workflowItem.Connections.Select(item => new WorkflowOperationConnection()
                {
                    Source = new WorkflowOperationConnectionPoint()
                    {
                        Operation = item.Source.Operation,
                        Field = item.Source.Field
                    },
                    Target = new WorkflowOperationConnectionPoint()
                    {
                        Operation = item.Target.Operation,
                        Field = item.Target.Field
                    },
                    Multiplexer = item.Multiplex
                }).ToArray();

                WorkflowOperation operation = new(suboperations.ToArray(), connections)
                {
                    Identifier = operationItem.Id
                };
                operation.DefaultValues = operationItem.Default;

                return operation;
            }
            else
            {
                // TODO: Clean up and make private init members private init again.
                Type operationType = Operation.TypeFromName(operationItem.Type);

                Operation operation = (Operation)Activator.CreateInstance(operationType);
                operation.Identifier = operationItem.Id;
                operation.DefaultValues = operationItem.Default;

                return operation;
            }
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
            [FromQuery(Name = "limit")] int? limit
        )
        {
            // TODO: (Permissions) This endpoint should be accessible to any user.
            //       For now, until we add permissions on the toolbox level, every operation type is available.

            // Get the descriptions for all of the operations.
            OperationDescription[] descriptionsSimple = OperationDescription.FromOperationTypes();
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
                        string target = description.Display.ToLower();

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
        [HttpPost]
        public async Task<ActionResult<OperationItem>> CreateOperation(
            [FromBody] OperationItem operationItem
        )
        {
            // TODO: (Permissions) This endpoint should be accessible to any user.
            //       For now, until we add permissions on the toolbox level, every operation type is available.

            // Create the operation instance.
            // The defaults for this instance should be automatically generated when constructed.
            Type operationType = Operation.TypeFromName(operationItem.Type);
            OperationDescription description = OperationDescription.FromType(operationType);

            // If we did not find an operation type, we return a bad request error.
            if (operationType is null)
            {
                return BadRequest(new
                {
                    Error = "Operation type could not be found.",
                    Type = operationItem.Type
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
                        Subtype = operationItem.Subtype
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
            Operation operation = await InstantiateOperation(operationItem, _persistence);
            operationItem.Id = Ulid.NewUlid().ToString();
            operationItem.Default ??= operation.InitialValues;

            // Save the created operation item and return its internal representation.
            await SaveOperationAsync(operationItem, _persistence);
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
            mergingOperationItem.Default ??= operationItem.Default;

            // TODO: Updating an operation may cause connections to become invalid (for instance in dynamic fields).
            //       We should validate if this occurs and remove erraneous connections in this case. 

            // Save the updated operation item and return its internal representation.
            await UpdateOperationAsync(mergingOperationItem, _persistence);
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
            if (!success)
            {
                return NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                });
            }
            else return Ok();
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
            if (operationItem is null)
            {
                return NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                });
            }
            else return Ok(operationItem);
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

            // We construct an actual operation instance from the operation item and construct an execution context.
            Operation operation = await InstantiateOperation(operationItem, _persistence);
            OperationContext context = new()
            {
                Parent = null,
                Operation = operation,

                Default = operationItem.Default,
                Input = input,
                Output = new Dictionary<string, object>(),

                Threads = 32,
            };

            // TODO: Implement memoization of results using the hash of the input.
            //       - Account for whether the operation is deterministic.
            //       - Account for file uploads in hashes.

            // TODO: Get the hash of these inputs with respect to the input type (the input type matters because it may
            //       have hashing-related annotations such as [UnsortedArray]).

            // TODO: Modifying any operation inside of a workflow means that cached results for the workflow should be
            //       invalid.


            // Construct a job item for the executing operation instance.
            byte[] hash = await context.Total.ComputeByteArray().ComputeHashAsync();
            string jobId = hash.ToHexadecimalString();
            JobItem job = new(jobId, operationId)
            {
                Completed = false,
                Value = context.Total,
                Result = null,
                Tasks = new List<OperationTask>(),
            };

            // We save the job item to the store and push the job onto the job collection.
            // This queues the operation for execution on a separate thread so that we can return the job immediately.
            await SaveJobAsync(job, _persistence);
            _jobCollection.Push((job, operation, context));

            return Ok(job);
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
        /// If not specified, no data is returned.
        /// </param>
        /// <returns status="200">
        /// A job associated with the operation with the specified result.
        /// </returns>
        /// <returns status="404">
        /// Nothing when the identifier refers to an operation instance that does not exist.
        /// </returns>
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
                return NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                });
            }

            // Get the result if it exists.
            JobItem job = await LoadJobAsync(jobId, operationId, _persistence);
            if (job is null)
            {
                return NotFound(new
                {
                    Error = "Result with specified identifier could not be found.",
                    Id = operationId
                });
            }
            else
            {
                // Return only the specified field on request.
                if (field is not null)
                    job.Result = ((Dictionary<string, object>)job.Result)[field];

                // TODO: Implement support for applying selectors to graph-like fields.

                return job;
            }
        }
        // TODO: Reimplement.
        // [HttpPost("{operationId}/jobs/{jobId}/{field}/{selector}/prioritize")]
        // public async Task<ActionResult> PrioritizeOperationJob(
        //     [FromRoute] string operationId,
        //     [FromRoute] string jobId,
        //     [FromRoute] string field,
        //     [FromRoute] string selector
        // )
        // {
        //     // TODO: (Permissions) This endpoint should only be accessible to someone who has "execute" permissions to
        //     //       the referenced operation instance.

        //     // Get the job if it exists.
        //     (JobItem job, Operation operation, OperationContext context) = _jobCollection.Seek(jobId);
        //     if (job is null)
        //     {
        //         return NotFound(new
        //         {
        //             Error = "Result with specified identifier could not be found.",
        //             Id = operationId
        //         });
        //     }

        //     // Compute the selector.
        //     Operation selectorOperation = Operation.ConstructSelector(selector, out object selectorInput, out object _);
        //     await TryUpdateModelAsync(selectorInput, selectorInput.GetType(), "");
        //     Selector selectorInstance = new Selector(selectorOperation);

        //     // Add the selector to the priority queue of the context.
        //     context.Selectors.Enqueue(new(selectorInstance, selectorInput));

        //     return Ok();
        // }

        // // TODO: Review API URLs.
        // [HttpPost("{operationId}/jobs/{jobId}/{field}/upload")]
        // public async Task<ActionResult<JobItem>> UploadFileToOperation(
        //     [FromRoute] string operationId,
        //     [FromRoute] string jobId,
        //     [FromRoute] string field,
        //     [FromForm] IFormFile file
        // )
        // {
        //     // TODO: (Permissions) This endpoint should only be accessible to someone who has "execute" permissions to
        //     //       the referenced operation instance.

        //     OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);
        //     Operation operation = await InstantiateOperation(operationItem, _persistence);
        //     JobItem jobItem = await LoadJobAsync(jobId, operationId, _persistence);

        //     OperationContext context = new()
        //     {
        //         Parent = null,
        //         Operation = operation,

        //         Default = new Dictionary<string, object>(operationItem.Default),
        //         Input = new Dictionary<string, object>(jobItem.Value),
        //         Output = new Dictionary<string, object>(),
        //     };
        //     SaveFile(file.OpenReadStream());

        //     jobItem.Tasks = new List<OperationTask>(operation.GetTasks(context));

        //     await SaveJobAsync(jobItem, _persistence);
        //     _jobCollection.Push((jobItem, operation, context));
        //     return Ok(jobItem);
        // }
        // public async Task<ActionResult> DownloadFileFromOperation(
        //     [FromRoute] string operationId,
        //     [FromRoute] string jobId,
        //     [FromRoute] string field
        // )
        // {
        //     // TODO: (Permissions) This endpoint should only be accessible to someone who has "execute" permissions to
        //     //       the referenced operation instance.

        //     OperationItem operationItem = await LoadOperationAsync(operationId, _persistence);
        //     Operation operation = await InstantiateOperation(operationItem, _persistence);
        //     JobItem jobItem = await LoadJobAsync(jobId, operationId, _persistence);

        //     return File(stream, "application/octet-stream", field);
        // }
        // // TODO: Implement.
        // public async Task<ActionResult> AuthenticateOperation(

        // )
        // {
        //     // TODO: (Permissions) This endpoint should only be accessible to someone who has "execute" permissions to
        //     //       the referenced operation instance.

        //     return Ok();
        // }
        #endregion

        #region Endpoints (Schema)
        /// <summary>
        /// Represents a schema for an operation.
        /// </summary>
        public struct OperationSchema
        {
            /// <summary>
            /// The schemas for each input field of the operation.
            /// Notice that the schemas are already converted to JSON.
            /// </summary>
            public Dictionary<string, string> Inputs { get; init; }
            /// <summary>
            /// The schemas for each output field of the operation.
            /// Notice that the schemas are already converted to JSON.
            /// </summary>
            public Dictionary<string, string> Outputs { get; init; }
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
        [HttpGet("{operationId}/schema/{side}")]
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
                return NotFound(new
                {
                    Error = "Operation with specified identifier could not be found.",
                    Id = operationId
                });
            }

            // Instantiate the operation and retrieve each of the field schemas.
            Operation operation = await InstantiateOperation(operationItem, _persistence);
            Dictionary<string, string> inputSchemas = operation
                .GetInputFields()
                .ToDictionary(
                    field => field,
                    field => operation.GetInputFieldSchema(field).ToJson()
                );
            Dictionary<string, string> outputSchemas = operation
                .GetOutputFields()
                .ToDictionary(
                    field => field,
                    field => operation.GetOutputFieldSchema(field).ToJson()
                );

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