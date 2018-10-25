//-----------------------------------------------------------------------
// <copyright file="EmptyListToBooleanConverterTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Converters
{
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EmptyListToBooleanConverterTests
    {
        [TestMethod]
        public void WhenConvertingEmptyListThenResultIsFalse()
        {
            var emptyListToBooleanConverter = new EmptyListToBooleanConverter();

            var emptyList = new List<string>();
            object result = emptyListToBooleanConverter.Convert(emptyList, typeof(bool), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public void WhenConvertingNotEmptyListThenResultIsTrue()
        {
            var emptyListToBooleanConverter = new EmptyListToBooleanConverter();

            var list = new List<string>() { "I am not your toy", "you stupid boy" };
            object result = emptyListToBooleanConverter.Convert(list, typeof(bool), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(result, true);
        }
    }
}
