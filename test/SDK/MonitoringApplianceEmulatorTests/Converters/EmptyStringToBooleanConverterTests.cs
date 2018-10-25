//-----------------------------------------------------------------------
// <copyright file="EmptyStringToBooleanConverterTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Converters
{
    using System.Globalization;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EmptyStringToBooleanConverterTests
    {
        [TestMethod]
        public void WhenConvertingEmptyStringThenResultIsTrue()
        {
            var emptyStringToBooleanConverter = new EmptyStringToBooleanConverter();

            object result = emptyStringToBooleanConverter.Convert(string.Empty, typeof(bool), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public void WhenConvertingNonEmptyListThenResultIsFalse()
        {
            var emptyStringToBooleanConverter = new EmptyStringToBooleanConverter();

            object result = emptyStringToBooleanConverter.Convert("I am not your toy, you stupid boy", typeof(bool), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(result, false);
        }
    }
}
