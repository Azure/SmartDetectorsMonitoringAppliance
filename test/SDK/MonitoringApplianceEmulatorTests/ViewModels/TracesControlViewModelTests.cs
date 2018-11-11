//-----------------------------------------------------------------------
// <copyright file="TracesControlViewModelTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MonitoringApplianceEmulatorTests.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Microsoft.Azure.Monitoring.SmartDetectors;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Models;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.Trace;
    using Microsoft.Azure.Monitoring.SmartDetectors.MonitoringApplianceEmulator.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class TracesControlViewModelTests
    {
        private Mock<IEmulationSmartDetectorRunner> smartDetectorRunnerMock;
        private Mock<IPageableLogArchive> logArchiveMock;
        private TracesControlViewModel tracesControlViewModel;
        private Dictionary<string, int> propertyChangedCounters;

        [TestInitialize]
        public void TestInitialize()
        {
            this.smartDetectorRunnerMock = new Mock<IEmulationSmartDetectorRunner>();

            this.logArchiveMock = new Mock<IPageableLogArchive>();
            this.logArchiveMock
                .SetupGet(m => m.LogNames)
                .Returns(new ObservableCollection<string>(new[] { "log1", "log2" }));

            this.tracesControlViewModel = new TracesControlViewModel(
                this.smartDetectorRunnerMock.Object,
                this.logArchiveMock.Object,
                new Mock<ITracer>().Object);

            this.propertyChangedCounters = new Dictionary<string, int>();
            this.tracesControlViewModel.PropertyChanged += (sender, args) =>
            {
                if (!this.propertyChangedCounters.ContainsKey(args.PropertyName))
                {
                    this.propertyChangedCounters[args.PropertyName] = 0;
                }

                this.propertyChangedCounters[args.PropertyName]++;
            };
        }

        [TestMethod]
        public void WhenPageableLogIsNullThenAllPropertiesGiveTheRightValue()
        {
            Assert.IsTrue(this.tracesControlViewModel.LogNames.SequenceEqual(new[] { "log1", "log2" }));
            Assert.AreEqual(string.Empty, this.tracesControlViewModel.CurrentLogName, "Mismatch on CurrentLogName");
            Assert.AreEqual(0, this.tracesControlViewModel.CurrentPageIndex, "Mismatch on CurrentPageIndex");
            Assert.AreEqual(0, this.tracesControlViewModel.CurrentPageStart, "Mismatch on CurrentPageStart");
            Assert.AreEqual(0, this.tracesControlViewModel.CurrentPageEnd, "Mismatch on CurrentPageEnd");
            Assert.AreEqual(0, this.tracesControlViewModel.NumberOfPages, "Mismatch on NumberOfPages");
            Assert.AreEqual(0, this.tracesControlViewModel.NumberOfTraceLines, "Mismatch on NumberOfTraceLines");
            Assert.AreEqual(true, this.tracesControlViewModel.IsFirstPage, "Mismatch on IsFirstPage");
            Assert.AreEqual(true, this.tracesControlViewModel.IsLastPage, "Mismatch on IsLastPage");
        }

        [TestMethod]
        public void WhenRunnerLogChangesThenViewModelLogIsSet()
        {
            var logMock = new Mock<IPageableLog>();
            logMock.SetupGet(m => m.Name).Returns("testLog");
            logMock.SetupGet(m => m.CurrentPageIndex).Returns(4);
            logMock.SetupGet(m => m.CurrentPageStart).Returns(20);
            logMock.SetupGet(m => m.CurrentPageEnd).Returns(25);
            logMock.SetupGet(m => m.NumberOfPages).Returns(5);
            logMock.SetupGet(m => m.NumberOfTraceLines).Returns(120);

            this.smartDetectorRunnerMock
                .SetupGet(m => m.PageableLog)
                .Returns(logMock.Object);

            this.smartDetectorRunnerMock.Raise(
                m => m.PropertyChanged += null,
                new PropertyChangedEventArgs(nameof(IEmulationSmartDetectorRunner.PageableLog)));

            // Validate that the set operation occured
            Assert.AreSame(logMock.Object, this.tracesControlViewModel.PageableLog, "Mismatch on PageableLog");
            Assert.AreEqual(1, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");
            Assert.AreEqual(1, this.propertyChangedCounters[string.Empty], "Got wrong PropertyChanged event");

            // And validate the properties are updated correctly
            Assert.AreEqual("testLog", this.tracesControlViewModel.CurrentLogName, "Mismatch on CurrentLogName");
            Assert.AreEqual(5, this.tracesControlViewModel.CurrentPageIndex, "Mismatch on CurrentPageIndex");
            Assert.AreEqual(21, this.tracesControlViewModel.CurrentPageStart, "Mismatch on CurrentPageStart");
            Assert.AreEqual(26, this.tracesControlViewModel.CurrentPageEnd, "Mismatch on CurrentPageEnd");
            Assert.AreEqual(5, this.tracesControlViewModel.NumberOfPages, "Mismatch on NumberOfPages");
            Assert.AreEqual(120, this.tracesControlViewModel.NumberOfTraceLines, "Mismatch on NumberOfTraceLines");
            Assert.AreEqual(false, this.tracesControlViewModel.IsFirstPage, "Mismatch on IsFirstPage");
            Assert.AreEqual(true, this.tracesControlViewModel.IsLastPage, "Mismatch on IsLastPage");
        }

        [TestMethod]
        public void WhenRunnerStartsOrStopsRunningThenViewModelLogIsSet()
        {
            this.smartDetectorRunnerMock
                .SetupGet(m => m.IsSmartDetectorRunning)
                .Returns(true);

            this.smartDetectorRunnerMock.Raise(
                m => m.PropertyChanged += null,
                new PropertyChangedEventArgs(nameof(IEmulationSmartDetectorRunner.IsSmartDetectorRunning)));

            Assert.AreEqual(true, this.tracesControlViewModel.IsSmartDetectorRunning, "Mismatch on IsSmartDetectorRunning");
            Assert.AreEqual(1, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");
            Assert.AreEqual(1, this.propertyChangedCounters[nameof(TracesControlViewModel.IsSmartDetectorRunning)], "Got wrong PropertyChanged event");

            this.smartDetectorRunnerMock
                .SetupGet(m => m.IsSmartDetectorRunning)
                .Returns(false);

            this.smartDetectorRunnerMock.Raise(
                m => m.PropertyChanged += null,
                new PropertyChangedEventArgs(nameof(IEmulationSmartDetectorRunner.IsSmartDetectorRunning)));

            Assert.AreEqual(false, this.tracesControlViewModel.IsSmartDetectorRunning, "Mismatch on IsSmartDetectorRunning");
            Assert.AreEqual(1, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");
            Assert.AreEqual(2, this.propertyChangedCounters[nameof(TracesControlViewModel.IsSmartDetectorRunning)], "Got wrong PropertyChanged event");
        }

        [TestMethod]
        public void WhenUpdatingLogNameThenLogIsUpdated()
        {
            var logMock = new Mock<IPageableLog>();
            this.logArchiveMock
                .Setup(m => m.GetLogAsync("testLog", 50))
                .ReturnsAsync(logMock.Object);

            this.tracesControlViewModel.CurrentLogName = "testLog";
            Assert.AreSame(logMock.Object, this.tracesControlViewModel.PageableLog, "Mismatch on PageableLog");
            Assert.AreEqual(2, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");
            Assert.AreEqual(2, this.propertyChangedCounters[string.Empty], "Got wrong number of PropertyChanged events for string.Empty");
            Assert.AreEqual(1, this.propertyChangedCounters[nameof(TracesControlViewModel.LoadLogTask)], "Got wrong number of PropertyChanged events for LoadLogTask");
            this.logArchiveMock.Verify(m => m.GetLogAsync("testLog", 50), Times.Once);
            logMock.VerifySet(m => m.PageSize = 50, Times.Once);
        }

        [TestMethod]
        public void WhenUpdatingPageSizeItIsPersistedAndUsedForNewLogs()
        {
            var logMock = new Mock<IPageableLog>();
            this.logArchiveMock
                .Setup(m => m.GetLogAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(logMock.Object);

            this.tracesControlViewModel.CurrentLogName = "testLog";
            Assert.AreSame(logMock.Object, this.tracesControlViewModel.PageableLog, "Mismatch on PageableLog");
            Assert.AreEqual(2, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");
            Assert.AreEqual(2, this.propertyChangedCounters[string.Empty], "Got wrong number of PropertyChanged events for string.Empty");
            Assert.AreEqual(1, this.propertyChangedCounters[nameof(TracesControlViewModel.LoadLogTask)], "Got wrong number of PropertyChanged events for LoadLogTask");
            this.logArchiveMock.Verify(m => m.GetLogAsync("testLog", 50), Times.Once);
            logMock.VerifySet(m => m.PageSize = 50, Times.Once);

            this.propertyChangedCounters.Clear();
            this.tracesControlViewModel.PageSize = 150;
            Assert.AreEqual(0, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");
            logMock.VerifySet(m => m.PageSize = 150, Times.Once);

            this.propertyChangedCounters.Clear();
            this.tracesControlViewModel.CurrentLogName = "testLog2";
            Assert.AreSame(logMock.Object, this.tracesControlViewModel.PageableLog, "Mismatch on PageableLog");
            Assert.AreEqual(2, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");
            Assert.AreEqual(2, this.propertyChangedCounters[string.Empty], "Got wrong number of PropertyChanged events for string.Empty");
            Assert.AreEqual(1, this.propertyChangedCounters[nameof(TracesControlViewModel.LoadLogTask)], "Got wrong number of PropertyChanged events for LoadLogTask");
            this.logArchiveMock.Verify(m => m.GetLogAsync("testLog2", 150), Times.Once);
            logMock.VerifySet(m => m.PageSize = 150, Times.Exactly(2));
        }

        [TestMethod]
        public void WhenSettingTheCurrentPageIndexThenValueIsSetCorrectly()
        {
            var logMock = new Mock<IPageableLog>();
            logMock.SetupGet(m => m.NumberOfPages).Returns(5);
            this.logArchiveMock
                .Setup(m => m.GetLogAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(logMock.Object);

            this.tracesControlViewModel.CurrentLogName = "testLog";
            this.propertyChangedCounters.Clear();

            this.tracesControlViewModel.CurrentPageIndex = -1;
            logMock.VerifySet(m => m.CurrentPageIndex = 0, Times.Once);
            Assert.AreEqual(0, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");

            this.tracesControlViewModel.CurrentPageIndex = 0;
            logMock.VerifySet(m => m.CurrentPageIndex = 0, Times.Exactly(2));
            Assert.AreEqual(0, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");

            this.tracesControlViewModel.CurrentPageIndex = 1;
            logMock.VerifySet(m => m.CurrentPageIndex = 0, Times.Exactly(3));
            Assert.AreEqual(0, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");

            this.tracesControlViewModel.CurrentPageIndex = 3;
            logMock.VerifySet(m => m.CurrentPageIndex = 2, Times.Once);
            Assert.AreEqual(0, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");

            this.tracesControlViewModel.CurrentPageIndex = 5;
            logMock.VerifySet(m => m.CurrentPageIndex = 4, Times.Once);
            Assert.AreEqual(0, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");

            this.tracesControlViewModel.CurrentPageIndex = 6;
            logMock.VerifySet(m => m.CurrentPageIndex = 4, Times.Exactly(2));
            Assert.AreEqual(0, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");
        }

        [TestMethod]
        public void WhenCallingPagingCommandsTheCorrectPageIsSet()
        {
            var logMock = new Mock<IPageableLog>();
            logMock.SetupGet(m => m.NumberOfPages).Returns(5);
            logMock.SetupGet(m => m.CurrentPageIndex).Returns(2);
            this.logArchiveMock
                .Setup(m => m.GetLogAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(logMock.Object);

            this.tracesControlViewModel.CurrentLogName = "testLog";
            this.propertyChangedCounters.Clear();

            this.tracesControlViewModel.FirstPageCommand.Execute(null);
            logMock.VerifySet(m => m.CurrentPageIndex = 0, Times.Once);
            Assert.AreEqual(0, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");

            this.tracesControlViewModel.PrevPageCommand.Execute(null);
            logMock.VerifySet(m => m.CurrentPageIndex = 1, Times.Once);
            Assert.AreEqual(0, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");

            this.tracesControlViewModel.NextPageCommand.Execute(null);
            logMock.VerifySet(m => m.CurrentPageIndex = 3, Times.Once);
            Assert.AreEqual(0, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");

            this.tracesControlViewModel.LastPageCommand.Execute(null);
            logMock.VerifySet(m => m.CurrentPageIndex = 4, Times.Once);
            Assert.AreEqual(0, this.propertyChangedCounters.Count, "Got wrong number of PropertyChanged events");
        }
    }
}
