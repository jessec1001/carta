using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using NUnit.Framework;

using CartaCore.Integration.Hyperthought.Data;

namespace CartaTest.Integration.Hyperthought.Data
{
    /// <summary>
    /// Tests the deserialization of JSON returned from HyperThought API calls.
    /// </summary>
    [TestFixture]
    public class TestHyperthoughtDataSerialization
    {
        /// <summary>
        /// Tests the deserialization of multiple HyperThought workflow processes.
        /// </summary>
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
                    ""template"": true,
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
                ""headers"": {
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
            IList<HyperthoughtWorkflow> workflows = JsonSerializer.Deserialize<List<HyperthoughtWorkflow>>(jsonPkg);
            HyperthoughtWorkflow workflow = workflows.FirstOrDefault();

            // Assert a collection of random values.
            Assert.AreEqual(1, workflows.Count);
            Assert.AreEqual(HyperthoughtProcessStatus.None, workflow.Content.Status);
            Assert.AreEqual("string", workflow.Content.Name);
            Assert.AreEqual(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), workflow.Content.ClientId);
            Assert.AreEqual(1, workflow.Content.ChildrenIds.Count);
            Assert.AreEqual(new DateTime(2021, 01, 15, 19, 26, 8, 667), workflow.Content.CompletedTime);
            Assert.AreEqual(1, workflow.Triples.Count);
            Assert.AreEqual(1, workflow.Metadata.Count);
            Assert.AreEqual("string", workflow.Metadata[0].Value.Link.ToString());
            Assert.AreEqual(HyperthoughtDistribution.DistributionA, workflow.Restrictions.Distribution);
            Assert.AreEqual(HyperthoughtExportControl.ITAR, workflow.Restrictions.ExportControl);
        }

        /// <summary>
        /// Tests the deserialization of a single HyperThought workflow process.
        /// </summary>
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
                    ""template"": false,
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
                ""headers"": {
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
            HyperthoughtWorkflow workflow = JsonSerializer.Deserialize<HyperthoughtWorkflow>(jsonPkg);

            // Assert a collection of random values.
            Assert.AreEqual(HyperthoughtProcessStatus.None, workflow.Content.Status);
            Assert.AreEqual("string", workflow.Content.Name);
            Assert.AreEqual(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), workflow.Content.ClientId);
            Assert.AreEqual(1, workflow.Content.ChildrenIds.Count);
            Assert.AreEqual(new DateTime(2021, 01, 15, 21, 9, 14, 60), workflow.Content.CompletedTime);
            Assert.AreEqual(1, workflow.Triples.Count);
            Assert.AreEqual(1, workflow.Metadata.Count);
            Assert.AreEqual("string", workflow.Metadata[0].Value.Link.ToString());
            Assert.AreEqual(HyperthoughtDistribution.DistributionB, workflow.Restrictions.Distribution);
            Assert.AreEqual(HyperthoughtExportControl.EAR, workflow.Restrictions.ExportControl);
        }

        /// <summary>
        /// Tests the deserialization of multiple HyperThought files.
        /// </summary>
        [Test]
        public void TestApiFiles()
        {
            string jsonPkg =
            @"
            [
                {
                    ""content"": {
                        ""name"": ""New Text Document - Copy (1).txt"",
                        ""ftype"": ""txt"",
                        ""path"": "",3bbf74b6-c351-49cb-8ddd-b5646e587d75,874d9e44-fe02-408c-9396-6c6b078672f8,"",
                        ""path_string"": ""/test2/test3/New Text Document - Copy.txt"",
                        ""size"": 0,
                        ""items"": 0,
                        ""created_by"": ""fourmajr"",
                        ""created"": ""2020-02-12T14:39:03.787Z"",
                        ""modified_by"": ""fourmajr"",
                        ""modified"": ""2020-02-12T14:39:03.787Z"",
                        ""backend"": ""default"",
                        ""file"": ""48e69a94-fcea-4dbc-9ce5-bb189ef62361"",
                        ""pk"": ""00dfb7b9-b675-4303-b2e6-4916c813ca31""
                    },
                    ""metadata"": [
                        {
                            ""keyName"": ""test"",
                            ""value"": {
                                ""type"": ""string"",
                                ""link"": ""blue""
                            },
                            ""annotation"": """"
                        }
                    ],
                    ""triples"": [],
                    ""permissions"": {
                        ""groups"": {},
                        ""projects"": {},
                        ""users"": {
                            ""fourmajr"": ""edit""
                        }
                    },
                    ""restrictions"": {
                        ""distribution"": """",
                        ""exportControl"": """",
                        ""securityMarking"": """"
                    },
                    ""headers"": {
                        ""canonical-uri"": ""/files/filesystementry/00dfb7b9-b675-4303-b2e6-4916c813ca31"",
                        ""sys-creation-timestamp"": ""2020-02-12T14:39:03.7871543-05:00"",
                        ""sys-last-modified"": ""2020-02-12T14:39:03.7871543-05:00"",
                        ""createdBy"": ""fourmajr"",
                        ""modifiedBy"": ""fourmajr"",
                        ""uri"": ""/files/filesystementry/00dfb7b9-b675-4303-b2e6-4916c813ca31/versions/0"",
                        ""pid"": ""b26ea920-571f-4831-a895-778029f98294""
                    }
                }
            ]
            ";

            // Parse the package string into an appropriately-typed JSON object.
            IList<HyperthoughtFile> files = JsonSerializer.Deserialize<List<HyperthoughtFile>>(jsonPkg);
            HyperthoughtFile file = files.FirstOrDefault();

            // Assert a collection of random values.
            Assert.AreEqual(1, files.Count);
            Assert.AreEqual("New Text Document - Copy (1).txt", file.Content.Name);
            Assert.AreEqual("txt", file.Content.FileExtension);
            Assert.AreEqual("/test2/test3/New Text Document - Copy.txt", file.Content.DirectoryPath);
            Assert.AreEqual("fourmajr", file.Content.CreatedBy);
            Assert.AreEqual(new DateTime(2020, 2, 12, 14, 39, 03, 787), file.Content.CreatedTime);
            Assert.AreEqual(HyperthoughtBackend.Default, file.Content.Backend);
            Assert.AreEqual("blue", file.Metadata[0].Value.Link.ToString());
            Assert.AreEqual(0, file.Triples.Count);
            Assert.AreEqual(HyperthoughtDistribution.None, file.Restrictions.Distribution);
            Assert.AreEqual("/files/filesystementry/00dfb7b9-b675-4303-b2e6-4916c813ca31/versions/0", file.Headers.Uri);
        }

        /// <summary>
        /// Tests the deserialization of a single HyperThought file.
        /// </summary>
        [Test]
        public void TestApiFilesKey()
        {
            string jsonPkg =
            @"
            {
                ""content"": {
                    ""name"": ""New Text Document - Copy (3).pdf"",
                    ""ftype"": ""pdf"",
                    ""path"": "",3bbf74b6-c351-49cb-8ddd-b5646e587d75,874d9e44-fe02-408c-9396-6c6b078672f8,"",
                    ""path_string"": ""/test2/test3/New Text Document - Copy.txt"",
                    ""size"": 0,
                    ""items"": 0,
                    ""created_by"": ""fourmajr"",
                    ""created"": ""2020-02-12T14:39:03.787Z"",
                    ""modified_by"": ""fourmajr"",
                    ""modified"": ""2020-02-12T14:39:03.787Z"",
                    ""backend"": ""default"",
                    ""file"": ""48e69a94-fcea-4dbc-9ce5-bb189ef62361"",
                    ""pk"": ""00dfb7b9-b675-4303-b2e6-4916c813ca31""
                },
                ""metadata"": [
                    {
                        ""keyName"": ""test"",
                        ""value"": {
                            ""type"": ""string"",
                            ""link"": ""red""
                        },
                        ""annotation"": """"
                    }
                ],
                ""triples"": [],
                ""permissions"": {
                    ""groups"": {},
                    ""projects"": {},
                    ""users"": {
                        ""fourmajr"": ""edit""
                    }
                },
                ""restrictions"": {
                    ""distribution"": """",
                    ""exportControl"": """",
                    ""securityMarking"": """"
                },
                ""headers"": {
                    ""canonical-uri"": ""/files/filesystementry/00dfb7b9-b675-4303-b2e6-4916c813ca31"",
                    ""sys-creation-timestamp"": ""2020-02-12T14:39:03.7871543-05:00"",
                    ""sys-last-modified"": ""2020-02-12T14:39:03.7871543-05:00"",
                    ""createdBy"": ""fourmajr"",
                    ""modifiedBy"": ""fourmajr"",
                    ""uri"": ""/files/filesystementry/00dfb7b9-b675-4303-b2e6-4916c813ca31/versions/0"",
                    ""pid"": ""b26ea920-571f-4831-a895-778029f98294""
                }
            }
            ";

            // Parse the package string into an appropriately-typed JSON object.
            HyperthoughtFile file = JsonSerializer.Deserialize<HyperthoughtFile>(jsonPkg);

            // Assert a collection of random values.
            Assert.AreEqual("New Text Document - Copy (3).pdf", file.Content.Name);
            Assert.AreEqual("pdf", file.Content.FileExtension);
            Assert.AreEqual("/test2/test3/New Text Document - Copy.txt", file.Content.DirectoryPath);
            Assert.AreEqual("fourmajr", file.Content.CreatedBy);
            Assert.AreEqual(new DateTime(2020, 2, 12, 14, 39, 03, 787), file.Content.CreatedTime);
            Assert.AreEqual(HyperthoughtBackend.Default, file.Content.Backend);
            Assert.AreEqual("red", file.Metadata[0].Value.Link.ToString());
            Assert.AreEqual(0, file.Triples.Count);
            Assert.AreEqual(HyperthoughtDistribution.None, file.Restrictions.Distribution);
            Assert.AreEqual("/files/filesystementry/00dfb7b9-b675-4303-b2e6-4916c813ca31/versions/0", file.Headers.Uri);
        }
    }
}