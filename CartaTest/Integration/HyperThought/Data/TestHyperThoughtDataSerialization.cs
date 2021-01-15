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
                    ""header"": {
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
            IList<File> files = JsonSerializer.Deserialize<List<File>>(jsonPkg);
            File file = files.FirstOrDefault();

            // Assert a collection of random values.
            Assert.AreEqual(1, files.Count);
            Assert.AreEqual("New Text Document - Copy (1).txt", file.Content.Name);
            Assert.AreEqual("txt", file.Content.FileExtension);
            Assert.AreEqual("/test2/test3/New Text Document - Copy.txt", file.Content.DirectoryPath);
            Assert.AreEqual("fourmajr", file.Content.CreatedBy);
            Assert.AreEqual(new DateTime(2020, 2, 12, 14, 39, 03, 787), file.Content.CreatedTime);
            Assert.AreEqual(Backend.Default, file.Content.Backend);
            Assert.AreEqual("blue", file.Metadata[0].Value.Link);
            Assert.AreEqual(0, file.Triples.Count);
            Assert.AreEqual(Distribution.None, file.Restrictions.Distribution);
            Assert.AreEqual("/files/filesystementry/00dfb7b9-b675-4303-b2e6-4916c813ca31/versions/0", file.Header.Uri);
        }

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
                ""header"": {
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
            File file = JsonSerializer.Deserialize<File>(jsonPkg);

            // Assert a collection of random values.
            Assert.AreEqual("New Text Document - Copy (3).pdf", file.Content.Name);
            Assert.AreEqual("pdf", file.Content.FileExtension);
            Assert.AreEqual("/test2/test3/New Text Document - Copy.txt", file.Content.DirectoryPath);
            Assert.AreEqual("fourmajr", file.Content.CreatedBy);
            Assert.AreEqual(new DateTime(2020, 2, 12, 14, 39, 03, 787), file.Content.CreatedTime);
            Assert.AreEqual(Backend.Default, file.Content.Backend);
            Assert.AreEqual("red", file.Metadata[0].Value.Link);
            Assert.AreEqual(0, file.Triples.Count);
            Assert.AreEqual(Distribution.None, file.Restrictions.Distribution);
            Assert.AreEqual("/files/filesystementry/00dfb7b9-b675-4303-b2e6-4916c813ca31/versions/0", file.Header.Uri);
        }
    }
}