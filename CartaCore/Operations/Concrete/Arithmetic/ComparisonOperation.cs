using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;
using CartaCore.Serialization.Json;

namespace CartaCore.Operations.Arithmetic
{
    /// <summary>
    /// The type of comparison operation that may be performed in the <see cref="ComparisonOperation" /> operation.
    /// </summary>
    [JsonConverter(typeof(JsonFullStringEnumConverter))]
    public enum ComparisonOperationType
    {
        /// <summary>
        /// The numeric less than comparison operation.
        /// </summary>
        [EnumMember(Value = "Less than")]
        LessThan,
        /// <summary>
        /// The numeric less than or equal to comparison operation.
        /// </summary>
        [EnumMember(Value = "Less than or equal to")]
        LessThanOrEqualTo,
        /// <summary>
        /// The numeric greater than comparison operation.
        /// </summary>
        [EnumMember(Value = "Greater than")]
        GreaterThan,
        /// <summary>
        /// The numeric greater than or equal to comparison operation.
        /// </summary>
        [EnumMember(Value = "Greater than or equal to")]
        GreaterThanOrEqualTo,
        /// <summary>
        /// The numeric equal to comparison operation.
        /// </summary>
        [EnumMember(Value = "Equal to")]
        EqualTo,
        /// <summary>
        /// The numeric not equal to comparison operation.
        /// </summary>
        [EnumMember(Value = "Not equal to")]
        NotEqualTo
    }

    /// <summary>
    /// The input for the <see cref="ComparisonOperation" /> operation.
    /// </summary>
    public struct ComparisonOperationIn
    {
        /// <summary>
        /// The type of comparison to perform.
        /// </summary>
        [FieldName("Comparison")]
        public ComparisonOperationType Type { get; set; }

        /// <summary>
        /// The first operand of the comparison expression.
        /// </summary>
        [FieldName("Input 1")]
        public double Input1 { get; set; }
        /// <summary>
        /// The second operand of the comparison expression.
        /// </summary>
        [FieldName("Input 2")]
        public double Input2 { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ComparisonOperation" /> operation.
    /// </summary>
    public struct ComparisonOperationOut
    {
        /// <summary>
        /// The output of the comparison expression.
        /// </summary>
        [FieldName("Output")]
        public bool Output { get; set; }
    }

    /// <summary>
    /// Computes a simple comparison expression with a binary operator on two operands.
    /// </summary>
    [OperationName(Display = "Comparison", Type = "comparison")]
    [OperationTag(OperationTags.Arithmetic)]
    public class ComparisonOperation : TypedOperation
    <
        ComparisonOperationIn,
        ComparisonOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<ComparisonOperationOut> Perform(ComparisonOperationIn input)
        {
            // We get the result by evaluating each possible comparison operation type.
            bool result = input.Type switch
            {
                ComparisonOperationType.LessThan => input.Input1 < input.Input2,
                ComparisonOperationType.LessThanOrEqualTo => input.Input1 <= input.Input2,
                ComparisonOperationType.GreaterThan => input.Input1 > input.Input2,
                ComparisonOperationType.GreaterThanOrEqualTo => input.Input1 >= input.Input2,
                ComparisonOperationType.EqualTo => input.Input1 == input.Input2,
                ComparisonOperationType.NotEqualTo => input.Input1 != input.Input2,
                _ => false
            };
            return Task.FromResult(new ComparisonOperationOut { Output = result });
        }
    }
}