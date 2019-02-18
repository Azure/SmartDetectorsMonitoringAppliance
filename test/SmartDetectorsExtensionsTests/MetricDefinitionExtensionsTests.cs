//-----------------------------------------------------------------------
// <copyright file="MetricDefinitionExtensionsTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartDetectorsExtensionsTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Management.Monitor.Fluent.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MetricDefinition = Microsoft.Azure.Monitoring.SmartDetectors.Metric.MetricDefinition;

    [TestClass]
    public class MetricDefinitionExtensionsTests
    {
        [TestMethod]
        public void WhenConvertingToSmartDetectorsMetricDefinitionThenItWasConvertedCorrectly()
        {
            var definition1 = new Microsoft.Azure.Management.Monitor.Fluent.Models.MetricDefinition(
                isDimensionRequired: true,
                resourceId: "resourceId",
                name: new LocalizableString("StorageSize"),
                unit: Unit.Bytes,
                primaryAggregationType: AggregationType.Maximum,
                metricAvailabilities: new List<MetricAvailability>()
                {
                    new MetricAvailability(TimeSpan.FromMinutes(60), TimeSpan.FromDays(30)),
                    new MetricAvailability(TimeSpan.FromMinutes(1), null),
                },
                id: "id1",
                dimensions: new List<LocalizableString>()
                {
                    new LocalizableString("dim1"),
                    new LocalizableString("dim2"),
                    new LocalizableString("dim3"),
                });

            VerifyMetricDefinitionConversion(definition1, definition1.ConvertToSmartDetectorsMetricDefinition());

            var definition2 = new Microsoft.Azure.Management.Monitor.Fluent.Models.MetricDefinition(
                isDimensionRequired: null,
                resourceId: "resourceId",
                name: new LocalizableString("StorageLatency"),
                unit: Unit.MilliSeconds,
                primaryAggregationType: null,
                metricAvailabilities: new List<MetricAvailability>()
                {
                    new MetricAvailability(TimeSpan.FromMinutes(30), TimeSpan.FromDays(20))
                },
                id: "id2",
                dimensions: new List<LocalizableString>());

            VerifyMetricDefinitionConversion(definition2, definition2.ConvertToSmartDetectorsMetricDefinition());
        }

        private static void VerifyMetricDefinitionConversion(
            Microsoft.Azure.Management.Monitor.Fluent.Models.MetricDefinition originalDefinition,
            MetricDefinition resultDefinition)
        {
            Assert.AreEqual(originalDefinition.Name.Value, resultDefinition.Name, "Metric name isn't correct");
            Assert.IsTrue(originalDefinition.MetricAvailabilities.Select(x => Tuple.Create(x.Retention, x.TimeGrain)).SequenceEqual(resultDefinition.Availabilities), "Metric availabilities aren't correct");
            Assert.IsTrue(originalDefinition.Dimensions.Select(x => x.Value).SequenceEqual(resultDefinition.Dimensions), "Metric dimensions aren't correct");
            Assert.AreEqual(originalDefinition.IsDimensionRequired, resultDefinition.IsDimensionRequired, "Metric is dimension required isn't correct");
            Assert.AreEqual(originalDefinition.PrimaryAggregationType?.ToString(), resultDefinition.PrimaryAggregationType?.ToString(), "Metric primary aggregation type isn't correct");
            Assert.AreEqual(originalDefinition.Unit?.ToString(), resultDefinition.Unit, "Metric unit isn't correct");
        }
    }
}
