using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using CartaCore.Integration.HyperThought.Data;
using NUnit.Framework;

namespace CartaTest.Integration.HyperThought.Data
{
    [TestFixture]
    public class TestHyperThoughtDataSerialization
    {
        [Test]
        public void TestApiWorkflowProcess()
        {
            string jsonPkg =
            @"
            [
            {
                ""content"": {
                    ""status"": """",
                    ""xml"": ""string"",
                    ""name"": ""string"",
                    ""parent_process"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                    ""pid"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                    ""client_id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                    ""successors"": [
                        ""3fa85f64-5717-4562-b3fc-2c963f66afa6""
                    ],
                    ""predecessors"": [
                        ""3fa85f64-5717-4562-b3fc-2c963f66afa6""
                    ],
                    ""process_type"": ""workflow"",
                    ""template"": ""string"",
                    ""children"": [
                        ""3fa85f64-5717-4562-b3fc-2c963f66afa6""
                    ],
                    ""assignee"": ""string"",
                    ""started"": ""2021-01-15T19:26:08.667Z"",
                    ""completed"": ""2021-01-15T19:26:08.667Z"",
                    ""notes"": ""string"",
                    ""creator"": ""string"",
                    ""created"": ""2021-01-15T19:26:08.667Z"",
                    ""modifier"": ""string"",
                    ""modified"": ""2021-01-15T19:26:08.667Z"",
                    ""pk"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6""
                },
                ""triples"": [
                    {
                        ""triple"": {
                            ""subject"": {
                                ""value"": ""string""
                            },
                            ""predicate"": {
                                ""value"": ""string""
                            },
                            ""object"": {
                                ""value"": ""string""
                            }
                        }
                    }
                ],
                ""metadata"": [
                    {
                        ""keyName"": ""string"",
                        ""value"": {
                            ""type"": ""string"",
                            ""link"": ""string""
                        },
                        ""annotation"": ""string""
                    }
                ],
                ""header"": {
                    ""canonincal-uri"": ""string"",
                    ""sys-creation-timestamp"": ""2021-01-15T19:26:08.667Z"",
                    ""sys-last-modified"": ""2021-01-15T19:26:08.667Z"",
                    ""createdBy"": ""string"",
                    ""modifiedBy"": ""string"",
                    ""uri"": ""string"",
                    ""pid"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6""
                },
                ""permissions"": {
                    ""projects"": {},
                    ""groups"": {},
                    ""users"": {}
                },
                ""restrictions"": {
                    ""distribution"": ""distribution a"",
                    ""exportControl"": ""itar"",
                    ""securityMarking"": ""string""
                }
            }
            ]
            ";

            // Parse the package string into an appropriately-typed JSON object.
            IList<Workflow> workflows = JsonSerializer.Deserialize<List<Workflow>>(jsonPkg);
            Workflow workflow = workflows.FirstOrDefault();

            // Assert a collection of random values.
            Assert.AreEqual(1, workflows.Count);
            Assert.AreEqual(WorkflowStatus.None, workflow.Content.Status);
            Assert.AreEqual("string", workflow.Content.Name);
            Assert.AreEqual("3fa85f64-5717-4562-b3fc-2c963f66afa6", workflow.Content.ClientId);
            Assert.AreEqual(1, workflow.Content.ChildrenIds.Count);
            Assert.AreEqual(new DateTime(2021, 01, 15, 19, 26, 8, 667), workflow.Content.CompletedTime);
            Assert.AreEqual(1, workflow.Triples.Count);
            Assert.AreEqual(1, workflow.Metadata.Count);
            Assert.AreEqual("string", workflow.Metadata[0].Value.Link);
            Assert.AreEqual(Distribution.DistributionA, workflow.Restrictions.Distribution);
            Assert.AreEqual(ExportControl.ITAR, workflow.Restrictions.ExportControl);
        }

        [Test]
        public void TestApiWorkflowProcessKey()
        {
            string jsonPkg =
            @"
            {
                ""content"": {
                    ""status"": """",
                    ""xml"": ""string"",
                    ""name"": ""string"",
                    ""parent_process"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                    ""pid"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                    ""client_id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                    ""successors"": [
                        ""3fa85f64-5717-4562-b3fc-2c963f66afa6""
                    ],
                    ""predecessors"": [
                        ""3fa85f64-5717-4562-b3fc-2c963f66afa6""
                    ],
                    ""process_type"": ""workflow"",
                    ""template"": ""string"",
                    ""children"": [
                        ""3fa85f64-5717-4562-b3fc-2c963f66afa6""
                    ],
                    ""assignee"": ""string"",
                    ""started"": ""2021-01-15T19:26:08.667Z"",
                    ""completed"": ""2021-01-15T21:09:14.060Z"",
                    ""notes"": ""string"",
                    ""creator"": ""string"",
                    ""created"": ""2021-01-15T19:26:08.667Z"",
                    ""modifier"": ""string"",
                    ""modified"": ""2021-01-15T19:26:08.667Z"",
                    ""pk"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6""
                },
                ""triples"": [
                    {
                        ""triple"": {
                            ""subject"": {
                                ""value"": ""string""
                            },
                            ""predicate"": {
                                ""value"": ""string""
                            },
                            ""object"": {
                                ""value"": ""string""
                            }
                        }
                    }
                ],
                ""metadata"": [
                    {
                        ""keyName"": ""string"",
                        ""value"": {
                            ""type"": ""string"",
                            ""link"": ""string""
                        },
                        ""annotation"": ""string""
                    }
                ],
                ""header"": {
                    ""canonincal-uri"": ""string"",
                    ""sys-creation-timestamp"": ""2021-01-15T19:26:08.667Z"",
                    ""sys-last-modified"": ""2021-01-15T19:26:08.667Z"",
                    ""createdBy"": ""string"",
                    ""modifiedBy"": ""string"",
                    ""uri"": ""string"",
                    ""pid"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6""
                },
                ""permissions"": {
                    ""projects"": {},
                    ""groups"": {},
                    ""users"": {}
                },
                ""restrictions"": {
                    ""distribution"": ""distribution b"",
                    ""exportControl"": ""ear"",
                    ""securityMarking"": ""string""
                }
            }
            ";

            // Parse the package string into an appropriately-typed JSON object.
            Workflow workflow = JsonSerializer.Deserialize<Workflow>(jsonPkg);

            // Assert a collection of random values.
            Assert.AreEqual(WorkflowStatus.None, workflow.Content.Status);
            Assert.AreEqual("string", workflow.Content.Name);
            Assert.AreEqual("3fa85f64-5717-4562-b3fc-2c963f66afa6", workflow.Content.ClientId);
            Assert.AreEqual(1, workflow.Content.ChildrenIds.Count);
            Assert.AreEqual(new DateTime(2021, 01, 15, 21, 9, 14, 60), workflow.Content.CompletedTime);
            Assert.AreEqual(1, workflow.Triples.Count);
            Assert.AreEqual(1, workflow.Metadata.Count);
            Assert.AreEqual("string", workflow.Metadata[0].Value.Link);
            Assert.AreEqual(Distribution.DistributionB, workflow.Restrictions.Distribution);
            Assert.AreEqual(ExportControl.EAR, workflow.Restrictions.ExportControl);
        }
    }
}