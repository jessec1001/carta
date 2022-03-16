using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Typing;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// A base interface for attributes that override the default conversion of a particular field.
    /// </summary>
    public interface IOverrideConversionAttribute
    {
        /// <summary>
        /// Converts a particular input object into the expected type for the corresponding input field.
        /// The only input fields that are considered are those that are returned from <see cref="GetInputFields(OperationJob)"/>.
        /// </summary>
        /// <param name="operation">The executing operation.</param>
        /// <param name="field">The description of the field.</param>
        /// <param name="input">The input object.</param>
        /// <param name="job">The job performing the operation.</param>
        /// <returns>The converted field value.</returns>
        Task<object> ConvertInputField(
            Operation operation,
            OperationFieldDescriptor field,
            object input,
            OperationJob job);
        /// <summary>
        /// Converts a particular output object into the expected type for the corresponding output field.
        /// The only output fields that are considered are those that are returned from <see cref="GetOutputFields(OperationJob)"/>.
        /// </summary>
        /// <param name="operation">The executing operation.</param>
        /// <param name="field">The description of the field.</param>
        /// <param name="output">The output object.</param>
        /// <param name="job">The job performing the operation.</param>
        /// <returns>The converted field value.</returns>
        Task<object> ConvertOutputField(
            Operation operation,
            OperationFieldDescriptor field,
            object output,
            OperationJob job);
    }
}