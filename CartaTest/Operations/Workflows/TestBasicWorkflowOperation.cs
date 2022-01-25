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
        /// Tests the functionality of the <see cref="InputOperation" /> and <see cref="OutputOperation" /> operation
        /// when they are simply connected together as the only operations in a workflow.
        /// </summary>
        [Test]
        public async Task TestBasicInputOutput()
        {
            // Create operations corresponding to the inputs and outputs of the workflow. In particular:
            // There is a single input "foo".
            // There is a single output "bar".
            InputOperation opInput = new()
            {
                Identifier = "1",
                DefaultValuesTyped = new() { Name = "foo" }
            };
            OutputOperation opOutput = new()
            {
                Identifier = "2",
                DefaultValuesTyped = new() { Name = "bar" }
            };

            // We generate the workflow operation itself.
            // We form a single connection from input to output so that "bar" is set to "foo".
            WorkflowOperationConnection connInputToOutput = new()
            {
                Source = new() { Operation = opInput.Identifier, Field = nameof(InputOperationOut.Value) },
                Target = new() { Operation = opOutput.Identifier, Field = nameof(OutputOperationIn.Value) },
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
            OperationContext context = new()
            {
                Input = input,
                Output = output
            };
            await opWorkflow.Perform(context);

            Assert.Contains("bar", output.Keys);
            Assert.AreEqual(42, output["bar"]);
        }
    }
}