using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The type of comparison operation that may be performed in the <see cref="ComparisonOperation" /> operation.
    /// </summary>
    public enum ComparisonOperationType
    {
        /// <summary>
        /// The numeric less than comparison operation.
        /// </summary>
        LessThan,
        /// <summary>
        /// The numeric less than or equal to comparison operation.
        /// </summary>
        LessThanOrEqualTo,
        /// <summary>
        /// The numeric greater than comparison operation.
        /// </summary>
        GreaterThan,
        /// <summary>
        /// The numeric greater than or equal to comparison operation.
        /// </summary>
        GreaterThanOrEqualTo,
        /// <summary>
        /// The numeric equal to comparison operation.
        /// </summary>
        EqualTo,
        /// <summary>
        /// The numeric not equal to comparison operation.
        /// </summary>
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
        public ComparisonOperationType Type { get; set; }

        /// <summary>
        /// The first operand of the comparison expression.
        /// </summary>
        public double Input1 { get; set; }
        /// <summary>
        /// The second operand of the comparison expression.
        /// </summary>
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