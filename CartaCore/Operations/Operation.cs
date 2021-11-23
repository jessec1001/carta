using System.Threading.Tasks;

namespace CartaCore.Operations
{
    /// <summary>
    /// Represents an abstract base for an operation that takes an input mapping of named entries and produces a similar
    /// output mapping of named entries.
    /// </summary>
    public abstract class Operation
    {
        /// <summary>
        /// A unique identifier for this operation that should be used for specifying references to this operation.
        /// </summary>
        public string Identifier { get; init; }

        /// <summary>
        /// Operates on a specified operation context containing input and output mappings. Most operations will use the
        /// input mapping to produce an output mapping. This method is implemented by concrete subclasses.
        /// </summary>
        /// <param name="context">
        /// The context for the calling operation. Will be assigned to a value that provides access to inputs, outputs,
        /// defaults, and configurations for the current operation. When operation inside of a workflow, a parent
        /// context is also provided.
        /// </param>
        /// <returns>
        /// Nothing. The implementation is expected to set values on the input or output of the context.
        /// </returns>
        public abstract Task Perform(OperationContext context);
    
        /// <summary>
        /// Determines whether the operation is deterministic or non-deterministic on a specified context. This allows
        /// for operations to be memoized. By default, operations are assumed to be deterministic, and thus, memoized.
        /// </summary>
        /// <param name="context">
        /// The context for the calling operation. Will be assigned to a value that provides access to inputs, outputs,
        /// defaults, and configurations for the current operation. When operation inside of a workflow, a parent
        /// context is also provided.
        /// </param>
        /// <returns><c>true</c> if the operation is deterministic on a context; otherwise <c>false</c>.</returns>
        public virtual bool IsDeterministic(OperationContext context) => true;
    }
}