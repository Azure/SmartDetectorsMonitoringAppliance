namespace SmartSignalSchedulerTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Trace;
    using Microsoft.SmartSignals.Scheduler;
    using Microsoft.SmartSignals.Scheduler.AzureStorage;
    using Microsoft.SmartSignals.Scheduler.SignalRunTracker;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage.Table;
    using Moq;
    using NCrontab;

    [TestClass]
    public class SignalRunTrackerTest
    {
        private SignalRunsTracker _signalRunsTracker;
        private Mock<ICloudTableWrapper> _tableMock;

        [TestInitialize]
        public void Setup()
        {
            _tableMock = new Mock<ICloudTableWrapper>();
            var tableClientMock = new Mock<ICloudTableClientWrapper>();
            tableClientMock.Setup(m => m.GetTableReference(It.IsAny<string>())).Returns(_tableMock.Object);

            var tracerMock = new Mock<ITracer>();
            _signalRunsTracker = new SignalRunsTracker(tableClientMock.Object, tracerMock.Object);
        }

        [TestMethod]
        public async Task WhenUpdatingSignalRunThenUpdateIsCalledCorrectly()
        {
            const string signalTestId = "some_signal";
            await _signalRunsTracker.UpdateSignalRunAsync(signalTestId);
            _tableMock.Verify(m => m.ExecuteAsync(It.Is<TableOperation>(operation =>
                operation.OperationType == TableOperationType.InsertOrReplace &&
                operation.Entity.RowKey.Equals(signalTestId))));
        }

        [TestMethod]
        public async Task WhenGettingSignalsToRunWithConfigurationThenOnlyValidSignalsAreReturned()
        {
            var configurations = new List<SmartSignalConfiguration>
            {
                new SmartSignalConfiguration
                {
                    SignalId = "should_not_run",
                    Schedule = CrontabSchedule.Parse("0 0 */1 * *") // once a day at midnight
                },
                new SmartSignalConfiguration
                {
                    SignalId = "should_run",
                    Schedule = CrontabSchedule.Parse("0 */1 * * *") // every round hour
                },
                new SmartSignalConfiguration
                {
                    SignalId = "should_run2",
                    Schedule = CrontabSchedule.Parse("0 0 */1 * *") // once a day at midnight
                }
            };

            // create a table tracking result where 1 signal never ran, 1 signal that ran today and 1 signal that ran 2 hours ago
            var now = DateTime.UtcNow;
            var tableResult = new List<TrackSignalRunEntity>
            {
                new TrackSignalRunEntity
                {
                    RowKey = "should_not_run",
                    LastRunTime = new DateTime(now.Year, now.Month, now.Day, 0, 5, 0)
                },
                new TrackSignalRunEntity
                {
                    RowKey = "should_run",
                    LastRunTime = now.AddHours(-2)
                }
            };

            // using reflecation since constructor is internal
            // TODO: consider using a wrapper for the query segment as well
            Type[] constructorParametersTypes = { typeof(List<TrackSignalRunEntity>) };
            object[] constructorParameters = { tableResult };
            var tableQuerySegment = (TableQuerySegment<TrackSignalRunEntity>)typeof(TableQuerySegment<TrackSignalRunEntity>).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, constructorParametersTypes, null)?.Invoke(constructorParameters);

            _tableMock.Setup(m => m.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<TrackSignalRunEntity>>(),
                It.IsAny<TableContinuationToken>())).ReturnsAsync(tableQuerySegment);

            var signalIdsToRun = await _signalRunsTracker.GetSignalsToRunAsync(configurations);
            Assert.AreEqual(2, signalIdsToRun.Count);
            Assert.AreEqual("should_run", signalIdsToRun.First());
            Assert.AreEqual("should_run2", signalIdsToRun.Last());

        }
    }
}
