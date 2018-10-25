//-----------------------------------------------------------------------
// <copyright file="TimeSpanExtensionsTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Extensions
{
    using System;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TimeSpanExtensionsTests
    {
        [TestMethod]
        public void WhenConvertingTimeSpanToReadableStringThenConversionResultIsTheCorrectString()
        {
            TimeSpan firstSpan = new TimeSpan(days: 0, hours: 1, minutes: 27, seconds: 0);
            Assert.AreEqual("1 hour, 27 minutes", firstSpan.ToReadableString());

            TimeSpan secondSpan = new TimeSpan(days: 3, hours: 18, minutes: 52, seconds: 4);
            Assert.AreEqual("3 days, 18 hours, 52 minutes, 4 seconds", secondSpan.ToReadableString());

            TimeSpan thirdSpan = new TimeSpan(days: 4, hours: 0, minutes: 0, seconds: 0);
            Assert.AreEqual("4 days", thirdSpan.ToReadableString());

            TimeSpan forthSpan = new TimeSpan(days: 0, hours: 0, minutes: 1, seconds: 0);
            Assert.AreEqual("1 minute", forthSpan.ToReadableString());
        }
    }
}
