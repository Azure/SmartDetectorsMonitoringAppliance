//-----------------------------------------------------------------------
// <copyright file="InverseBooleanConverterTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Converters
{
    using System.Globalization;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InverseBooleanConverterTests
    {
        [TestMethod]
        public void WhenConvertingTrueThenResultIsFalse()
        {
            var inverseBooleanConverter = new InverseBooleanConverter();

            object result = inverseBooleanConverter.Convert(true, typeof(bool), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public void WhenConvertingFalseThenResultIsTrue()
        {
            var inverseBooleanConverter = new InverseBooleanConverter();

            object result = inverseBooleanConverter.Convert(false, typeof(bool), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(result, true);
        }
    }
}
