using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Operations;
using NUnit.Framework;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests a workflow that computes some statistics of a normal distribution.
    /// </summary>
    public class TestNormalDistributionStatisticsWorkflow
    {
        /// <summary>
        /// Tests that the normal distribution is represented and sampled correctly using a workflow.
        /// The samples from the normal distribution in this test are unmodified.
        /// </summary>
        [Test]
        public async Task TestUnmodifiedDistribution()
        {
            // The following workflow (diagram) is generated by this test code.
            /*
                Input (Seed) --------+-> Random Normal(Mean=1.0,Deviation=2.0) [...A]
                Input (SampleCount) -/

                [A...] -+-> Mean ------> Output (Mu)
                        \-> Deviation -> Output (Sigma)
            */
            // TODO: Add telemetry back in.
            // TODO: Add a string representation of operations and workflow operations.
            // Equivalently, this workflow could be represented more standardly as:
            /*
                Workflow
                ++++++++++++++++++++++++++++++++++++++++++++++++++
                Operation #1 (Input)
                ["Seed" ==========> Name                         ]
                [                               Value --------- A]
                
                Operation #2 (Input)
                ["SampleCount" ===> Name                         ]
                [                               Value --------- B]

                Operation #3 (Sample Normal)
                [A ---------------> Seed                         ]
                [B ---------------> SampleCount                  ]
                [1.0 =============> Mean                         ]
                [2.0 =============> Deviation                    ]
                [                               Samples ------- C]

                Operation #4 (Statistical Mean)
                [C ---------------> Values                       ]
                [                               Mean ---------- D]

                Operation #5 (Statistical Deviation)
                [C ---------------> Values                       ]
                [                               Deviation ----- E]

                Operation #6 (Output)
                [D ---------------> Value                        ]
                ["Mu" ============> Name                         ]

                Operation #7 (Output)
                [E ---------------> Value                        ]
                ["Sigma" =========> Name                         ]
                ++++++++++++++++++++++++++++++++++++++++++++++++++
            */

            // Define the operations required.
            #region Operations
            InputOperation opInputSeed = new()
            {
                DefaultValuesTyped = new() { Name = "Seed" }
            };
            InputOperation opInputSampleCount = new()
            {
                DefaultValuesTyped = new() { Name = "SampleCount" }
            };

            SampleNormalOperation opSampleNormal = new()
            {
                DefaultValuesTyped = new() { Mean = 1.0, Deviation = 2.0 }
            };

            MeanOperation opMean = new();
            DeviationOperation opDeviation = new();

            OutputOperation opOutputMu = new()
            {
                DefaultValuesTyped = new() { Name = "Mu" }
            };
            OutputOperation opOutputSigma = new()
            {
                DefaultValuesTyped = new() { Name = "Sigma" }
            };
            #endregion

            // Define the connections required.
            #region Connections
            WorkflowOperationConnection connSeedToRandom = new()
            {
                Source = new WorkflowOperationConnectionPoint()
                {
                    Operation = opInputSeed.Identifier,
                    Field = nameof(InputOperationOut.Value)
                },
                Target = new WorkflowOperationConnectionPoint()
                {
                    Operation = opSampleNormal.Identifier,
                    Field = nameof(SampleNormalOperationIn.Seed)
                }
            };
            WorkflowOperationConnection connCountToRandom = new()
            {
                Source = new WorkflowOperationConnectionPoint()
                {
                    Operation = opInputSampleCount.Identifier,
                    Field = nameof(InputOperationOut.Value)
                },
                Target = new WorkflowOperationConnectionPoint()
                {
                    Operation = opSampleNormal.Identifier,
                    Field = nameof(SampleNormalOperationIn.Count)
                }
            };

            WorkflowOperationConnection connRandomToMean = new()
            {
                Source = new WorkflowOperationConnectionPoint()
                {
                    Operation = opSampleNormal.Identifier,
                    Field = nameof(SampleNormalOperationOut.Samples)
                },
                Target = new WorkflowOperationConnectionPoint()
                {
                    Operation = opMean.Identifier,
                    Field = nameof(MeanOperationIn.Values)
                }
            };
            WorkflowOperationConnection connRandomToDeviation = new()
            {
                Source = new WorkflowOperationConnectionPoint()
                {
                    Operation = opSampleNormal.Identifier,
                    Field = nameof(SampleNormalOperationOut.Samples)
                },
                Target = new WorkflowOperationConnectionPoint()
                {
                    Operation = opDeviation.Identifier,
                    Field = nameof(DeviationOperationIn.Values)
                }
            };

            WorkflowOperationConnection connMeanToMu = new()
            {
                Source = new WorkflowOperationConnectionPoint()
                {
                    Operation = opMean.Identifier,
                    Field = nameof(MeanOperationOut.Mean)
                },
                Target = new WorkflowOperationConnectionPoint()
                {
                    Operation = opOutputMu.Identifier,
                    Field = nameof(OutputOperationIn.Value)
                }
            };
            WorkflowOperationConnection connDeviationToSigma = new()
            {
                Source = new WorkflowOperationConnectionPoint()
                {
                    Operation = opDeviation.Identifier,
                    Field = nameof(DeviationOperationOut.Deviation)
                },
                Target = new WorkflowOperationConnectionPoint()
                {
                    Operation = opOutputSigma.Identifier,
                    Field = nameof(OutputOperationIn.Value)
                }
            };
            #endregion

            // Create the workflow operation.
            WorkflowOperation opWorkflow = new(
                new Operation[]
                {
                    opInputSeed,
                    opInputSampleCount,
                    opSampleNormal,
                    opMean,
                    opDeviation,
                    opOutputMu,
                    opOutputSigma
                },
                new WorkflowOperationConnection[]
                {
                    connSeedToRandom,
                    connCountToRandom,
                    connRandomToMean,
                    connRandomToDeviation,
                    connMeanToMu,
                    connDeviationToSigma
                }
            );

            // Check that an input to the workflow is correctly processed and produces expected results.
            Dictionary<string, object> input = new()
            {
                ["Seed"] = 0,
                ["SampleCount"] = 100000,
            };
            Dictionary<string, object> output = new();
            OperationContext context = new()
            {
                Input = input,
                Output = output
            };
            await opWorkflow.Perform(context);

            Assert.Contains("Mu", output.Keys);
            Assert.Contains("Sigma", output.Keys);
            Assert.AreEqual(1.0, (double)output["Mu"], 0.01);
            Assert.AreEqual(2.0, (double)output["Sigma"], 0.01);
        }

        /// <summary>
        /// Tests that the normal distribution is represented and sampled correctly using a workflow.
        /// The samples from the normal distribution in this test are modified by a pair of arithmetic operations.
        /// </summary>
        [Test]
        public async Task TestModifiedDistribution()
        {
            // The following workflow (diagram) is generated by this test code.
            /*
                Input (Seed) --------+-> Random Normal(Mean=0.0,Deviation=1.0,Count=1000000) [...A]
                Input (SampleCount) -/


                [A...] ->N Arithmetic(Type=Add,Value1=2.0) ->N Arithmetic(Type=Multiply,Value1=-3.0) [...B]

                [B...] -+-> Mean ------> Output (Mu)
                        \-> Deviation -> Output (Sigma)
            */

            // Define the operations required.
            #region Operations
            InputOperation opInputSeed = new()
            {
                DefaultValuesTyped = new() { Name = "Seed" }
            };
            InputOperation opInputSampleCount = new()
            {
                DefaultValuesTyped = new() { Name = "SampleCount" }
            };

            SampleNormalOperation opSampleNormal = new()
            {
                DefaultValuesTyped = new() { Mean = 0.0, Deviation = 1.0 }
            };

            ArithmeticOperation opAdd = new()
            {
                DefaultTyped = new() { Type = ArithmeticOperationType.Add, Input1 = 2.0 }
            };
            ArithmeticOperation opMult = new()
            {
                DefaultTyped = new() { Type = ArithmeticOperationType.Multiply, Input1 = -3.0 }
            };

            MeanOperation opMean = new();
            DeviationOperation opDeviation = new();

            OutputOperation opOutputMu = new()
            {
                DefaultValuesTyped = new() { Name = "Mu" }
            };
            OutputOperation opOutputSigma = new()
            {
                DefaultValuesTyped = new() { Name = "Sigma" }
            };
            #endregion

            // Define the connections required.
            #region Connections
            WorkflowOperationConnection connSeedToRandom = new()
            {
                Source = new() { Operation = opInputSeed.Identifier, Field = nameof(InputOperationOut.Value) },
                Target = new() { Operation = opSampleNormal.Identifier, Field = nameof(SampleNormalOperationIn.Seed) }
            };
            WorkflowOperationConnection connCountToRandom = new()
            {
                Source = new() { Operation = opInputSampleCount.Identifier, Field = nameof(InputOperationOut.Value) },
                Target = new() { Operation = opSampleNormal.Identifier, Field = nameof(SampleNormalOperationIn.Count) }
            };

            WorkflowOperationConnection connRandomToAdd = new()
            {
                Source = new() { Operation = opSampleNormal.Identifier, Field = nameof(SampleNormalOperationOut.Samples) },
                Target = new() { Operation = opAdd.Identifier, Field = nameof(ArithmeticOperationIn.Input2) },
                Multiplexer = true
            };
            WorkflowOperationConnection connAddToMult = new()
            {
                Source = new() { Operation = opAdd.Identifier, Field = nameof(ArithmeticOperationOut.Output) },
                Target = new() { Operation = opMult.Identifier, Field = nameof(ArithmeticOperationIn.Input2), },
                Multiplexer = true
            };

            WorkflowOperationConnection connMultToMean = new()
            {
                Source = new() { Operation = opMult.Identifier, Field = nameof(ArithmeticOperationOut.Output) },
                Target = new() { Operation = opMean.Identifier, Field = nameof(MeanOperationIn.Values) }
            };
            WorkflowOperationConnection connMultToDeviation = new()
            {
                Source = new() { Operation = opMult.Identifier, Field = nameof(ArithmeticOperationOut.Output) },
                Target = new() { Operation = opDeviation.Identifier, Field = nameof(DeviationOperationIn.Values) }
            };

            WorkflowOperationConnection connMeanToMu = new()
            {
                Source = new() { Operation = opMean.Identifier, Field = nameof(MeanOperationOut.Mean) },
                Target = new() { Operation = opOutputMu.Identifier, Field = nameof(OutputOperationIn.Value) }
            };
            WorkflowOperationConnection connDeviationToSigma = new()
            {
                Source = new() { Operation = opDeviation.Identifier, Field = nameof(DeviationOperationOut.Deviation) },
                Target = new() { Operation = opOutputSigma.Identifier, Field = nameof(OutputOperationIn.Value) }
            };
            #endregion

            // Create the workflow operation.
            WorkflowOperation opWorkflow = new(
                new Operation[]
                {
                    opInputSeed,
                    opInputSampleCount,
                    opSampleNormal,
                    opAdd,
                    opMult,
                    opMean,
                    opDeviation,
                    opOutputMu,
                    opOutputSigma
                },
                new WorkflowOperationConnection[]
                {
                    connSeedToRandom,
                    connCountToRandom,
                    connRandomToAdd,
                    connAddToMult,
                    connMultToMean,
                    connMultToDeviation,
                    connMeanToMu,
                    connDeviationToSigma
                }
            );

            // Check that an input to the workflow is correctly processed and produces expected results.
            Dictionary<string, object> input = new()
            {
                ["Seed"] = 0,
                ["SampleCount"] = 100000,
            };
            Dictionary<string, object> output = new();
            OperationContext context = new()
            {
                Input = input,
                Output = output
            };
            await opWorkflow.Perform(context);

            Assert.Contains("Mu", output.Keys);
            Assert.Contains("Sigma", output.Keys);
            Assert.AreEqual(-6.0, (double)output["Mu"], 0.01);
            Assert.AreEqual(3.0, (double)output["Sigma"], 0.01);
        }
    }
}