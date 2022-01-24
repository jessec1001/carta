using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    // TODO: We need to tee streams if we want to support multiple outputs.

    /// <summary>
    /// Represents an operation that executes as a topologically-sorted graph of sub-operations. Connections determine
    /// how outputs from some operations are bound to inputs for other operations.
    /// </summary>
    [OperationTag(OperationTags.Workflow)]
    [OperationName(Type = "workflow", Display = "Workflow")]
    public class WorkflowOperation : Operation
    {
        /// <summary>
        /// A class that provides a means of cleanly and efficiently running a workflow.
        /// </summary>
        private class WorkflowOperationRunner
        {
            /// <summary>
            /// The workflow operation that is running.
            /// </summary>
            private WorkflowOperation Workflow { get; init; }
            /// <summary>
            /// The context of the workflow operation.
            /// </summary>
            private OperationContext Context { get; init; }

            /// <summary>
            /// Stores the results of executed operations.
            /// </summary>
            private readonly ConcurrentDictionary<string, Dictionary<string, object>> Results = new();

            /// <summary>
            /// Constructs a new instance of the <see cref="WorkflowOperationRunner" /> class with the specified
            /// workflow and context.
            /// </summary>
            /// <param name="workflow">The workflow to run.</param>
            /// <param name="context">The context under which the workflow is running.</param>
            public WorkflowOperationRunner(WorkflowOperation workflow, OperationContext context)
            {
                Workflow = workflow;
                Context = context;
            }

            /// <summary>
            /// Runs the workflow operation by executing all of its sub-operations in order.
            /// </summary>
            /// <returns>Nothing.</returns>
            public async Task Run()
            {
                // TODO: Formalize and replace with per-context configuration.
                int operationRunningLimit = 32;

                // TODO: Generalize to all types of operations instead of just workflow operations.
                // Before starting, we need to load any file streams that are referenced by the workflow.
                foreach (string inputField in Workflow.GetInputFields())
                {
                    Type inputType = Workflow.GetInputFieldType(inputField);
                    if (inputType.IsAssignableTo(typeof(Stream)))
                    {
                        Stream fileUpload = await Context.LoadFile(Context.OperationId, Context.JobId, "upload", inputField);
                        Context.Input.Add(inputField, fileUpload);
                    }
                }

                // Keep a list of currently running operation tasks.
                // Keep executing operations while there are tasks in this list.
                List<Task<string>> running = new();
                List<string> runningIds = new();
                do
                {
                    // Find all new operations that can start running.
                    foreach (Operation operation in Workflow.Operations)
                    {
                        if (running.Count >= operationRunningLimit) break;
                        if (!Results.ContainsKey(operation.Identifier) && CanResolveOperation(operation.Identifier))
                        {
                            running.Add(ResolveOperation(operation.Identifier));
                            runningIds.Add(operation.Identifier);
                        }
                    }

                    // If we have run out of tasks, stop running.
                    if (running.Count == 0)
                        break;

                    // Wait for any task to complete running.
                    Task<string> task = await Task.WhenAny(running);
                    string id = await task;
                    int index = runningIds.IndexOf(id);
                    running.RemoveAt(index);
                    runningIds.RemoveAt(index);
                } while (true);

                // TODO: Generalize to all types of operations instead of just workflow operations.
                // After running, we need to save any file streams that are referenced by the workflow.
                foreach (string outputField in Workflow.GetOutputFields())
                {
                    Type outputType = Workflow.GetOutputFieldType(outputField);
                    if (outputType.IsAssignableTo(typeof(Stream)))
                    {
                        if (!Context.Output.ContainsKey(outputField)) continue;
                        if (Context.Output[outputField] is not Stream fileDownload) continue;

                        await Context.SaveFile(fileDownload, Context.OperationId, Context.JobId, "download", outputField);
                        Context.Output.Remove(outputField);
                    }
                }
            }

            /// <summary>
            /// Checks if an operation can be resolved by executing it. An operation cannot be resolved if it depends on
            /// other operations that have not yet been resolved.
            /// </summary>
            /// <param name="operationId">The unique identifier of the operation.</param>
            /// <returns><c>true</c> if the operation can be resolved; otherwise, <c>false</c>.</returns>
            private bool CanResolveOperation(string operationId)
            {
                // Check if the operation has a dependency on another operation that has not yet resolved.
                // If there is such a dependency, then this operation cannot be resolved.
                foreach (WorkflowOperationConnection connection in Workflow.Connections)
                {
                    if (connection.Target.Operation == operationId)
                    {
                        if (!Results.ContainsKey(connection.Source.Operation))
                            return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// Resolves an operation by executing it and storing its results.
            /// </summary>
            /// <param name="operationId">The unique identifier of the operation.</param>
            /// <returns>The unique identifier of the operation that was resolved.</returns>
            private async Task<string> ResolveOperation(string operationId)
            {
                // TODO: How might we break up a multiplexed operation into multiple simultaneous operations?
                // TODO: Related to above, should we induce a limit on the number of operations that can be running in
                //       a workflow at any one time?

                // TODO: Finish details once multiplexer and property connections are completed.
                // The following describes some ways in which we might resolve an operation.
                /*
                    In the most general infrastructure of our platform, each operation may be running completely
                    independently of any other operation inside of a workflow. However, it is unlikely in the near
                    future, that a single operation with a single set of inputs would be executed on multiple different
                    compute instances. Thus, we resolve each single operation as an atomic unit of computation.
                */

                // TODO: For now, we will assume that an operation that has multiplexed inputs should be run as a single
                //       atomic operation inside of a loop. In the future, we should have multiplexed operations
                //       executed per-item as an atomic unit and dependencies are fulfilled when all of instances of an
                //       operation are finished executing.

                /*
                    - NOTICE -
                    We do a bit of fancy type conversion here to assign connection values to/from the appropriate
                    input and output values on the operations.
                */

                // We execute the operation from input to output.
                // We take into consideration a parameter set on the workflow that allows for memoization.
                Operation operation = GetOperation(operationId);
                // TODO: Use the total property of the operation context to get these fields.
                Dictionary<string, object> input = GetInput(operationId);
                OperationContext totalContext = new OperationContext()
                {
                    Parent = Context,
                    Operation = operation,

                    Input = input,
                    Output = new Dictionary<string, object>(),
                    Default = operation.DefaultValues
                };
                Dictionary<string, object> total = totalContext.Total;

                // We translate property connections into this operation.
                foreach (WorkflowOperationConnection connection in Workflow.Connections)
                {
                    if (connection.Property is not null && connection.Target.Operation == operationId)
                    {
                        Graph graph = (Graph)total[connection.Target.Field];
                        if (((IGraph)graph).TryProvide(out IEntireGraph entireGraph))
                        {
                            total[connection.Target.Field] = await entireGraph.GetVertices()
                                .Select(vertex => vertex.Properties
                                    .FirstOrDefault(property => string.Equals(
                                        property.Identifier.ToString(), connection.Property, StringComparison.OrdinalIgnoreCase)
                                    )?.Value
                                ).ToArrayAsync();
                        }
                    }
                }

                // We grab the multiplexed fields into this operation.
                string[] multiplexed = GetOperationMultiplexedFields(operationId);

                // TODO: For now, we will grab the cardinality of the least multiplexed element. In the future, we just
                //       want defaults for values not in range.
                int cardinality = 1;
                if (multiplexed.Length > 0)
                {
                    cardinality = int.MaxValue;
                    for (int k = 0; k < multiplexed.Length; k++)
                    {
                        if (!total.TryGetValue(multiplexed[k], out object value))
                            throw new Exception("Multiplexed property not found.");
                        if (!value.GetType().IsArray)
                            throw new Exception("Multiplexed value is not an array.");

                        cardinality = Math.Min(cardinality, ((System.Array)value).Length);
                    }
                }

                // TODO: For now, we will handle multiplexing and non-multiplexing differently. However, multiplexing is
                //       the more general case where we have an array of inputs to execute. The non-multiplexing case
                //       occurs when each input has an implied multiplicity of 1. 
                Dictionary<string, object> result;
                if (multiplexed.Length > 0)
                {
                    result = new Dictionary<string, object>();

                    for (int k = 0; k < cardinality; k++)
                    {
                        Dictionary<string, object> partial = new();
                        foreach (KeyValuePair<string, object> entry in total)
                        {
                            if (multiplexed.Contains(entry.Key))
                                partial.Add(entry.Key, ((System.Array)entry.Value).GetValue(k));
                            else
                                partial.Add(entry.Key, entry.Value);
                        }

                        OperationContext context = new()
                        {
                            Parent = Context,
                            Operation = operation,

                            Input = partial,
                            Output = new Dictionary<string, object>(),
                            Default = new Dictionary<string, object>()
                        };

                        await operation.Perform(context);
                        foreach (KeyValuePair<string, object> entry in context.Output)
                        {
                            if (!result.ContainsKey(entry.Key))
                                result.Add(entry.Key, new object[cardinality]);

                            System.Array value = (System.Array)result[entry.Key];
                            value.SetValue(entry.Value, k);
                        }
                    }
                }
                else
                {
                    OperationContext context = new()
                    {
                        Parent = Context,
                        Operation = operation,

                        Input = total,
                        Output = new Dictionary<string, object>(),
                        Default = new Dictionary<string, object>()

                        // TODO: Update thread count.
                    };
                    await operation.Perform(context);
                    result = context.Output;
                }

                // We store the output as a new result on the results dictionary.
                Results.TryAdd(operationId, result);
                return operationId;
            }

            private string[] GetOperationMultiplexedFields(string operationId)
            {
                return Workflow.Connections
                    .Where(connection => connection.Target.Operation == operationId && connection.Multiplexer)
                    .Select(connection => connection.Target.Field)
                    .ToArray();
            }

            /// <summary>
            /// Retrieves an operation corresponding to a certain operation identifier from the workflow.
            /// </summary>
            /// <param name="operationId">The unique identifier of the operation.</param>
            /// <returns>The operation.</returns>
            private Operation GetOperation(string operationId)
            {
                return System.Array.Find(Workflow.Operations, op => op.Identifier == operationId);
            }
            /// <summary>
            /// Gets the output for an operation specified by identifier.
            /// </summary>
            /// <param name="operationId">The unique identifier of the operation.</param>
            /// <returns>The input that the operation requires.</returns>
            private Dictionary<string, object> GetInput(string operationId)
            {
                // For each connection to the specified operation, we add an input value specified by the connection.
                Dictionary<string, object> input = new();
                foreach (WorkflowOperationConnection connection in Workflow.Connections)
                {
                    if (connection.Target.Operation == operationId)
                    {
                        // This performs the binding from output to input.
                        Dictionary<string, object> output = GetOutput(connection.Source.Operation);
                        input.Add(connection.Target.Field, output[connection.Source.Field]);
                    }
                }

                // TODO: Temporary.
                // Forward the authentication settings.
                if (Context.Input.TryGetValue("authentication", out object authentication))
                    input.Add("authentication", authentication);

                return input;
            }
            /// <summary>
            /// Gets the output for an operation specified by identifier.
            /// </summary>
            /// <param name="operationId">The unique identifier of the operation.</param>
            /// <returns>The output that the operation produced.</returns>
            private Dictionary<string, object> GetOutput(string operationId)
            {
                if (Results.TryGetValue(operationId, out Dictionary<string, object> output))
                    return output;
                else
                    throw new KeyNotFoundException($"Could not find output for operation '{operationId}'.");
            }
        }

        /// <summary>
        /// The sub-operations that are contained within this workflow operation.
        /// All operations specified within the workflow, regardless of how they are connected, will be executed.
        /// </summary>
        protected Operation[] Operations { get; init; }
        /// <summary>
        /// The connections that bind an output of an operation to an input of another operation.
        /// The connections determine the order and flow of calling operations.
        /// </summary>
        protected WorkflowOperationConnection[] Connections { get; init; }

        /// <summary>
        /// Constructs a new instance of the <see cref="WorkflowOperation" /> class with the specified operations and
        /// connections.
        /// </summary>
        /// <param name="operations">The operations contained within the workflow operation.</param>
        /// <param name="connections">The connections that bind inputs to outputs in the workflow operation.</param>
        public WorkflowOperation(Operation[] operations, WorkflowOperationConnection[] connections)
        {
            // Test for null arguments.
            if (operations is null)
                throw new ArgumentNullException(nameof(operations));
            if (connections is null)
                throw new ArgumentNullException(nameof(connections));

            // Set the values of the operations and connections.
            // Then, check for their validity.
            Operations = operations;
            Connections = connections;

            // Test for valid connections.
            if (!HasValidConnections())
            {
                throw new ArgumentException(
                    "Connections must only contain references to operations in the workflow.",
                    nameof(connections)
                );
            }
            // Test for cyclicity.
            if (!HasNoncyclicConnections())
            {
                throw new ArgumentException(
                    "Connections must be noncyclic in the workflow.",
                    nameof(connections)
                );
            }
        }
        public WorkflowOperation()
        {
            Operations = System.Array.Empty<Operation>();
            Connections = System.Array.Empty<WorkflowOperationConnection>();
        }

        /// <summary>
        /// Determines whether the specified connections reference only valid operations in the workflow.
        /// </summary>
        /// <returns><c>true</c> if all connections are valid; otherwise, <c>false</c>.</returns>
        private bool HasValidConnections()
        {
            // Check that each connection point has a corresponding operation.
            foreach (WorkflowOperationConnection connection in Connections)
            {
                if (!System.Array.Exists(Operations, op => op.Identifier == connection.Source.Operation))
                    return false;
                if (!System.Array.Exists(Operations, op => op.Identifier == connection.Target.Operation))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Determines whether the specified connections do not form any cycles in the workflow.
        /// </summary>
        /// <returns><c>true</c> if connections are noncyclic; otherwise, <c>false</c>.</returns>
        private bool HasNoncyclicConnections()
        {
            // Foreach operation, we check that its dependencies never form a cycle.
            foreach (Operation operation in Operations)
            {
                // Initialize the dependency list for an operation to contain only itself.
                // This kicks off the recursive checking for more dependencies.
                List<string> operationDependencyList = new() { operation.Identifier };

                // We loop through the dependency list iteratively finding all subdependencies.
                for (int k = 0; k < operationDependencyList.Count; k++)
                {
                    // Check for the current dependency, any subdependencies that are new or cyclic.
                    string currentDependency = operationDependencyList[k];
                    foreach (WorkflowOperationConnection connection in Connections)
                    {
                        if (connection.Target.Operation == currentDependency)
                        {
                            // We check for a cycle by seeing if any dependency eventually depends on the operation
                            // itself again.
                            if (connection.Source.Operation == operation.Identifier)
                                return false;
                            else if (!operationDependencyList.Contains(connection.Source.Operation))
                                operationDependencyList.Add(connection.Source.Operation);
                        }
                    }
                }
            }
            return true;
        }

        // TODO: Demonstrate an operation that exhibits a workflow structure but actually performs as an optimized
        //       simple operation.
        /// <inheritdoc />
        public override async Task Perform(OperationContext context)
        {
            // TODO: Use vectorized operations in multiplexing connections.
            /*
                The purpose of a workflow operation is to perform all of the sub-operations contained within in the
                appropriate order of execution. This is different from moving from input operations to output operations
                which was the previous experimental implementation of workflows. This is because some operations may
                only produce side-effects but are not relied upon earlier or later as an input/output such as an export
                operation.
                
                This aids in making operations work consistently (i.e. there are no special operations). For instance,
                instead of this workflow operation providing inputs to an `InputOperation` or extracting outputs from an
                `OutputOperation`, we simply allow those operations to defined logic that works with the operation
                context.

                Additionally, in the interest of completely separating operations (modularism) from each other,
                operating in a "when inputs are ready" fashion allows individual operations to not worry about grabbing
                the outputs from other operations. This helps us easily prevent duplicating calls to the same operation
                in the same run of a workflow.
            */

            // We actually execute the workflow using the runner.
            // Note, we separate the runner from the workflow because the workflow should only be in charge of
            // operations and connections. The running has the concern of the threads running and other execution
            // information. 
            WorkflowOperationRunner runner = new(this, context);
            await runner.Run();
        }

        /// <inheritdoc />
        public override Task PrePerform(OperationContext context)
        {
            // Since the preperform phase should not need inputs nor outputs, we simply run this phase on each
            // suboperation in no particular order.
            Task[] tasks = new Task[Operations.Length];
            for (int k = 0; k < Operations.Length; k++)
            {
                OperationContext childContext = new() { Parent = context };
                tasks[k] = Operations[k].PrePerform(childContext);
            }
            return Task.WhenAll(tasks);
        }
        /// <inheritdoc />
        public override Task PostPerform(OperationContext context)
        {
            // Since the postperform phase should not need inputs nor outputs, we simply run this phase on each
            // suboperation in no particular order.
            Task[] tasks = new Task[Operations.Length];
            for (int k = 0; k < Operations.Length; k++)
            {
                OperationContext childContext = new() { Parent = context };
                tasks[k] = Operations[k].PostPerform(childContext);
            }
            return Task.WhenAll(tasks);
        }

        // TODO: We need to resolve types based on the connections that are formed between operations.
        //       For instance, if a connection connects an output of operation A with field I to operation B with field II
        //       we do the following. If field I is assignable to field II but field II is not assignable to field I, then,
        //       the type of both field I and II should be the type of field II because it is more specific. Notice, that if
        //       neither field I is assignable to field II nor field II is assignable to field I, then there is a type error
        //       that is catchable at compile-time.

        // TODO: Instead of treating the `InputOperation` or `OutputOperation` specially, we should do a prepass with
        //       the context that helps to determine the structure of the workflow (exposes operations that needs inputs
        //       or produce outputs).
        // TODO: It wouldn't hurt to make some of these methods produce enumerables rather than arrays to give a small
        //       boost to efficiency.
        // TODO: We may want to optimize the usage of a workflow to compile it to a graph structure so it is easier to
        //       get the out- or in-edges for vertices (operations). This might also just be useful for abstraction
        //       purposes of the graph elsewhere in development.
        /// <inheritdoc />
        public override string[] GetInputFields()
        {
            List<string> inputs = new();
            foreach (Operation operation in Operations)
            {
                if (operation is InputOperation inputOperation &&
                    inputOperation.DefaultValues.ContainsKey("Name"))
                {
                    inputs.Add((string)inputOperation.DefaultValues["Name"]);
                }
            }
            return inputs.ToArray();
        }
        /// <inheritdoc />
        public override string[] GetOutputFields()
        {
            List<string> outputs = new();
            foreach (Operation operation in Operations)
            {
                if (operation is OutputOperation outputOperation &&
                    outputOperation.DefaultValues.ContainsKey("Name"))
                {
                    outputs.Add((string)outputOperation.DefaultValues["Name"]);
                }
            }
            return outputs.ToArray();
        }

        /// <inheritdoc />
        public override Type GetInputFieldType(string field)
        {
            foreach (Operation operation in Operations)
            {
                if (operation is InputOperation inputOperation &&
                    inputOperation.DefaultValues.ContainsKey("Name") &&
                    (string)inputOperation.DefaultValues["Name"] == field)
                {
                    foreach (WorkflowOperationConnection connection in Connections)
                    {
                        if (
                            connection.Source.Operation == inputOperation.Identifier &&
                            connection.Source.Field == "Value"
                        )
                        {
                            Operation target = Operations.First(operation => operation.Identifier == connection.Target.Operation);
                            Type targetType = target.GetInputFieldType(connection.Target.Field);
                            return targetType;
                        }
                    }
                }
            }
            // If there were no connections to the input object, we assume any type will do.
            return typeof(object);
        }
        /// <inheritdoc />
        public override Type GetOutputFieldType(string field)
        {
            foreach (Operation operation in Operations)
            {
                if (operation is OutputOperation outputOperation &&
                    outputOperation.DefaultValues.ContainsKey("Name") &&
                    (string)outputOperation.DefaultValues["Name"] == field)
                {
                    foreach (WorkflowOperationConnection connection in Connections)
                    {
                        if (
                            connection.Target.Operation == outputOperation.Identifier &&
                            connection.Source.Field == "Value"
                        )
                        {
                            Operation source = Operations.First(operation => operation.Identifier == connection.Source.Operation);
                            Type sourceType = source.GetOutputFieldType(connection.Source.Field);
                            return sourceType;
                        }
                    }
                }
            }
            // If there were no connections to the output object, we assume any type will do.
            return typeof(object);
        }

        public bool IsMultiplexLike(string operationId, WorkflowOperationConnection pending)
        {
            Operation sourceOperation = Operations.First(operation => operation.Identifier == pending.Source.Operation);
            Operation targetOperation = Operations.First(operation => operation.Identifier == pending.Target.Operation);

            Type outputFieldType = sourceOperation.GetOutputFieldType(pending.Source.Field);
            Type inputFieldType = targetOperation.GetInputFieldType(pending.Target.Field);

            // Determine if the source operation is multiplexed.
            bool sourceIsMultiplexed = false;
            foreach (WorkflowOperationConnection connection in Connections)
            {
                if (connection.Target.Operation == sourceOperation.Identifier && connection.Multiplexer)
                {
                    sourceIsMultiplexed = true;
                    break;
                }
            }

            // Adjust the source output type if the source is multiplexed.
            if (sourceIsMultiplexed)
                outputFieldType = outputFieldType.MakeArrayType();

            // TODO: Make this more concrete with some typing mechanism.
            if (inputFieldType.IsAssignableFrom(typeof(object)))
                return false;
            else
            {
                if (!outputFieldType.IsArray) return false;
                if (!inputFieldType.IsArray) return true;
                if (outputFieldType.GetArrayRank() < inputFieldType.GetArrayRank()) return true;
            }

            // foreach (WorkflowOperationConnection connection in Connections)
            // {
            //     if (connection.Target.Operation == operationId)
            //     {
            //         if (connection.Multiplexer) return true;
            //         if (connection.Property is not null) return true;

            //         // TODO: Abstract to multi-dimensional arrays.
            //         if (outputFieldType.IsArray && !inputFieldType.IsArray)
            //             return true;

            //         if (IsMultiplexLike(connection.Source.Operation, pending))
            //         {
            //             return true;
            //         }
            //     }
            // }
            return false;
        }
    }
}