//-----------------------------------------------------------------------
// <copyright file="CountToVisibilityConverterTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Converters
{
    using System.Globalization;
    using System.Windows;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CountToVisibilityConverterTests
    {
        [TestMethod]
        public void WhenConvertingPositiveNumberThenResultIsVisible()
        {
            var countToVisibilityConverter = new CountToVisibilityConverter();

            object result = countToVisibilityConverter.Convert(2, typeof(int), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(Visibility));
            Assert.AreEqual(Visibility.Visible, result);
        }

        [TestMethod]
        public void WhenConvertingZeroThenResultIsCollapsed()
        {
            var countToVisibilityConverter = new CountToVisibilityConverter();

            object result = countToVisibilityConverter.Convert(0, typeof(int), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(Visibility));
            Assert.AreEqual(Visibility.Collapsed, result);
        }

        [TestMethod]
        public void WhenConvertingZeroWithVisibilityParametersThenResultIsAsDefinedInParameters()
        {
            var countToVisibilityConverter = new CountToVisibilityConverter();

            object result = countToVisibilityConverter.Convert(0, typeof(int), $"{Visibility.Visible}, {Visibility.Hidden}", new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(Visibility));
            Assert.AreEqual(Visibility.Hidden, result);
        }
    }
}
