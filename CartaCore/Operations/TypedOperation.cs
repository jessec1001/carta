using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;
using CartaCore.Typing.Conversion;
using CartaCore.Utilities;

namespace CartaCore.Operations
{
    /// <summary>
    /// Represents an abstract operation that takes a particular typed input and produces a particular typed output.
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    public abstract class TypedOperation<TInput, TOutput> : Operation
        where TInput : new()
        where TOutput : new()
    {
        // TODO: We need to reconfigure these methods to not use the parent context automatically.
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
            TInput input = context.Input.AsTyped<TInput>(TypeConverterContext.Default);
            TOutput output = await Perform(input, context.Parent);
            context.Output = output.AsDictionary(TypeConverterContext.Default);
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
            TInput input = context.Input.AsTyped<TInput>(TypeConverterContext.Default);
            return IsDeterministic(input);
        }

        /// <summary>
        /// The typed default values of the operation as specified externally.
        /// </summary>
        public TInput DefaultTyped
        {
            get => Default.AsTyped<TInput>(TypeConverterContext.Default);
            set => Default = value.AsDictionary();
        }

        // TODO: We need to incorporate the attributes into the following methods.  
        /// <inheritdoc />
        public override string[] GetInputFields()
        {
            return typeof(TInput)
                .GetProperties(
                    BindingFlags.IgnoreCase |
                    BindingFlags.Public |
                    BindingFlags.Instance
                )
                .Select(property => property.Name)
                .ToArray();
        }
        /// <inheritdoc />
        public override string[] GetOutputFields()
        {
            return typeof(TOutput)
                .GetProperties(
                    BindingFlags.IgnoreCase |
                    BindingFlags.Public |
                    BindingFlags.Instance
                )
                .Select(property => property.Name)
                .ToArray();
        }

        // TODO: Refactor
        /*
            We need more information that simply the type or types that a value can be set to.
            In some circumstances, we need some special functionality defined in terms of attributes.
            We need to transform this additional functionality into a generalizeable structure that
            may be passed around easily.
        */
        /// <inheritdoc />
        public override Type GetInputFieldType(string field)
        {
            return typeof(TInput).GetProperty(field,
                BindingFlags.IgnoreCase |
                BindingFlags.Public |
                BindingFlags.Instance).PropertyType;
        }
        /// <inheritdoc />
        public override Type GetOutputFieldType(string field)
        {
            return typeof(TOutput).GetProperty(field,
                BindingFlags.IgnoreCase |
                BindingFlags.Public |
                BindingFlags.Instance).PropertyType;
        }

        public OperationTemplate GetTemplate(TInput defaults)
        {
            // Get the operation name attribute assosicated with this operation.
            OperationNameAttribute attr = GetType().GetCustomAttribute<OperationNameAttribute>();
            if (attr is null) return null;
            else return new OperationTemplate()
            {
                Type = attr.Type,
                Subtype = null,
                Default = defaults.AsDictionary()
            };
        }
    }
}