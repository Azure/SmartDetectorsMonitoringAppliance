//-----------------------------------------------------------------------
// <copyright file="ResourceTypeToIconConverterTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Converters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Converters;
    using Microsoft.Azure.Monitoring.SmartDetectors.RuntimeEnvironment.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ResourceTypeToIconConverterTests
    {
        private ResourceTypeToIconConverter resourceTypeToIconConverter = new ResourceTypeToIconConverter();

        [TestInitialize]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "Test code, allowed")]
        public void Init()
        {
            // Register the 'pack' port if needed - only way to do it is to create an Application instance.
            if (!UriParser.IsKnownScheme("pack"))
            {
                new Application();
            }

            // Set the assembly resources to be like in runtime
            if (Application.ResourceAssembly == null)
            {
                Application.ResourceAssembly = typeof(MainWindow).Assembly;
            }
        }

        [TestMethod]
        public void WhenConvertingSubscriptionThenResultIsSubscriptionIcon()
        {
            this.TestConverter(ResourceType.Subscription, new BitmapImage(new Uri("pack://application:,,,/Media/subscription.png")));
        }

        [TestMethod]
        public void WhenConvertingResourceGroupThenResultIsResourceGroupIcon()
        {
            this.TestConverter(ResourceType.ResourceGroup, new BitmapImage(new Uri("pack://application:,,,/Media/resource_group.png")));
        }

        [TestMethod]
        public void WhenConvertingStorageThenResultIsStorageIcon()
        {
            this.TestConverter(ResourceType.AzureStorage, new BitmapImage(new Uri("pack://application:,,,/Media/storage.png")));
        }

        [TestMethod]
        public void WhenConvertingLogAnalyticsThenResultIsLogAnalyticsIcon()
        {
            this.TestConverter(ResourceType.LogAnalytics, new BitmapImage(new Uri("pack://application:,,,/Media/log_analytics.png")));
        }

        [TestMethod]
        public void WhenConvertingAppInsightsThenResultIsAppInsightsIcon()
        {
            this.TestConverter(ResourceType.ApplicationInsights, new BitmapImage(new Uri("pack://application:,,,/Media/app_insights.png")));
        }

        [TestMethod]
        public void WhenConvertingVirtualMachineThenResultIsVirtualMachineIcon()
        {
            this.TestConverter(ResourceType.VirtualMachine, new BitmapImage(new Uri("pack://application:,,,/Media/virtual_machine.png")));
        }

        [TestMethod]
        public void WhenConvertingVirtualMachineSetThenResultIsVirtualMachineSetIcon()
        {
            this.TestConverter(ResourceType.VirtualMachineScaleSet, new BitmapImage(new Uri("pack://application:,,,/Media/virtual_machine_set.png")));
        }

        [TestMethod]
        public void WhenConvertingUnknownResourceThenResultIsDefaultIcon()
        {
            object result = this.resourceTypeToIconConverter.Convert("I am not a valid resource type", typeof(bool), null, new CultureInfo("en-us"));
            var expected = new BitmapImage(new Uri("pack://application:,,,/Media/resource_default.png"));

            Assert.IsInstanceOfType(result, typeof(BitmapImage));
            Assert.AreEqual(expected.UriSource, ((BitmapImage)result).UriSource);
        }

        private void TestConverter(ResourceType resourceTypeToConvert, BitmapImage expectedImage)
        {
            object result = this.resourceTypeToIconConverter.Convert(resourceTypeToConvert, typeof(bool), null, new CultureInfo("en-us"));

            Assert.IsInstanceOfType(result, typeof(BitmapImage));
            Assert.AreEqual(expectedImage.UriSource, ((BitmapImage)result).UriSource);
        }
    }
}
