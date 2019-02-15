//-----------------------------------------------------------------------
// <copyright file="ExtendedMetricClientTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsSharedTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.Clients;
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.Trace;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ExtendedMetricClientTests
    {
        [Ignore]
        [TestMethod]
        public async Task WhenSendingMetricQueryThenTheResultsAreAsExpected()
        {
            // Set real values in resourceIdentifier to run this test
            var resourceIdentifier = new ResourceIdentifier(
                ResourceType.AzureStorage,
                subscriptionId: "SUBSCRIPTION_ID",
                resourceGroupName: "RESOURCE_GROUP_NAME",
                resourceName: "STORAGE_NAME");

            // Authenticate
            var authenticationServices = new AuthenticationServices();
            authenticationServices.AuthenticateUser();
            ICredentialsFactory credentialsFactory = new ActiveDirectoryCredentialsFactory(authenticationServices);

            var resourceId = $"/subscriptions/{resourceIdentifier.SubscriptionId}/resourceGroups/{resourceIdentifier.ResourceGroupName}/providers/Microsoft.Storage/storageAccounts/{resourceIdentifier.ResourceName}/queueServices/default";
            var tracerMock = new Mock<IExtendedTracer>();
            ExtendedMetricClient client = new ExtendedMetricClient(tracerMock.Object, credentialsFactory);

            var parameters = new QueryParameters()
            {
                StartTime = DateTime.UtcNow.Date.AddDays(-1),
                EndTime = DateTime.UtcNow.Date,
                Aggregations = new List<Aggregation> { Aggregation.Total },
                MetricNames = new List<string>() { "QueueMessageCount" },
                Interval = TimeSpan.FromMinutes(60)
            };

            var metrics1 = (await client.GetResourceMetricsAsync(resourceId, parameters, default(CancellationToken))).ToList();
            var metrics2 = (await client.GetResourceMetricsAsync(resourceIdentifier, ServiceType.AzureStorageQueue, parameters, default(CancellationToken))).ToList();
            Assert.IsTrue(metrics1.Any() && metrics2.Any(), "Lists are not full with data");
            Assert.IsTrue(metrics1.First().Timeseries.Any() && metrics2.First().Timeseries.Any(), "Metrics do not contain Time series");
            Assert.IsTrue(metrics1[0].Timeseries[0].Data.Any() && metrics2[0].Timeseries[0].Data.Any(), "Time series are not full with data");
        }
    }
}
