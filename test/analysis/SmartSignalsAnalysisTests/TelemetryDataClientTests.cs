//-----------------------------------------------------------------------
// <copyright file="TelemetryDataClientTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalsAnalysisSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Analysis;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.HttpClient;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class TelemetryDataClientTests
    {
        private const string Query = "myQuery";
        private const string WorkspaceId = "id";
        private static readonly List<string> WorkspaceNames = new List<string>() { "workspaceName1", "workspaceName2" };

        private readonly Mock<ITracer> tracerMock = new Mock<ITracer>();
        private readonly Mock<ICredentialsFactory> credentialsFactoryMock = new Mock<ICredentialsFactory>();

        [TestMethod]
        public async Task WhenSendingQueryThenTheResultsAreAsExpected()
        {
            var client = new LogAnalyticsTelemetryDataClient(this.tracerMock.Object, new TestHttpClientWrapper(), this.credentialsFactoryMock.Object, WorkspaceId, WorkspaceNames, TimeSpan.FromMinutes(10));
            IList<DataTable> results = await client.RunQueryAsync(Query, default(CancellationToken));
            VerifyDataTables(TestHttpClientWrapper.GetExpectedResults(), results);
        }

        [TestMethod]
        public async Task WhenSendingQueryToApplicationInsightsThenTheResultsAreAsExpected()
        {
            var client = new ApplicationInsightsTelemetryDataClient(this.tracerMock.Object, new TestHttpClientWrapper(applicationInsights: true), this.credentialsFactoryMock.Object, WorkspaceId, WorkspaceNames, TimeSpan.FromMinutes(10));
            IList<DataTable> results = await client.RunQueryAsync(Query, default(CancellationToken));
            VerifyDataTables(TestHttpClientWrapper.GetExpectedResults(), results);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task WhenSendingQueryWithInvalidTypeThenAnExceptionIsThrown()
        {
            var client = new LogAnalyticsTelemetryDataClient(this.tracerMock.Object, new TestHttpClientWrapper(invalidType: true), this.credentialsFactoryMock.Object, WorkspaceId, WorkspaceNames, TimeSpan.FromMinutes(10));
            IList<DataTable> results = await client.RunQueryAsync(Query, default(CancellationToken));
            VerifyDataTables(TestHttpClientWrapper.GetExpectedResults(), results);
        }

        [TestMethod]
        public async Task WhenSendingQueryWithEmptyResultsThenResultsAreAsExpected()
        {
            var client = new LogAnalyticsTelemetryDataClient(this.tracerMock.Object, new TestHttpClientWrapper(emptyResults: true), this.credentialsFactoryMock.Object, WorkspaceId, WorkspaceNames, TimeSpan.FromMinutes(10));
            IList<DataTable> results = await client.RunQueryAsync(Query, default(CancellationToken));
            VerifyDataTables(new List<DataTable>(), results);
        }

        [TestMethod]
        public async Task WhenQueryReturnsAnErrorThenTheCorrectExceptionIsThrown()
        {
            var client = new LogAnalyticsTelemetryDataClient(this.tracerMock.Object, new TestHttpClientWrapper(error: true), this.credentialsFactoryMock.Object, WorkspaceId, WorkspaceNames, TimeSpan.FromMinutes(10));
            try
            {
                await client.RunQueryAsync(Query, default(CancellationToken));
                Assert.Fail("An exception should have been thrown");
            }
            catch (TelemetryDataClientException e)
            {
                Assert.AreEqual($"[test error code] test error message\r\n query = {Query}", e.Message, "Exception message mismatch");
                Assert.IsNotNull(e.InnerException, "Inner exception is null");
                Assert.AreEqual(typeof(TelemetryDataClientException), e.InnerException.GetType(), "Inner exception is null");
                Assert.AreEqual("[test inner error code] test inner error message", e.InnerException.Message, "Inner exception message mismatch");
                Assert.IsNull(e.InnerException.InnerException, "Inner exception of inner exception is not null");
            }
        }

        private static void VerifyDataTables(IList<DataTable> expectedTables, IList<DataTable> actualTables)
        {
            Assert.AreEqual(expectedTables.Count, actualTables.Count, "Number of tables mismatch");

            for (int i = 0; i < expectedTables.Count; i++)
            {
                MemoryStream ms1 = new MemoryStream();
                expectedTables[i].WriteXml(ms1);
                string s1 = Encoding.UTF8.GetString(ms1.ToArray());
                MemoryStream ms2 = new MemoryStream();
                actualTables[i].WriteXml(ms2);
                string s2 = Encoding.UTF8.GetString(ms2.ToArray());
                Assert.AreEqual(s1, s2, "Data tables mismatch");
            }
        }

        private class TestHttpClientWrapper : IHttpClientWrapper
        {
            private readonly bool invalidType;
            private readonly bool emptyResults;
            private readonly bool applicationInsights;
            private readonly bool error;

            public TestHttpClientWrapper(bool invalidType = false, bool emptyResults = false, bool applicationInsights = false, bool error = false)
            {
                this.invalidType = invalidType;
                this.emptyResults = emptyResults;
                this.applicationInsights = applicationInsights;
                this.error = error;
            }

            public TimeSpan Timeout { get; set; }

            public static List<DataTable> GetExpectedResults()
            {
                DataTable table1 = new DataTable("PrimaryResult");
                table1.Columns.Add("Category", typeof(string));
                table1.Columns.Add("count_", typeof(long));
                table1.Rows.Add("Administrative", 20839);
                table1.Rows.Add("Recommendation", 122);
                table1.Rows.Add("Alert", 64);
                table1.Rows.Add("ServiceHealth", 11);

                DataTable table2 = new DataTable("SecondaryResult");
                table2.Columns.Add("UniqueId", typeof(Guid));
                table2.Columns.Add("Name", typeof(string));
                table2.Columns.Add("Birthday", typeof(DateTime));
                table2.Rows.Add(Guid.Parse("9edfe824-0f7b-4cb1-aab9-facb1237876b"), "John", new DateTime(1643, 1, 4, 00, 11, 22));
                table2.Rows.Add(Guid.Parse("fd9f3b3c-4766-4a9c-b830-e1f31b1f8c9a"), "Elisabeth", new DateTime(1777, 4, 30, 11, 22, 33));
                table2.Rows.Add(Guid.Parse("73f75173-eb77-4096-9c01-84f58d9753c4"), "George", new DateTime(1912, 6, 23, 22, 33, 44));

                return new List<DataTable>() { table1, table2 };
            }

            public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // Verify request
                string expectedUri = this.applicationInsights ? $"https://api.applicationinsights.io/v1/apps/{WorkspaceId}/query" : $"https://api.loganalytics.io/v1/workspaces/{WorkspaceId}/query";
                Assert.AreEqual(expectedUri, request.RequestUri.ToString(), "Request URI mismatch");
                Assert.AreEqual("Prefer: wait=600\r\n", request.Headers.ToString(), "Request headers mismatch");

                JObject expectedContent = new JObject()
                {
                    ["query"] = Query,
                    [this.applicationInsights ? "applications" : "workspaces"] = new JArray(WorkspaceNames)
                };

                JObject actualContent = (JObject)JsonConvert.DeserializeObject(await request.Content.ReadAsStringAsync());
                Assert.AreEqual(expectedContent.ToString(), actualContent.ToString(), "Request content mismatch");

                // Return error if requested
                if (this.error)
                {
                    JObject errorObject = new JObject()
                    {
                        ["error"] = new JObject()
                        {
                            ["message"] = "test error message",
                            ["code"] = "test error code",
                            ["innererror"] = new JObject()
                            {
                                ["code"] = "test inner error code",
                                ["message"] = "test inner error message"
                            }
                        }
                    };

                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(errorObject.ToString())
                    };
                }

                // Return OK result
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(this.GetExpectedResultsAsJObject().ToString())
                };
            }

            private JObject GetExpectedResultsAsJObject()
            {
                if (this.emptyResults)
                {
                    return new JObject()
                    {
                        ["tables"] = new JArray()
                    };
                }

                return new JObject()
                {
                    ["tables"] = new JArray()
                    {
                        new JObject()
                        {
                            ["name"] = "PrimaryResult",
                            ["columns"] = new JArray()
                            {
                                new JObject()
                                {
                                    ["name"] = "Category",
                                    ["type"] = "string"
                                },
                                new JObject()
                                {
                                    ["name"] = "count_",
                                    ["type"] = "long"
                                }
                            },
                            ["rows"] = new JArray
                            {
                                new JArray()
                                {
                                    "Administrative",
                                    20839
                                },
                                new JArray()
                                {
                                    "Recommendation",
                                    122
                                },
                                new JArray()
                                {
                                    "Alert",
                                    64
                                },
                                new JArray()
                                {
                                    "ServiceHealth",
                                    11
                                }
                            }
                        },
                        new JObject()
                        {
                            ["name"] = "SecondaryResult",
                            ["columns"] = new JArray()
                            {
                                new JObject()
                                {
                                    ["name"] = "UniqueId",
                                    ["type"] = "guid"
                                },
                                new JObject()
                                {
                                    ["name"] = "Name",
                                    ["type"] = this.invalidType ? "bad_type" : "string"
                                },
                                new JObject()
                                {
                                    ["name"] = "Birthday",
                                    ["type"] = "datetime"
                                }
                            },
                            ["rows"] = new JArray
                            {
                                new JArray()
                                {
                                    "9edfe824-0f7b-4cb1-aab9-facb1237876b",
                                    "John",
                                    "1643-01-04T00:11:22.000Z"
                                },
                                new JArray()
                                {
                                    "fd9f3b3c-4766-4a9c-b830-e1f31b1f8c9a",
                                    "Elisabeth",
                                    "1777-04-30T11:22:33.000Z"
                                },
                                new JArray()
                                {
                                    "73f75173-eb77-4096-9c01-84f58d9753c4",
                                    "George",
                                    "1912-06-23T22:33:44.000Z"
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}
