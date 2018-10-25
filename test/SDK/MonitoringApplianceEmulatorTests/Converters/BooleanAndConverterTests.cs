//-----------------------------------------------------------------------
// <copyright file="BooleanAndConverterTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Converters
{
    using System.Globalization;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BooleanAndConverterTests
    {
        [TestMethod]
        public void WhenConvertingOnlyTrueValuesThenResultIsTrue()
        {
            var booleanAndConverter = new BooleanAndConverter();

            object[] values = { true, true, true };
            object result = booleanAndConverter.Convert(values, typeof(bool), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public void WhenConvertingTrueAndFalseValuesThenResultIsFalse()
        {
            var booleanAndConverter = new BooleanAndConverter();

            object[] values = { true, false, true };
            object result = booleanAndConverter.Convert(values, typeof(bool), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(result, false);
        }
    }
}
