using System.Threading.Tasks;
using CartaCore.Utilities;

namespace CartaCore.Operations
{
    /// <summary>
    /// Represents an abstract operation that takes a particular typed input and produces a particular typed output.
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    public abstract class TypedOperation<TInput, TOutput> : Operation
    {
        /// <summary>
        /// Operates on a specified operation context containing input and output mappings. Most operations will use the
        /// input mapping to produce an output mapping. This method is implemented by concrete subclasses.
        /// </summary>
        /// <param name="input">The typed input to the operation.</param>
        /// <returns>The typed output from the operation.</returns>
        public virtual Task<TOutput> Perform(TInput input) => Task.FromResult(default(TOutput));
        /// <summary>
        /// Operates on a specified operation context containing input and output mappings. Most operations will use the
        /// input mapping to produce an output mapping. This method is implemented by concrete subclasses.
        /// </summary>
        /// <param name="input">The typed input to the operation.</param>
        /// <param name="callingContext">
        /// If the operation is being called from within a workflow, this is a reference to the workflow context.
        /// </param>
        /// <returns>The typed output from the operation.</returns>
        public virtual Task<TOutput> Perform(TInput input, OperationContext callingContext) => Perform(input);
        /// <inheritdoc />
        public override async Task Perform(OperationContext context)
        {
            TInput input = context.Input.AsTyped<TInput>();
            TOutput output = await Perform(input);
            context.Output = output.AsDictionary();
        }

        /// <summary>
        /// Determines whether the operation is deterministic or non-deterministic on a specified context. This allows
        /// for operations to be memoized. By default, operations are assumed to be deterministic, and thus, memoized.
        /// </summary>
        /// <param name="input">The typed input to the operation.</param>
        /// <returns><c>true</c> if the operation is deterministic on a context; otherwise <c>false</c>.</returns>
        public virtual bool IsDeterministic(TInput input) => true;
        /// <inheritdoc />
        public override bool IsDeterministic(OperationContext context)
        {
            TInput input = context.Input.AsTyped<TInput>();
            return IsDeterministic(input);
        }
    }
}