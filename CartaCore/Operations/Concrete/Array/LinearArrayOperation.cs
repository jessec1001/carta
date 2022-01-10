using System;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="LinearArrayOperation" /> operation.
    /// </summary>
    public struct LinearArrayOperationIn
    {
        /// <summary>
        /// The minimum of the range for generating values.
        /// </summary>
        public double Minimum { get; set; }
        /// <summary>
        /// Whether the minimum value in the range should be exclusive (not included).
        /// </summary>
        public bool ExclusiveMinimum { get; set; }
        /// <summary>
        /// The maximum of the range for generating values.
        /// </summary>
        public double Maximum { get; set; }
        /// <summary>
        /// Whether the maximum value in the range should be exclusive (not included).
        /// </summary>
        public bool ExclusiveMaximum { get; set; }
        /// <summary>
        /// The number of steps from the minimum to maximum to progress to generate the sequence.
        /// </summary>
        public int Steps { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="LinearArrayOperation" /> operation.
    /// </summary>
    public struct LinearArrayOperationOut
    {
        /// <summary>
        /// The linear sequence of values from the range.
        /// </summary>
        public double[] Values { get; set; }
    }

    /// <summary>
    /// Generates a sequence of values linearly from a range of numbers.
    /// </summary>
    [OperationName(Display = "Linear Array", Type = "linearArray")]
    [OperationTag(OperationTags.Arithmetic)]
    [OperationTag(OperationTags.Array)]
    public class LinearArrayOperation : TypedOperation
    <
        LinearArrayOperationIn,
        LinearArrayOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<LinearArrayOperationOut> Perform(LinearArrayOperationIn input)
        {
            // If the number of steps is negative, flip the range.
            if (input.Steps < 0)
            {
                input.Steps = -input.Steps;
                (input.Maximum, input.Minimum) = (input.Minimum, input.Maximum);
                (input.ExclusiveMaximum, input.ExclusiveMinimum) = (input.ExclusiveMinimum, input.ExclusiveMaximum);
            }

            // Generate the values by stepping upwards from minimum to maximum.
            // Notice that if the max and min are swapped, this algorithm still performs but in the opposite direction.
            double[] values = new double[input.Steps];
            int stepCount = (input.ExclusiveMinimum ? 1 : 0) + (input.ExclusiveMaximum ? 1 : 0) + input.Steps;
            double stepValue = (input.Maximum - input.Minimum) / (stepCount <= 1 ? 1 : stepCount - 1);
            double currentValue = input.Minimum + (input.ExclusiveMinimum ? 1 : 0) * stepValue;
            for (int k = 0; k < input.Steps; k++)
            {
                values[k] = currentValue;
                currentValue += stepValue;
            }
            return Task.FromResult(new LinearArrayOperationOut() { Values = values });
        }
    }
}