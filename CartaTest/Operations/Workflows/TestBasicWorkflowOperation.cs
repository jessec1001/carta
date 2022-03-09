using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Operations;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the basic functionality of the <see cref="WorkflowOperation" /> class.
    /// </summary>
    public class TestBasicWorkflowOperation
    {
        /// <summary>
        /// Tests the functionality of the <see cref="InputOperation{T}" /> and <see cref="OutputOperation{T}" />
        /// operation when they are simply connected together as the only operations in a workflow.
        /// </summary>
        [Test]
        public async Task TestBasicInputOutput()
        {
            // Create operations corresponding to the inputs and outputs of the workflow. In particular:
            // There is a single input "foo".
            // There is a single output "bar".
            InputOperation<int> opInput = new()
            {
                Id = "1",
                DefaultsTyped = new() { Name = "foo" }
            };
            OutputOperation<int> opOutput = new()
            {
                Id = "2",
                DefaultsTyped = new() { Name = "bar" }
            };

            // We generate the workflow operation itself.
            // We form a single connection from input to output so that "bar" is set to "foo".
            WorkflowOperationConnection connInputToOutput = new()
            {
                Source = new() { Operation = opInput.Id, Field = nameof(InputOperationOut<int>.Value) },
                Target = new() { Operation = opOutput.Id, Field = nameof(OutputOperationIn<int>.Value) },
            };
            WorkflowOperation opWorkflow = new(
                new Operation[] { opInput, opOutput },
                new WorkflowOperationConnection[] { connInputToOutput }
            );

            // This first test asserts that the value of "foo" is assigned to the value of "bar" which is 42.
            Dictionary<string, object> input = new()
            {
                ["foo"] = 42
            };
            Dictionary<string, object> output = new();
            OperationJob context = new(opWorkflow, null, input, output);
            await opWorkflow.Perform(context);

            Assert.Contains("bar", output.Keys);
            Assert.AreEqual(42, output["bar"]);
        }
    }
}