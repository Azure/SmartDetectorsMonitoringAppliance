//-----------------------------------------------------------------------
// <copyright file="TelemetryDataClientTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Arm;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions.Clients;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class TelemetryDataClientTests
    {
        private const string Query = "myQuery";
        private static readonly TimeSpan DataTimeSpan = new TimeSpan(0, 5, 0);
        private static readonly List<string> Workspaces = new List<string>()
        {
            "/subscriptions/09cfc56a-0a51-417f-99dc-ebaf882fef3b/resourceGroups/someResourceGroup/providers/Microsoft.OperationalInsights/workspaces/workspaceName1",
            "/subscriptions/09cfc56a-0a51-417f-99dc-ebaf882fef3b/resourceGroups/someResourceGroup/providers/Microsoft.OperationalInsights/workspaces/workspaceName2"
        };

        private static readonly List<string> Applications = new List<string>()
        {
            "/subscriptions/09cfc56a-0a51-417f-99dc-ebaf882fef3b/resourceGroups/someResourceGroup/providers/microsoft.insights/components/application1"
        };

        private static readonly Dictionary<ResourceIdentifier, string> WorkspaceResourceIdToWorkspaceIdMapping = new Dictionary<ResourceIdentifier, string>
        {
            { ResourceIdentifier.CreateFromResourceId("/subscriptions/09cfc56a-0a51-417f-99dc-ebaf882fef3b/resourceGroups/someResourceGroup/providers/Microsoft.OperationalInsights/workspaces/workspaceName1"), "262cd8d9-465e-454e-bcf0-39829755d802" },
            { ResourceIdentifier.CreateFromResourceId("/subscriptions/09cfc56a-0a51-417f-99dc-ebaf882fef3b/resourceGroups/someResourceGroup/providers/Microsoft.OperationalInsights/workspaces/workspaceName2"), "dfcfdffb-c367-4842-897a-8407f3bfdde3" }
        };

        private static readonly Dictionary<ResourceIdentifier, string> ApplicationResourceIdToApplicationIdMapping = new Dictionary<ResourceIdentifier, string>
        {
            { ResourceIdentifier.CreateFromResourceId("/subscriptions/09cfc56a-0a51-417f-99dc-ebaf882fef3b/resourceGroups/someResourceGroup/providers/microsoft.insights/components/application1"), "24bacd4b-2a90-4f62-b8ae-df73ccfb0e67" }
        };

        private readonly Mock<IExtendedTracer> tracerMock = new Mock<IExtendedTracer>();
        private readonly Mock<ICredentialsFactory> credentialsFactoryMock = new Mock<ICredentialsFactory>();
        private readonly Mock<IExtendedAzureResourceManagerClient> azureResourceManagerClientMock = new Mock<IExtendedAzureResourceManagerClient>();

        [TestInitialize]
        public void TestInitialize()
        {
            this.azureResourceManagerClientMock
            .Setup(m => m.GetLogAnalyticsWorkspaceIdAsync(It.IsAny<ResourceIdentifier>(), It.IsAny<CancellationToken>()))
            .Returns<ResourceIdentifier, CancellationToken>(
                (resource, token) =>
                    WorkspaceResourceIdToWorkspaceIdMapping.ContainsKey(resource) ?
                        Task.FromResult(WorkspaceResourceIdToWorkspaceIdMapping[resource]) :
                        throw new Microsoft.Rest.Azure.CloudException()
                        {
                            Response = new Microsoft.Rest.HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
                        });

            this.azureResourceManagerClientMock
            .Setup(m => m.GetApplicationInsightsAppIdAsync(It.IsAny<ResourceIdentifier>(), It.IsAny<CancellationToken>()))
            .Returns<ResourceIdentifier, CancellationToken>(
                (resource, token) =>
                    ApplicationResourceIdToApplicationIdMapping.ContainsKey(resource) ?
                        Task.FromResult(ApplicationResourceIdToApplicationIdMapping[resource]) :
                        throw new Microsoft.Rest.Azure.CloudException()
                        {
                            Response = new Microsoft.Rest.HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
                        });
        }

        [TestMethod]
        public async Task WhenSendingQueryThenTheResultsAreAsExpected()
        {
            var client = new LogAnalyticsTelemetryDataClient(this.tracerMock.Object, new TestHttpClientWrapper(), this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object, Workspaces, TimeSpan.FromMinutes(10));
            IList<DataTable> results = await client.RunQueryAsync(Query, DataTimeSpan, default(CancellationToken));
            VerifyDataTables(TestHttpClientWrapper.GetExpectedResults(), results);
        }

        [TestMethod]
        public async Task WhenSendingQueryToApplicationInsightsThenTheResultsAreAsExpected()
        {
            var client = new ApplicationInsightsTelemetryDataClient(this.tracerMock.Object, new TestHttpClientWrapper(applicationInsights: true), this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object, Applications, TimeSpan.FromMinutes(10));
            IList<DataTable> results = await client.RunQueryAsync(Query, DataTimeSpan, default(CancellationToken));
            VerifyDataTables(TestHttpClientWrapper.GetExpectedResults(), results);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task WhenSendingQueryWithInvalidTypeThenAnExceptionIsThrown()
        {
            var client = new LogAnalyticsTelemetryDataClient(this.tracerMock.Object, new TestHttpClientWrapper(invalidType: true), this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object, Workspaces, TimeSpan.FromMinutes(10));
            IList<DataTable> results = await client.RunQueryAsync(Query, DataTimeSpan, default(CancellationToken));
            VerifyDataTables(TestHttpClientWrapper.GetExpectedResults(), results);
        }

        [TestMethod]
        public async Task WhenSendingQueryWithNullTimeSpanThenTheResultsAreAsExpected()
        {
            var client = new LogAnalyticsTelemetryDataClient(this.tracerMock.Object, new TestHttpClientWrapper(expectTimeSpan: false), this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object, Workspaces, TimeSpan.FromMinutes(10));
            IList<DataTable> results = await client.RunQueryAsync(Query, default(CancellationToken));
            VerifyDataTables(TestHttpClientWrapper.GetExpectedResults(), results);
        }

        [TestMethod]
        public async Task WhenSendingQueryWithEmptyResultsThenResultsAreAsExpected()
        {
            var client = new LogAnalyticsTelemetryDataClient(this.tracerMock.Object, new TestHttpClientWrapper(emptyResults: true), this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object, Workspaces, TimeSpan.FromMinutes(10));
            IList<DataTable> results = await client.RunQueryAsync(Query, DataTimeSpan, default(CancellationToken));
            VerifyDataTables(new List<DataTable>(), results);
        }

        [TestMethod]
        public async Task WhenQueryReturnsAnErrorThenTheCorrectExceptionIsThrown()
        {
            var client = new LogAnalyticsTelemetryDataClient(this.tracerMock.Object, new TestHttpClientWrapper(error: true), this.credentialsFactoryMock.Object, this.azureResourceManagerClientMock.Object, Workspaces, TimeSpan.FromMinutes(10));
            try
            {
                await client.RunQueryAsync(Query, DataTimeSpan, default(CancellationToken));
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

        [TestMethod]
        public async Task WhenCrossTelemetryResourcesQueryIsSentThenResultsAreCombinedCorrectly()
        {
            var workspaceResourceIdToWorkspaceIdMapping = CreateWorkspaceResourceIdToWorkspaceIdMapping();

            Mock<IExtendedAzureResourceManagerClient> azureResourceManagerClientMock = new Mock<IExtendedAzureResourceManagerClient>();
            azureResourceManagerClientMock
            .Setup(m => m.GetLogAnalyticsWorkspaceIdAsync(It.IsAny<ResourceIdentifier>(), It.IsAny<CancellationToken>()))
            .Returns<ResourceIdentifier, CancellationToken>(
                (resource, token) =>
                    workspaceResourceIdToWorkspaceIdMapping.ContainsKey(resource) ?
                        Task.FromResult(workspaceResourceIdToWorkspaceIdMapping[resource]) :
                        throw new Microsoft.Rest.Azure.CloudException()
                        {
                            Response = new Microsoft.Rest.HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty)
                        });

            var clientWrapper = new CrossTelemetryResourcesTestHttpClientWrapper(workspaceResourceIdToWorkspaceIdMapping);
            var client = new LogAnalyticsTelemetryDataClient(this.tracerMock.Object, clientWrapper, this.credentialsFactoryMock.Object, azureResourceManagerClientMock.Object, workspaceResourceIdToWorkspaceIdMapping.Keys.Select(resource => resource.ToResourceId()), TimeSpan.FromMinutes(10));
            IList<DataTable> results = await client.RunQueryAsync(Query, DataTimeSpan, default(CancellationToken));

            // Verify results
            DataTable expectedResult = new DataTable("PrimaryResult");
            expectedResult.Columns.Add("Category", typeof(string));
            expectedResult.Columns.Add("count_", typeof(long));
            expectedResult.Columns.Add("Subcategory", typeof(string));

            expectedResult.Rows.Add("Administrative", 20839, null);
            expectedResult.Rows.Add("Recommendation", 122, null);
            expectedResult.Rows.Add("Alert", 64, null);
            expectedResult.Rows.Add("ServiceHealth", 11, null);
            expectedResult.Rows.Add("Administrative", 20839, null);
            expectedResult.Rows.Add("Benefits", 2, null);
            expectedResult.Rows.Add("Alert", 28, null);
            expectedResult.Rows.Add("Employers", 15, null);
            expectedResult.Rows.Add("Spendings", 15687456, "Contractors");
            expectedResult.Rows.Add("Actions", 122, null);
            expectedResult.Rows.Add("Alert", 28, "Sev1");
            expectedResult.Rows.Add("Employees", 889, null);

            VerifyDataTables(new List<DataTable> { expectedResult }, results);

            Assert.AreEqual(3, clientWrapper.CallsToSendAsync);
        }

        [TestMethod]
        public async Task WhenSomeOfTelemetryResourcesAreDeletedThenDeletedResourcesAreIgnoredAndResultsAreReturnedSuccessfully()
        {
            Dictionary<ResourceIdentifier, string> workspaceResourceIdToWorkspaceIdMapping = CreateWorkspaceResourceIdToWorkspaceIdMapping();

            Mock<IExtendedAzureResourceManagerClient> azureResourceManagerClientMock = new Mock<IExtendedAzureResourceManagerClient>();
            azureResourceManagerClientMock
            .Setup(m => m.GetLogAnalyticsWorkspaceIdAsync(It.IsAny<ResourceIdentifier>(), It.IsAny<CancellationToken>()))
            .Returns<ResourceIdentifier, CancellationToken>(
                (resource, token) =>
                    workspaceResourceIdToWorkspaceIdMapping.ContainsKey(resource) ?
                        Task.FromResult(workspaceResourceIdToWorkspaceIdMapping[resource]) :
                            throw new AzureResourceManagerClientException(HttpStatusCode.NotFound, string.Empty, null));

            azureResourceManagerClientMock
            .Setup(m => m.GetResourcePropertiesAsync(It.IsAny<ResourceIdentifier>(), It.IsAny<CancellationToken>()))
            .Returns<ResourceIdentifier, CancellationToken>(
                (resource, token) =>
                    workspaceResourceIdToWorkspaceIdMapping.ContainsKey(resource) ?
                        Task.FromResult<ResourceProperties>(null) :
                            throw new AzureResourceManagerClientException(HttpStatusCode.NotFound, string.Empty, null));

            var resourcesForRequest = workspaceResourceIdToWorkspaceIdMapping.Keys.Select(resource => resource.ToResourceId()).ToList();

            // Delete some workspaces
            workspaceResourceIdToWorkspaceIdMapping.Remove(new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace1"));
            workspaceResourceIdToWorkspaceIdMapping.Remove(new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace2"));
            workspaceResourceIdToWorkspaceIdMapping.Remove(new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace25"));
            workspaceResourceIdToWorkspaceIdMapping.Remove(new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace31"));

            var clientWrapper = new CrossTelemetryResourcesTestHttpClientWrapper(workspaceResourceIdToWorkspaceIdMapping);
            var client = new LogAnalyticsTelemetryDataClient(this.tracerMock.Object, clientWrapper, this.credentialsFactoryMock.Object, azureResourceManagerClientMock.Object, resourcesForRequest, TimeSpan.FromMinutes(10));
            IList<DataTable> results = await client.RunQueryAsync(Query, DataTimeSpan, default(CancellationToken));

            // Verify results
            DataTable expectedResult = new DataTable("PrimaryResult");
            expectedResult.Columns.Add("Category", typeof(string));
            expectedResult.Columns.Add("count_", typeof(long));

            expectedResult.Rows.Add("Administrative", 20839);
            expectedResult.Rows.Add("Recommendation", 122);
            expectedResult.Rows.Add("Alert", 64);
            expectedResult.Rows.Add("ServiceHealth", 11);
            expectedResult.Rows.Add("Administrative", 20839);
            expectedResult.Rows.Add("Benefits", 2);
            expectedResult.Rows.Add("Alert", 28);
            expectedResult.Rows.Add("Employers", 15);

            VerifyDataTables(new List<DataTable> { expectedResult }, results);

            Assert.AreEqual(3, clientWrapper.CallsToSendAsync);
        }

        private static Dictionary<ResourceIdentifier, string> CreateWorkspaceResourceIdToWorkspaceIdMapping()
        {
            return new Dictionary<ResourceIdentifier, string>
            {
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace1"), "8b75ff40-04bf-4350-890a-fda80020ee6d" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace2"), "546d187d-6959-491e-8578-c6a88e7002ea" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace3"), "da38cfd8-f1cf-4614-bb88-68d478d3fb86" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace4"), "f9888b01-2a3c-4342-a89d-3b4616a59a40" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace5"), "b809f137-4ac1-49e5-a11c-090c20c0d27d" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace6"), "0de4e377-ee08-4f79-9e55-6d19c1f74d60" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace7"), "240db491-527f-4381-beeb-30c04fc3271b" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace8"), "3d85bf04-ba53-4822-b657-e9dc83d9063a" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace9"), "9a84e3c2-5c1b-4d34-a2f8-0617524f0d10" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace10"), "f7a98119-a886-41ea-925c-9e7d224c556a" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace11"), "d534bff3-ffc7-4277-8b0f-dbd7935ac594" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace12"), "e2b48cc9-3e5f-4c8f-be9b-4b4dec50fa06" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace13"), "0d84d17d-4eb5-40c1-b878-10bbf3d5de56" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace14"), "e5896fae-bf4a-4f99-8ade-aa761c3b4f86" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace15"), "8cadc160-31ff-414c-9655-ed6f8911cf14" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace16"), "890564a1-1830-45a8-9d27-c1c9913286db" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace17"), "d85f8357-b64a-490b-b230-51ab271c5bd6" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace18"), "a9eeb483-6dcb-47a1-b7ce-b9814420e192" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace19"), "6f0b3bda-31ba-4297-ad6b-eee3ae82ae22" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace20"), "81bf7a69-1fe9-4cf7-a6e5-6029ffc5931f" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace21"), "5e2f3905-d5cb-45f1-bf90-9f4c72691747" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace22"), "52dcbcf7-2f82-48ca-b18f-ecda27c7abfa" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace23"), "063ddd09-f236-49a3-9974-a0da0f7bd47f" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace24"), "01317ed1-c007-47a7-9c94-5cd365377229" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace25"), "6437c891-8c9e-4786-a213-f67336ffb273" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace26"), "fd621344-c489-434b-ba80-62942f7ecfaa" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace27"), "53a5299a-cd9f-447b-8c47-c90829a2e62f" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace28"), "feb631d4-4d47-4390-8992-0520e2d965cd" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace29"), "2ffe744c-abbf-4dba-9a9d-0299b2305064" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace30"), "471c5edc-a7cd-4137-830a-189aa67bddda" },
                { new ResourceIdentifier(ResourceType.LogAnalytics, "f0b2be5e-9cfa-489e-b627-50a216a18c93", "someGroup", "workspace31"), "d219080d-2039-4cc7-b2ee-8a472a4461d6" }
            };
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
            private readonly bool applicationInsights;
            private readonly bool error;
            private readonly bool expectTimeSpan;

            public TestHttpClientWrapper(bool invalidType = false, bool emptyResults = false, bool applicationInsights = false, bool error = false, bool expectTimeSpan = true)
            {
                this.invalidType = invalidType;
                this.emptyResults = emptyResults;
                this.applicationInsights = applicationInsights;
                this.error = error;
                this.expectTimeSpan = expectTimeSpan;
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
                string expectedUriPrefix = this.applicationInsights ? $"https://api.applicationinsights.io/v1/apps/" : $"https://api.loganalytics.io/v1/workspaces/";
                Assert.IsTrue(request.RequestUri.ToString().StartsWith(expectedUriPrefix, StringComparison.InvariantCulture), "Request URI prefix mismatch");
                Assert.IsTrue(request.RequestUri.ToString().EndsWith("/query", StringComparison.InvariantCulture), "Request URI suffix mismatch");
                Assert.AreEqual(3, request.Headers.Count(), "Request headers count mismatch");
                Assert.AreEqual("wait=600, ai.include-error-payload=true", request.Headers.GetValues("Prefer").First(), "Prefer header mismatch");
                Assert.AreEqual("SmartAlertsRuntime", request.Headers.GetValues("x-ms-app").First(), "Draft app name header mismatch");
                Assert.IsTrue(request.Headers.Contains("x-ms-client-request-id"), "Draft request ID header missing");

                JObject expectedContent = new JObject() { ["query"] = Query };
                if (this.expectTimeSpan == true)
                {
                    expectedContent["timespan"] = System.Xml.XmlConvert.ToString(DataTimeSpan);
                }

                var additionalTelemetryResources = (this.applicationInsights ? Applications : Workspaces).Skip(1).ToList();
                if (additionalTelemetryResources.Count > 0)
                {
                    expectedContent.Add(this.applicationInsights ? "applications" : "workspaces", new JArray(additionalTelemetryResources));
                }

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

        private class CrossTelemetryResourcesTestHttpClientWrapper : IHttpClientWrapper
        {
            private readonly Dictionary<ResourceIdentifier, string> telemetryResourceIdToTelemetryIdMapping;

            public CrossTelemetryResourcesTestHttpClientWrapper(Dictionary<ResourceIdentifier, string> telemetryResourceIdToTelemetryIdMapping)
            {
                this.telemetryResourceIdToTelemetryIdMapping = telemetryResourceIdToTelemetryIdMapping;
            }

            public int CallsToSendAsync { get; private set; } = 0;

            public TimeSpan Timeout { get; set; }

            public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, TimeSpan? timeout, CancellationToken cancellationToken)
            {
                this.CallsToSendAsync++;

                JObject content = (JObject)JsonConvert.DeserializeObject(await request.Content.ReadAsStringAsync());

                string extractedTelemetryId = request.RequestUri.ToString().Split('/')[5];

                List<string> telemetryResources = new List<string>();
                if (content.ContainsKey("applications"))
                {
                    telemetryResources.AddRange((content["applications"] as JArray).Values<string>());
                }
                else if (content.ContainsKey("workspaces"))
                {
                    telemetryResources.AddRange((content["workspaces"] as JArray).Values<string>());
                }

                // Prepare error response
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

                var errorResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(errorObject.ToString())
                };

                // Return error if one of resources is missing, or if the telemetry Id in the URL exists in the list
                if (telemetryResources.Any(resource => this.telemetryResourceIdToTelemetryIdMapping.ContainsKey(ResourceIdentifier.CreateFromResourceId(resource)) == false)
                    || telemetryResources.Any(resource => resource == extractedTelemetryId))
                {
                    return errorResponse;
                }

                telemetryResources = telemetryResources.Select(resource => this.telemetryResourceIdToTelemetryIdMapping[ResourceIdentifier.CreateFromResourceId(resource)]).ToList();
                if (telemetryResources.Contains(extractedTelemetryId))
                {
                    return errorResponse;
                }

                telemetryResources.Add(extractedTelemetryId);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(this.MergeJObjectTables(telemetryResources.Select(GetExpectedResultForWorkspaceAsJObject)).ToString())
                };
            }

            private static JObject GetExpectedResultForWorkspaceAsJObject(string telemetryId)
            {
                if (telemetryId == "da38cfd8-f1cf-4614-bb88-68d478d3fb86")
                {
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
                            }
                        }
                    };
                }
                else if (telemetryId == "3d85bf04-ba53-4822-b657-e9dc83d9063a")
                {
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
                                        "Benefits",
                                        2
                                    },
                                    new JArray()
                                    {
                                        "Alert",
                                        28
                                    },
                                    new JArray()
                                    {
                                        "Employers",
                                        15
                                    }
                                }
                            }
                        }
                    };
                }
                else if (telemetryId == "d219080d-2039-4cc7-b2ee-8a472a4461d6")
                {
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
                                    },
                                    new JObject()
                                    {
                                        ["name"] = "Subcategory",
                                        ["type"] = "string"
                                    }
                                },
                                ["rows"] = new JArray
                                {
                                    new JArray()
                                    {
                                        "Spendings",
                                        15687456,
                                        "Contractors"
                                    },
                                    new JArray()
                                    {
                                        "Actions",
                                        122,
                                        null
                                    },
                                    new JArray()
                                    {
                                        "Alert",
                                        28,
                                        "Sev1"
                                    },
                                    new JArray()
                                    {
                                        "Employees",
                                        889,
                                        null
                                    }
                                }
                            }
                        }
                    };
                }
                else
                {
                    return new JObject()
                    {
                        ["tables"] = new JArray()
                    };
                }
            }

            private JObject MergeJObjectTables(IEnumerable<JObject> jobjectTables)
            {
                var combinedRows = new JArray();
                var combinedColumn = new JArray();
                foreach (var table in jobjectTables)
                {
                    var tables = table["tables"] as JArray;
                    if (tables.Count > 0)
                    {
                        var primaryTable = tables[0] as JObject;
                        foreach (var row in primaryTable["rows"] as JArray)
                        {
                            combinedRows.Add(row);
                        }

                        foreach (var column in primaryTable["columns"] as JArray)
                        {
                            if (combinedColumn.Select(col => col["name"]).Contains(column["name"]) == false)
                            {
                                combinedColumn.Add(column);
                            }
                        }
                    }
                }

                return new JObject()
                {
                    ["tables"] = new JArray()
                    {
                        new JObject()
                        {
                            ["name"] = "PrimaryResult",
                            ["columns"] = combinedColumn,
                            ["rows"] = combinedRows
                        }
                    }
                };
            }
        }
    }
}
