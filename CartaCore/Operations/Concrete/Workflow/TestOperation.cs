using System;
using System.Threading.Tasks;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="TestOperation"/> operation.
    /// </summary>
    public struct TestOperationIn
    {
        /// <summary>
        /// The condition to test.
        /// </summary>
        public bool Condition { get; set; }
        /// <summary>
        /// The message to display if the condition is not satisfied.
        /// </summary>
        public string Message { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="TestOperation"/> operation.
    /// </summary>
    public struct TestOperationOut { }

    /// <summary>
    /// Tests that a condition is satisfied and throws an error if it is not.
    /// </summary>
    public class TestOperation : TypedOperation
    <
        TestOperationIn,
        TestOperationOut
    >
    {
        /// <inheritdoc />
        public override Task<TestOperationOut> Perform(TestOperationIn input)
        {
            if (input.Condition) return Task.FromResult(new TestOperationOut());
            else throw new Exception(input.Message);
        }
    }
}