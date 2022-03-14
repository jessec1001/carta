using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;
using Newtonsoft.Json.Converters;

namespace CartaCore.Operations.Arithmetic
{
    /// <summary>
    /// The type of arithmetic operation that may be performed in the <see cref="ArithmeticOperation" /> operation.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ArithmeticOperationType
    {
        /// <summary>
        /// Add.
        /// </summary>
        [EnumMember(Value = "add")]
        Add,
        /// <summary>
        /// Substract.
        /// </summary>
        [EnumMember(Value = "sub")]
        Subtract,
        /// <summary>
        /// Multiply.
        /// </summary>
        [EnumMember(Value = "mul")]
        Multiply,
        /// <summary>
        /// Divide.
        /// </summary>
        [EnumMember(Value = "div")]
        Divide,
        /// <summary>
        /// Exponentiate.
        /// </summary>
        [EnumMember(Value = "exp")]
        Exponentiate
    }

    /// <summary>
    /// The input for the <see cref="ArithmeticOperation" /> operation.
    /// </summary>
    public struct ArithmeticOperationIn
    {
        /// <summary>
        /// The type of arithmetic operation to perform.
        /// </summary>
        [FieldName("Operation")]
        public ArithmeticOperationType Type { get; set; }

        /// <summary>
        /// The first operand of the arithmetic expression.
        /// </summary>
        [FieldName("Input 1")]
        public double Input1 { get; set; }
        /// <summary>
        /// The second operand of the arithmetic expression.
        /// </summary>
        [FieldName("Input 2")]
        public double Input2 { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ArithmeticOperation" /> operation.
    /// </summary>
    public struct ArithmeticOperationOut
    {
        /// <summary>
        /// The output of the arithmetic expression.
        /// </summary>
        [FieldName("Output")]
        public double Output { get; set; }
    }

    /// <summary>
    /// Computes a simple arithmetic expression with a binary operation operating on two operands.
    /// </summary>
    [OperationName(Display = "Arithmetic", Type = "arithmetic")]
    [OperationTag(OperationTags.Arithmetic)]
    public class ArithmeticOperation : TypedOperation
    <
        ArithmeticOperationIn,
        ArithmeticOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<ArithmeticOperationOut> Perform(ArithmeticOperationIn input)
        {
            // We get the result by evaluating each possible arithmetic operation type.
            double result = input.Type switch
            {
                ArithmeticOperationType.Add => input.Input1 + input.Input2,
                ArithmeticOperationType.Subtract => input.Input1 - input.Input2,
                ArithmeticOperationType.Multiply => input.Input1 * input.Input2,
                ArithmeticOperationType.Divide => input.Input1 / input.Input2,
                ArithmeticOperationType.Exponentiate => Math.Pow(input.Input1, input.Input2),
                _ => 0.0,
            };
            return Task.FromResult(new ArithmeticOperationOut() { Output = result });
        }
    }
}