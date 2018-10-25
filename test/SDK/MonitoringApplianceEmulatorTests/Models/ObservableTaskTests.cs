//-----------------------------------------------------------------------
// <copyright file="ObservableTaskTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ObservableTaskTests
    {
        private Mock<ITracer> tracerMock;

        [TestInitialize]
        public void Setup()
        {
            this.tracerMock = new Mock<ITracer>();
        }

        [TestMethod]
        public void WhenCreatingAnObservableTaskForTaskThatWasAlreadyCompletedThenItIsInitializedCorrectly()
        {
            var expectedResult = new List<string>() { "Iniesta", "Costa", "Pique" };
            ObservableTask<List<string>> observableTask = new ObservableTask<List<string>>(Task.FromResult(expectedResult), this.tracerMock.Object);

            Assert.IsFalse(observableTask.IsRunning);
            CollectionAssert.AreEqual(expectedResult, observableTask.Result);
        }

        [TestMethod]
        public async Task WhenCreatingAnObservableTaskForTaskThatWasNotCompletedYetThenItIsInitializedCorrectly()
        {
            var expectedResult = new List<string>() { "Iniesta", "Costa", "Pique" };

            Task<List<string>> someTask = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
                return new List<string>() { "Iniesta", "Costa", "Pique" };
            });

            ObservableTask<List<string>> observableTask = new ObservableTask<List<string>>(someTask, this.tracerMock.Object);

            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.IsFalse(observableTask.IsRunning);
            CollectionAssert.AreEqual(expectedResult, observableTask.Result);
        }

        [TestMethod]
        public async Task WhenTheTaskwasCompletedThenTheOnTaskCompletedCallbackWasInvokedWithExpectedParameter()
        {
            var expectedTaskResult = new List<string>() { "Iniesta", "Costa", "Pique" };
            List<string> actualCallbackInputParameter = null;

            Task<List<string>> someTask = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
                return new List<string>() { "Iniesta", "Costa", "Pique" };
            });

            ObservableTask<List<string>> observableTask = new ObservableTask<List<string>>(
                someTask,
                this.tracerMock.Object,
                (param) =>
                {
                    actualCallbackInputParameter = param;
                });

            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.IsFalse(observableTask.IsRunning);
            CollectionAssert.AreEqual(expectedTaskResult, observableTask.Result);
            CollectionAssert.AreEqual(expectedTaskResult, actualCallbackInputParameter);
        }
    }
}
