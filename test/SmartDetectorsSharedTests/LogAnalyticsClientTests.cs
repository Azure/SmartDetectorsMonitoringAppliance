//-----------------------------------------------------------------------
// <copyright file="LogAnalyticsClientTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class LogAnalyticsClientTests
    {
        private const string Query = "myQuery";
        private static readonly TimeSpan DataTimeSpan = new TimeSpan(0, 5, 0);

        private readonly Mock<ITracer> tracerMock = new Mock<ITracer>();
        private readonly Mock<ICredentialsFactory> credentialsFactoryMock = new Mock<ICredentialsFactory>();

        [TestMethod]
        public async Task WhenSendingQueryThenTheResultsAreAsExpected()
        {
            var client = new LogAnalyticsClient(this.tracerMock.Object, new TestHttpClientWrapper(), this.credentialsFactoryMock.Object, new Uri("https://dummy.query.com"), TimeSpan.FromMinutes(10));
            IList<DataTable> results = await client.RunQueryAsync(Query, DataTimeSpan, default(CancellationToken));
            VerifyDataTables(TestHttpClientWrapper.GetExpectedResults(), results);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task WhenSendingQueryWithInvalidTypeThenAnExceptionIsThrown()
        {
            var client = new LogAnalyticsClient(this.tracerMock.Object, new TestHttpClientWrapper(invalidType: true), this.credentialsFactoryMock.Object, new Uri("https://dummy.query.com"), TimeSpan.FromMinutes(10));
            IList<DataTable> results = await client.RunQueryAsync(Query, DataTimeSpan, default(CancellationToken));
            VerifyDataTables(TestHttpClientWrapper.GetExpectedResults(), results);
        }

        [TestMethod]
        public async Task WhenSendingQueryWithEmptyResultsThenResultsAreAsExpected()
        {
            var client = new LogAnalyticsClient(this.tracerMock.Object, new TestHttpClientWrapper(emptyResults: true), this.credentialsFactoryMock.Object, new Uri("https://dummy.query.com"), TimeSpan.FromMinutes(10));
            IList<DataTable> results = await client.RunQueryAsync(Query, DataTimeSpan, default(CancellationToken));
            VerifyDataTables(new List<DataTable>(), results);
        }

        [TestMethod]
        public async Task WhenQueryReturnsAnErrorThenTheCorrectExceptionIsThrown()
        {
            var client = new LogAnalyticsClient(this.tracerMock.Object, new TestHttpClientWrapper(error: true), this.credentialsFactoryMock.Object, new Uri("https://dummy.query.com"), TimeSpan.FromMinutes(10));
            try
            {
                await client.RunQueryAsync(Query, DataTimeSpan, default(CancellationToken));
                Assert.Fail("An exception should have been thrown");
            }
            catch (TelemetryDataClientException e)
            {
                Assert.AreEqual($"[test error code] test error message", e.Message, "Exception message mismatch");
                Assert.IsNull(e.InnerException, "Inner exception is not null");
            }
        }

        private static void VerifyDataTables(IList<DataTable> expectedTables, IList<DataTable> actualTables)
        {
            Assert.AreEqual(expectedTables.Count, actualTables.Count, "Number of tables mismatch");

            // Sort the table rows and compare (to allow different row order)
            for (int i = 0; i < expectedTables.Count; i++)
            {
                expectedTables[i].DefaultView.Sort = string.Join(", ", from DataColumn col in expectedTables[i].Columns orderby col.ColumnName select col.ColumnName);
                string s1 = JsonConvert.SerializeObject(expectedTables[i].DefaultView.ToTable());

                actualTables[i].DefaultView.Sort = string.Join(", ", from DataColumn col in actualTables[i].Columns orderby col.ColumnName select col.ColumnName);
                string s2 = JsonConvert.SerializeObject(actualTables[i].DefaultView.ToTable());

                Assert.AreEqual(s1, s2, "Data tables mismatch");
            }
        }

        private class TestHttpClientWrapper : IHttpClientWrapper
        {
            private readonly bool invalidType;
            private readonly bool emptyResults;
            private readonly bool error;

            public TestHttpClientWrapper(bool invalidType = false, bool emptyResults = false, bool error = false)
            {
                this.invalidType = invalidType;
                this.emptyResults = emptyResults;
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

                return new List<DataTable>() { table1 };
            }

            public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, TimeSpan? timeout, CancellationToken cancellationToken)
            {
                // Verify request
                Assert.AreEqual(new Uri("https://dummy.query.com"), request.RequestUri, "Request URI mismatch");
                Assert.AreEqual(3, request.Headers.Count(), "Request headers count mismatch");
                Assert.AreEqual("wait=600, ai.include-error-payload=true", request.Headers.GetValues("Prefer").First(), "Prefer header mismatch");
                Assert.AreEqual("SmartDetectorsMonitoringAppliance", request.Headers.GetValues("x-ms-app").First(), "App name header mismatch");
                Assert.IsTrue(request.Headers.Contains("x-ms-client-request-id"), "Draft request ID header missing");

                JObject expectedContent = new JObject()
                {
                    ["query"] = Query,
                    ["timespan"] = System.Xml.XmlConvert.ToString(DataTimeSpan)
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
                                    ["type"] = this.invalidType ? "bad_type" : "string"
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
                        }
                    }
                };
            }
        }
    }
}
