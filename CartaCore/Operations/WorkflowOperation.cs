using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    // TODO: We need to tee streams if we want to support multiple outputs.
    // TODO: Implement pipelining.

    /// <summary>
    /// Represents an operation that executes as a topologically-sorted graph of sub-operations. Connections determine
    /// how outputs from some operations are bound to inputs for other operations.
    /// </summary>
    [OperationTag(OperationTags.Workflow)]
    [OperationName(Type = "workflow", Display = "Workflow")]
    public class WorkflowOperation : Operation
    {
        /// <summary>
        /// The sub-operations that are contained within this workflow operation.
        /// All operations specified within the workflow, regardless of how they are connected, will be executed.
        /// </summary>
        public Operation[] Operations { get; init; }
        /// <summary>
        /// The connections that bind an output of an operation to an input of another operation.
        /// The connections determine the order and flow of calling operations.
        /// </summary>
        public WorkflowOperationConnection[] Connections { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowOperation"/> class with the specified operations and
        /// connections.
        /// </summary>
        /// <param name="operations">The operations to execute.</param>
        /// <param name="connections">The connections to dictate flow.</param>
        public WorkflowOperation(
            IEnumerable<Operation> operations,
            IEnumerable<WorkflowOperationConnection> connections)
        {
            // Test for null arguments.
            if (operations is null)
                throw new ArgumentNullException(nameof(operations));
            if (connections is null)
                throw new ArgumentNullException(nameof(connections));

            // Set the operations and connections.
            Operations = operations.ToArray();
            Connections = connections.ToArray();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowOperation"/> class with no operations or connections.
        /// </summary>
        public WorkflowOperation()
            : this(Array.Empty<Operation>(), Array.Empty<WorkflowOperationConnection>()) { }

        /// <inheritdoc />
        public override async Task Perform(OperationContext context)
        {
            // To perform the actual execution of a workflow, we delegate this functionality to a workflow runner.
            WorkflowRunner runner = new(this, context);

            // We need to verify the structure of the workflow before trying to execute it. This will capture any
            // anomolies such as invalid or missing connections, wrongly specified input or outputs, etc.
            try
            {
                await runner.VerifyStructure();
            }
            catch (ArgumentException argumentEx)
            {
                // TODO: Handle this exception which should only be thrown if the workflow is malformed.
            }

            // Note, we separate the runner from the workflow because the workflow should only be in charge of
            // operations and connections. The running has the concern of the threads running and other execution
            // information.
            try
            {
                await runner.RunWorkflow();
            }
            catch (Exception unknownEx)
            {
                // TODO: Handle this exception which should never be thrown.
            }
        }

        /// <inheritdoc />
        public override bool IsDeterministic(OperationContext context)
        {
            // The workflow is deterministic if all of its operations are deterministic.
            return Operations.All(operation => operation.IsDeterministic(context));
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
        public override async Task<string[]> GetInputFields(OperationContext context)
        {
            // TODO: Completely rewrite.
            List<string> inputs = new();
            foreach (Operation operation in Operations)
            {
                if (operation is InputOperation inputOperation &&
                    inputOperation.DefaultValues.ContainsKey("Name"))
                {
                    string name = inputOperation.DefaultValues["Name"] as string;
                    if (name is not null) inputs.Add(name);
                }
            }
            return inputs.ToArray();
        }
        /// <inheritdoc />
        public override async Task<string[]> GetOutputFields(OperationContext context)
        {
            // TODO: Completely rewrite.
            List<string> outputs = new();
            foreach (Operation operation in Operations)
            {
                if (operation is OutputOperation outputOperation &&
                    outputOperation.DefaultValues.ContainsKey("Name"))
                {
                    string name = outputOperation.DefaultValues["Name"] as string;
                    if (name is not null) outputs.Add(name);
                }
            }
            return outputs.ToArray();
        }

        /// <inheritdoc />
        public override Type GetInputFieldType(string field)
        {
            // TODO: Completely rewrite.
            foreach (Operation operation in Operations)
            {
                if (operation is InputOperation inputOperation &&
                    inputOperation.DefaultValues.ContainsKey("Name") &&
                    (string)inputOperation.DefaultValues["Name"] == field)
                {
                    foreach (WorkflowOperationConnection connection in Connections)
                    {
                        if (
                            connection.Source.Operation == inputOperation.Id &&
                            connection.Source.Field == "Value"
                        )
                        {
                            Operation target = Operations.First(operation => operation.Id == connection.Target.Operation);
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
            // TODO: Completely rewrite.
            foreach (Operation operation in Operations)
            {
                if (operation is OutputOperation outputOperation &&
                    outputOperation.DefaultValues.ContainsKey("Name") &&
                    (string)outputOperation.DefaultValues["Name"] == field)
                {
                    foreach (WorkflowOperationConnection connection in Connections)
                    {
                        if (
                            connection.Target.Operation == outputOperation.Id &&
                            connection.Target.Field == "Value"
                        )
                        {
                            Operation source = Operations.First(operation => operation.Id == connection.Source.Operation);
                            Type sourceType = source.GetOutputFieldType(connection.Source.Field);
                            return sourceType;
                        }
                    }
                }
            }
            // If there were no connections to the output object, we assume any type will do.
            return typeof(object);
        }

        // TODO: Provide custom implementations for the following methods.
        // public override JsonSchema GetInputFieldSchema(string field)
        // {
        //     return base.GetInputFieldSchema(field);
        // }
        // public override JsonSchema GetOutputFieldSchema(string field)
        // {
        //     return base.GetOutputFieldSchema(field);
        // }
    }

    /// <summary>
    /// Represents a vertex in a workflow dependency graph.
    /// A single vertex is constructed for each sub-operation within a workflow.
    /// </summary>
    public class WorkflowDependencyVertex : IVertex<WorkflowDependencyEdge>
    {
        /// <inheritdoc />
        public string Id => Operation.Id;

        /// <inheritdoc />
        public IEnumerable<WorkflowDependencyEdge> Edges { get; set; }

        public Operation Operation { get; init; }
        public bool Started { get; set; }
        public bool Finished { get; set; }

        public WorkflowDependencyVertex(Operation operation)
        {
            Operation = operation;
        }
    }
    /// <summary>
    /// Represents an edge in a workflow dependency graph.
    /// Since a single vertex is constructed for each sub-operation within a workflow, there may be multiple edges
    /// between a pair of vertices if multiple fields are connected between the vertices.
    /// </summary>
    public class WorkflowDependencyEdge : IEdge
    {
        /// <inheritdoc />
        public string Id => $"{Source}::{Target}";
        /// <inheritdoc />
        public bool Directed => true;

        /// <inheritdoc />
        public string Source => SourcePoint.Operation;
        /// <inheritdoc />
        public string Target => TargetPoint.Operation;

        public WorkflowOperationConnectionPoint SourcePoint { get; init; }
        public WorkflowOperationConnectionPoint TargetPoint { get; init; }

        public WorkflowDependencyEdge(WorkflowOperationConnection connection)
        {
            SourcePoint = connection.Source;
            TargetPoint = connection.Target;
        }
    }
    /// <summary>
    /// Provides the mechanics for cleanly and optimally executing a workflow operation.
    /// </summary>
    public class WorkflowRunner
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
        /// The graph 
        /// </summary>
        /// <value></value>
        private FiniteGraph<WorkflowDependencyVertex, WorkflowDependencyEdge> DependencyGraph { get; init; }

        /// <summary>
        /// Stores the results of executed suboperations.
        /// </summary>
        private readonly ConcurrentDictionary<string, Dictionary<string, object>> Results = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowRunner"/> class.
        /// </summary>
        /// <param name="workflow">The workflow operation.</param>
        /// <param name="context">The operation context.</param>
        public WorkflowRunner(WorkflowOperation workflow, OperationContext context)
        {
            Workflow = workflow;
            Context = context;

            DependencyGraph = ConstructDependencyGraph();
        }

        private FiniteGraph<WorkflowDependencyVertex, WorkflowDependencyEdge> ConstructDependencyGraph()
        {
            FiniteGraph<WorkflowDependencyVertex, WorkflowDependencyEdge> dependencies = new(nameof(WorkflowOperation));
            foreach (Operation operation in Workflow.Operations)
                dependencies.AddVertex(new WorkflowDependencyVertex(operation));
            foreach (WorkflowOperationConnection connection in Workflow.Connections)
                dependencies.AddEdge(new WorkflowDependencyEdge(connection));
            return dependencies;
        }

        /// <summary>
        /// Verifies that the structure of the workflow is not malformed.
        /// If the structure is malformed, a corresponding <see cref="ArgumentException"/> will be thrown.
        /// </summary>
        /// <returns>Nothing.</returns>
        public async Task VerifyStructure()
        {
            if (!await VerifyValidConnections())
                throw new ArgumentException("Connections must only contain references to operations in the workflow.");
            if (!await VerifyNoncyclicConnections())
                throw new ArgumentException("Connections must be noncyclic in the workflow.");
        }
        /// <summary>
        /// Determines whether the specified connections reference only valid operations in the workflow.
        /// </summary>
        /// <returns><c>true</c> if all connections are valid; otherwise, <c>false</c>.</returns>
        public async Task<bool> VerifyValidConnections()
        {
            // Check that each connection point has a corresponding operation with a corresponding field.
            await foreach (WorkflowDependencyEdge edge in DependencyGraph.GetEdges())
            {
                // Check that the source and target operations are valid.
                WorkflowDependencyVertex source = await DependencyGraph.GetVertex(edge.Source);
                WorkflowDependencyVertex target = await DependencyGraph.GetVertex(edge.Target);
                if (source is null || target is null) return false;

                // Check that the source and target operations have the specified fields.
                string[] sourceFields = await source.Operation.GetOutputFields(Context);
                string[] targetFields = await target.Operation.GetInputFields(Context);
                if (!sourceFields.Contains(edge.SourcePoint.Field)) return false;
                if (!targetFields.Contains(edge.TargetPoint.Field)) return false;
            }
            return true;
        }
        /// <summary>
        /// Determines whether the specified connections do not form any cycles in the workflow.
        /// </summary>
        /// <returns><c>true</c> if connections are noncyclic; otherwise, <c>false</c>.</returns>
        public async Task<bool> VerifyNoncyclicConnections()
        {
            // Foreach operation, we check that its dependencies never form a cycle.
            HashSet<string> verifiedDependencies = new(capacity: Workflow.Operations.Length);
            await foreach (WorkflowDependencyVertex vertex in DependencyGraph.GetVertices())
            {
                // We do not need to check already verified dependencies.
                if (verifiedDependencies.Contains(vertex.Id)) continue;

                // Initialize the dependency list for an operation to contain only itself.
                // This kicks off the recursive checking for more dependencies.
                List<string> directDependencies = new() { vertex.Id };

                // We loop through the dependency list iteratively finding all subdependencies.
                for (int k = 0; k < directDependencies.Count; k++)
                {
                    // Check for the current dependency, any subdependencies that are new or cyclic.
                    WorkflowDependencyVertex dependency = await DependencyGraph.GetVertex(directDependencies[k]);
                    foreach (WorkflowDependencyEdge dependencyEdge in dependency.Edges)
                    {
                        // Check if the dependency edge is targeting the current operation.
                        if (dependencyEdge.Target != vertex.Id) continue;

                        // Check if the dependency edge is sourced from the verified list.
                        if (verifiedDependencies.Contains(dependencyEdge.Source)) continue;

                        // Check if the dependency edge is sourced from the dependency list.
                        if (directDependencies.Contains(dependencyEdge.Source)) return false;
                        else directDependencies.Add(dependencyEdge.Source);
                    }
                }

                // Add all of the dependencies to the verified list.
                foreach (string dependency in directDependencies)
                    verifiedDependencies.Add(dependency);
            }
            return true;
        }

        /// <summary>
        /// Runs the workflow operation by executing all of its sub-operations in order.
        /// </summary>
        /// <returns>Nothing but may modify the operation context.</returns>
        public async Task RunWorkflow()
        {
            /*
                The purpose of a workflow operation is to perform all of the sub-operations contained within in the
                appropriate order of execution. This is different from moving from input operations to output operations
                which was the previous experimental implementation of workflows. This is because some operations may
                only produce side-effects but are not relied upon earlier or later as an input/output such as an export
                operation.

                This aids in making operations work consistently (i.e. there are no special operations). For instance,
                instead of this workflow operation providing inputs to an `InputOperation` or extracting outputs from an
                `OutputOperation`, we simply allow those operations to define logic that works with the operation
                context.

                Additionally, in the interest of completely separating operations (modularism) from each other,
                operating in a "when inputs are ready" fashion allows individual operations to not worry about grabbing
                the outputs from other operations. This helps us easily prevent duplicating calls to the same operation
                in the same run of a workflow.
            */

            // TODO: Integrate the thread limit into operation execution.

            // TODO: Completely rewrite this.
            // Keep a list of currently running operation tasks.
            // Keep executing operations while there are tasks in this list.
            List<Task<string>> running = new();
            List<string> runningIds = new();
            do
            {
                // Find all new operations that can start running.
                foreach (Operation operation in Workflow.Operations)
                {
                    if (!Results.ContainsKey(operation.Id) && CanResolveOperation(operation.Id))
                    {
                        running.Add(RunOperation(operation.Id));
                        runningIds.Add(operation.Id);
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
        }

        // TODO: Completely refactor this.
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

        public async Task<string> RunOperation(string id)
        {
            /*
                In the most general infrastructure of our platform, each operation may be running completely
                independently of any other operation inside of a workflow. Thus, we resolve each single operation as an
                atomic unit of computation. We can quite easily extend this to using multiple independent computation
                servers by distributing certain operations to run on certain servers. Input and output data can then be
                streamed to and from those servers with identifiers indicating the job, operation, and field identifiers
                so that the same internal algorithms here can be implemented cross-server.
            */

            // Get the specified operation.
            WorkflowDependencyVertex vertex = await DependencyGraph.GetVertex(id);
            Operation operation = vertex.Operation;

            // TODO: For now we ignore multiplexing and indexing.
            // TODO: Implement basic execution.
            // TODO: Implement pipelining execution.

            // Create a context for the operation.
            Dictionary<string, object> inputs = await GetOperationInputs(id);
            Dictionary<string, object> outputs = new();
            OperationContext context = new(inputs, outputs)
            {
                Parent = Context,
                Operation = operation,
            };

            // TODO: Can we delete intermediate results to operations that are no longer dependencies?
            // Execute the operation and add the results to our collection.
            await operation.Perform(context);
            Results.TryAdd(id, context.Output);

            return id;
        }

         /// <summary>
        /// Gets the input for an operation specified by identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the operation.</param>
        /// <returns>The input that the operation requires.</returns>
        private async Task<Dictionary<string, object>> GetOperationInputs(string id)
        {
            // TODO: Clean this up and check if there are any ramifications to the way we are doing this.

            // For each dependency to the specified operation, we get the input for that dependency.
            Dictionary<string, object> inputs = new();
            WorkflowDependencyVertex vertex = await DependencyGraph.GetVertex(id);
            foreach (WorkflowDependencyEdge edge in vertex.Edges)
            {
                // TODO: Check that the corresponding fields actually exist for type safety.
                // Check if the edge targets the specified operation.
                if (edge.Target != id) continue;
                Dictionary<string, object> output = await GetOperationOutputs(edge.Source);
                inputs.Add(edge.SourcePoint.Field, output[edge.TargetPoint.Field]);
            }

            // TODO: Temporary.
            // Forward the authentication settings.
            if (Context.Input.TryGetValue("authentication", out object authentication))
                inputs.Add("authentication", authentication);

            return inputs;
        }
        /// <summary>
        /// Gets the output for an operation specified by identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the operation.</param>
        /// <returns>The output that the operation produced.</returns>
        private async Task<Dictionary<string, object>> GetOperationOutputs(string id)
        {
            if (Results.TryGetValue(id, out Dictionary<string, object> output))
                return await Task.FromResult(output);
            else throw new KeyNotFoundException($"Could not find output for operation with ID: {id}.");
        }
    }
}