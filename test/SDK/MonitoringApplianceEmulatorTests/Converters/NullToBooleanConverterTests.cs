//-----------------------------------------------------------------------
// <copyright file="NullToBooleanConverterTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Converters
{
    using System.Globalization;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NullToBooleanConverterTests
    {
        [TestMethod]
        public void WhenConvertingNullThenResultIsFalse()
        {
            var nullToBooleanConverter = new NullToBooleanConverter();

            object result = nullToBooleanConverter.Convert(null, typeof(bool), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public void WhenConvertingNonNullValueThenResultIsTrue()
        {
            var nullToBooleanConverter = new NullToBooleanConverter();

            object result = nullToBooleanConverter.Convert("for sure I am not null", typeof(bool), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(result, true);
        }
    }
}
