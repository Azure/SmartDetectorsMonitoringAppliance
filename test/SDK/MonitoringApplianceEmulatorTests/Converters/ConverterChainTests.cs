//-----------------------------------------------------------------------
// <copyright file="ConverterChainTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Converters
{
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows.Data;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConverterChainTests
    {
        [TestMethod]
        public void WhenConvertingWithChainedConvertersThenResultWasConvertedByEachConverterInTheExpectedOrder()
        {
            var emptyStringToBooleanConverter = new EmptyStringToBooleanConverter();
            var inverseBooleanConverter = new InverseBooleanConverter();

            var convertersChain = new ConverterChain(new Collection<IValueConverter>() { emptyStringToBooleanConverter, inverseBooleanConverter });

            object result = convertersChain.Convert("just a string", typeof(bool), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(result, true);
        }
    }
}
