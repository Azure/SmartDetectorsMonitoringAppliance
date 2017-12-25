//-----------------------------------------------------------------------
// <copyright file="TimeRangeTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalsSDKTests
{
    using System;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TimeRangeTests
    {
        #region Error cases

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenCreatingTimeRangeWithEndTimeBeforeStartThenExceptionIsThrown()
        {
            DateTime startTime = DateTime.Now;
            var unused = new TimeRange(startTime, startTime.AddTicks(-1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenCreatingTimeRangeNegativeDurationThenExceptionIsThrown()
        {
            var unused = new TimeRange(DateTime.Now, TimeSpan.FromTicks(-1));
        }

        #endregion

        #region Constructors tests

        [TestMethod]
        public void WhenCreatingTimeRangeWithEqualStartAndEndTimeThenDurationIsComputedCorrectly()
        {
            DateTime startTime = new DateTime(1976, 7, 6);

            var timeRange = new TimeRange(startTime, startTime);
            Assert.AreEqual(startTime, timeRange.StartTime, "Mismatch on time range start time");
            Assert.AreEqual(startTime, timeRange.EndTime, "Mismatch on time range end time");
            Assert.AreEqual(TimeSpan.Zero, timeRange.Duration, "Mismatch on time range duration");
        }

        [TestMethod]
        public void WhenCreatingTimeRangeWithDifferentStartAndEndTimeThenDurationIsComputedCorrectly()
        {
            DateTime startTime = new DateTime(1976, 7, 6);
            DateTime endTime = new DateTime(1976, 7, 6, 12, 0, 0);

            var timeRange = new TimeRange(startTime, endTime);
            Assert.AreEqual(startTime, timeRange.StartTime, "Mismatch on time range start time");
            Assert.AreEqual(endTime, timeRange.EndTime, "Mismatch on time range end time");
            Assert.AreEqual(TimeSpan.FromHours(12), timeRange.Duration, "Mismatch on time range duration");
        }

        [TestMethod]
        public void WhenCreatingTimeRangeWithVeryDifferentStartAndEndTimeThenDurationIsComputedCorrectly()
        {
            TimeSpan expectedDuration = DateTime.MaxValue - DateTime.MinValue; 
            var timeRange = new TimeRange(DateTime.MinValue, DateTime.MaxValue);
            Assert.AreEqual(DateTime.MinValue, timeRange.StartTime, "Mismatch on time range start time");
            Assert.AreEqual(DateTime.MaxValue, timeRange.EndTime, "Mismatch on time range end time");
            Assert.AreEqual(expectedDuration, timeRange.Duration, "Mismatch on time range duration");
        }

        [TestMethod]
        public void WhenCreatingTimeRangeWithZeroDurationThenEndTimeIsComputedCorrectly()
        {
            DateTime startTime = new DateTime(1976, 7, 6);

            var timeRange = new TimeRange(startTime, TimeSpan.Zero);
            Assert.AreEqual(startTime, timeRange.StartTime, "Mismatch on time range start time");
            Assert.AreEqual(startTime, timeRange.EndTime, "Mismatch on time range end time");
            Assert.AreEqual(TimeSpan.Zero, timeRange.Duration, "Mismatch on time range duration");
        }

        [TestMethod]
        public void WhenCreatingTimeRangeWithMonZeroDurationThenEndTimeIsComputedCorrectly()
        {
            DateTime startTime = new DateTime(1976, 7, 6);
            DateTime expectedEndTime = new DateTime(1976, 7, 6, 8, 0, 0);

            var timeRange = new TimeRange(startTime, TimeSpan.FromHours(8));
            Assert.AreEqual(startTime, timeRange.StartTime, "Mismatch on time range start time");
            Assert.AreEqual(expectedEndTime, timeRange.EndTime, "Mismatch on time range end time");
            Assert.AreEqual(TimeSpan.FromHours(8), timeRange.Duration, "Mismatch on time range duration");
        }

        [TestMethod]
        public void WhenCreatingTimeRangeWithMonVeryLargeDurationThenEndTimeIsComputedCorrectly()
        {
            TimeSpan duration = DateTime.MaxValue - DateTime.MinValue;
            var timeRange = new TimeRange(DateTime.MinValue, duration);
            Assert.AreEqual(DateTime.MinValue, timeRange.StartTime, "Mismatch on time range start time");
            Assert.AreEqual(DateTime.MaxValue, timeRange.EndTime, "Mismatch on time range end time");
            Assert.AreEqual(duration, timeRange.Duration, "Mismatch on time range duration");
        }

        #endregion

        #region Equality tests

        [TestMethod]
        public void WhenComparingTwoTimeRangesCreatedWithTheSameStartAndEndTimesThenTheyAreEqual()
        {
            DateTime startTime = new DateTime(2017, 11, 29);
            DateTime endTime = new DateTime(2018, 11, 29);
            TimeRange first = new TimeRange(startTime, endTime);
            TimeRange second = new TimeRange(startTime, endTime);

            Assert.IsTrue(first.Equals(second), "Expected both ranges to be equal");
            Assert.IsTrue(first == second, "Expected both ranges to be equal using equality comparison");
            Assert.IsFalse(first != second, "Expected both ranges to be equal using inequality comparison");
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode(), "Expected both ranges have equal hash codes");
        }

        [TestMethod]
        public void WhenComparingTwoTimeRangesCreatedWithTheSameStartTimeAndDurationThenTheyAreEqual()
        {
            DateTime startTime = new DateTime(2017, 11, 29);
            TimeSpan duration = TimeSpan.FromMinutes(53);
            TimeRange first = new TimeRange(startTime, duration);
            TimeRange second = new TimeRange(startTime, duration);

            Assert.IsTrue(first.Equals(second), "Expected both ranges to be equal");
            Assert.IsTrue(first == second, "Expected both ranges to be equal using equality comparison");
            Assert.IsFalse(first != second, "Expected both ranges to be equal using inequality comparison");
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode(), "Expected both ranges have equal hash codes");
        }

        [TestMethod]
        public void WhenComparingTwoTimeRangesCreatedWithDifferentStartAndEndTimesThenTheyAreNotEqual()
        {
            DateTime startTime = new DateTime(2017, 11, 29);
            DateTime endTime = new DateTime(2018, 11, 29);
            TimeRange first = new TimeRange(startTime, endTime);
            TimeRange second = new TimeRange(startTime, endTime.AddTicks(1));

            Assert.IsFalse(first.Equals(second), "Expected both ranges to be not equal");
            Assert.IsFalse(first == second, "Expected both ranges to be not equal using equality comparison");
            Assert.IsTrue(first != second, "Expected both ranges to be not equal using inequality comparison");
            Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode(), "Expected both ranges have not equal hash codes");
        }

        [TestMethod]
        public void WhenComparingTwoTimeRangesCreatedWithDifferentStartTimeAndDurationThenTheyAreNotEqual()
        {
            DateTime startTime = new DateTime(2017, 11, 29);
            TimeSpan duration = TimeSpan.FromMinutes(53);
            TimeRange first = new TimeRange(startTime, duration);
            TimeRange second = new TimeRange(startTime, duration + TimeSpan.FromTicks(-1));

            Assert.IsFalse(first.Equals(second), "Expected both ranges to be not equal");
            Assert.IsFalse(first == second, "Expected both ranges to be not equal using equality comparison");
            Assert.IsTrue(first != second, "Expected both ranges to be not equal using inequality comparison");
            Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode(), "Expected both ranges have not equal hash codes");
        }

        #endregion
    }
}
