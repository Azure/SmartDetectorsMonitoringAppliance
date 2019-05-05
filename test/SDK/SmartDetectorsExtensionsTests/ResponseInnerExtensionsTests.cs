//-----------------------------------------------------------------------
// <copyright file="ResponseInnerExtensionsTests.cs" company="Microsoft Corporation">
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
    using Microsoft.Azure.Monitoring.SmartDetectors.Metric;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ResponseInnerExtensionsTests
    {
        [TestMethod]
        public void WhenConvertingToSmartDetectorsMetricQueryResultThenItWasConvertedCorrectly()
        {
            var timestamp = DateTime.UtcNow;
            var responseInner = new ResponseInner("timespan", GetMetricList(timestamp));

            List<MetricQueryResult> metrics = responseInner.ToMetricQueryResult().ToList();

            // Validate first metric was converted successfully
            Assert.AreEqual("MetricName1", metrics[0].Name, "First metric name isn't correct");
            Assert.AreEqual("ByteSeconds", metrics[0].Unit, "First metric unit isn't correct");
            Assert.AreEqual(0, metrics[0].Timeseries.Count, "First metric timeseries should be empty");

            // Validate second metric was converted successfully
            Assert.AreEqual("MetricName2", metrics[1].Name, "Second metric name isn't correct");
            Assert.AreEqual("MilliSeconds", metrics[1].Unit, "Second metric unit isn't correct");
            Assert.AreEqual(5, metrics[1].Timeseries.Count, "Second metric timeseries should be empty");
            Assert.AreEqual(2, metrics[1].Timeseries[0].Data.Count, "Second metric first timeseries (Dimension1Value1) length should be 2");
            Assert.AreEqual(2, metrics[1].Timeseries[1].Data.Count, "Second metric second timeseries (Dimension1Value2) length should be 2");
            Assert.AreEqual(1.1, metrics[1].Timeseries[0].Data[0].Average, "Second metric first timeseries first average is wrong");
            Assert.AreEqual(1.0, metrics[1].Timeseries[0].Data[1].Average, "Second metric first timeseries second average is wrong");
            Assert.AreEqual(timestamp.AddMinutes(-1), metrics[1].Timeseries[0].Data[0].TimeStamp, "Second metric first timeseries first timestamp is wrong");
            Assert.IsNull(metrics[1].Timeseries[0].Data[0].Total, "Second metric first timeseries first total should be null");
            Assert.IsNull(metrics[1].Timeseries[0].Data[0].Maximum, "Second metric first timeseries first maximum should be null");
            Assert.IsNull(metrics[1].Timeseries[0].Data[0].Minimum, "Second metric first timeseries first minimum should be null");
            Assert.IsNull(metrics[1].Timeseries[0].Data[0].Count, "Second metric first timeseries first count should be null");

            Assert.AreEqual(2.1, metrics[1].Timeseries[1].Data[0].Minimum, "Second metric second timeseries first minimum is wrong");
            Assert.AreEqual(3.1, metrics[1].Timeseries[2].Data[0].Maximum, "Second metric third timeseries first maximum is wrong");
            Assert.AreEqual(4.1, metrics[1].Timeseries[3].Data[0].Total, "Second metric forth timeseries first total is wrong");
            Assert.AreEqual(1, metrics[1].Timeseries[4].Data[0].Count, "Second metric fifth timeseries first count is wrong");

            Assert.AreEqual(1, metrics[1].Timeseries[0].MetaData.Count, "Second metric first timeseries metadata length is wrong");
            Assert.AreEqual("Dimension1", metrics[1].Timeseries[0].MetaData[0].Key, "Second metric first timeseries first metadata key is wrong");
            Assert.AreEqual("Dimension1Value1", metrics[1].Timeseries[0].MetaData[0].Value, "Second metric first timeseries first metadata value is wrong");
        }

        /// <summary>
        /// Returns a synthetic metric list for test purposes
        /// </summary>
        /// <param name="timestamp">The time stamp to be used in the metric's timeseries</param>
        /// <returns>A synthetic metric list for test purposes</returns>
        private static List<Metric> GetMetricList(DateTime timestamp)
        {
            return new List<Metric>
            {
                new Metric()
                {
                    Id = "MetricId1",
                    Name = new LocalizableString("MetricName1"),
                    Unit = Unit.ByteSeconds,
                    Type = "MetricType1",
                },
                new Metric()
                {
                    Id = "MetricId2",
                    Name = new LocalizableString("MetricName2"),
                    Unit = Unit.MilliSeconds,
                    Type = "MetricType2",
                    Timeseries = new List<TimeSeriesElement>()
                    {
                        new TimeSeriesElement()
                        {
                            Metadatavalues = new List<MetadataValueInner>()
                            {
                                { new MetadataValueInner(new LocalizableString("Dimension1"), "Dimension1Value1") },
                            },
                            Data = new List<MetricValue>()
                            {
                                { new MetricValue(timestamp.AddMinutes(-1), average: 1.1) },
                                { new MetricValue(timestamp.AddMinutes(-2), average: 1.0) },
                            }
                        },
                        new TimeSeriesElement()
                        {
                            Metadatavalues = new List<MetadataValueInner>()
                            {
                                { new MetadataValueInner(new LocalizableString("Dimension1"), "Dimension1Value2") },
                            },
                            Data = new List<MetricValue>()
                            {
                                { new MetricValue(timestamp.AddMinutes(-1), minimum: 2.1) },
                                { new MetricValue(timestamp.AddMinutes(-2), minimum: 2.0) },
                            }
                        },
                        new TimeSeriesElement()
                        {
                            Metadatavalues = new List<MetadataValueInner>()
                            {
                                { new MetadataValueInner(new LocalizableString("Dimension1"), "Dimension1Value3") },
                            },
                            Data = new List<MetricValue>()
                            {
                                { new MetricValue(timestamp.AddMinutes(-1), maximum: 3.1) },
                                { new MetricValue(timestamp.AddMinutes(-2), maximum: 3.0) },
                            }
                        },
                        new TimeSeriesElement()
                        {
                            Metadatavalues = new List<MetadataValueInner>()
                            {
                                { new MetadataValueInner(new LocalizableString("Dimension1"), "Dimension1Value4") },
                            },
                            Data = new List<MetricValue>()
                            {
                                { new MetricValue(timestamp.AddMinutes(-1), total: 4.1) },
                                { new MetricValue(timestamp.AddMinutes(-2), total: 4.0) },
                            }
                        },
                        new TimeSeriesElement()
                        {
                            Metadatavalues = new List<MetadataValueInner>()
                            {
                                { new MetadataValueInner(new LocalizableString("Dimension1"), "Dimension1Value5") },
                            },
                            Data = new List<MetricValue>()
                            {
                                { new MetricValue(timestamp.AddMinutes(-1), count: 1) },
                                { new MetricValue(timestamp.AddMinutes(-2), count: 5) },
                            }
                        },
                    }
                }
            };
        }
    }
}
