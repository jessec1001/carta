using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CartaCore.Extensions.Typing;
using CartaCore.Operations.Attributes;

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
        /// <param name="context">
        /// This is a reference to the operation context that contains the input and output mappings. Additionally,
        /// ff the operation is being called from within a workflow, this is a reference to the workflow context.
        /// </param>
        /// <returns>The typed output from the operation.</returns>
        public virtual Task<TOutput> Perform(TInput input, OperationContext context = null) => Perform(input);

        /// <inheritdoc />
        public override Task PrePerform(OperationContext context) => Task.CompletedTask;
        /// <inheritdoc />
        public override async Task Perform(OperationContext context)
        {
            TInput input = context.Input.AsTyped<TInput>(DefaultTypeConverter);
            TOutput output = await Perform(input, context);
            context.Output = output.AsDictionary(DefaultTypeConverter);
        }
        /// <inheritdoc />
        public override Task PostPerform(OperationContext context) => Task.CompletedTask;

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
            TInput input = context.Input.AsTyped<TInput>(DefaultTypeConverter);
            return IsDeterministic(input);
        }

        /// <summary>
        /// The typed default values of the operation as specified externally.
        /// </summary>
        public TInput DefaultValuesTyped
        {
            get => DefaultValues.AsTyped<TInput>(DefaultTypeConverter);
            set => DefaultValues = value.AsDictionary();
        }

        /// <summary>
        /// The typed initial values of the operation.
        /// </summary>
        public virtual TInput InitialValuesTyped => default;
        /// <inheritdoc />
        public override Dictionary<string, object> InitialValues => InitialValuesTyped.AsDictionary();

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

        /// <summary>
        /// Gets the names of the dynamic input fields corresponding to particular input field.
        /// </summary>
        /// <param name="field">The input field to get the dynamic fields of.</param>
        /// <param name="input">The input being passed to the operation.</param>
        /// <returns>The dynamic input fields corresponding to a field.</returns>
        public virtual string[] GetDynamicInputFields(string field, TInput input) => Array.Empty<string>();
        /// <summary>
        /// Gets the names of the dynamic output fields corresponding to particular output field.
        /// </summary>
        /// <param name="field">The output field to get the dynamic fields of.</param>
        /// <param name="input">The input being passed to the operation.</param>
        /// <returns>The dynamic output fields corresponding to a field.</returns>
        public virtual string[] GetDynamicOutputFields(string field, TInput input) => Array.Empty<string>();

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