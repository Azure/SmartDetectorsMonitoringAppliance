namespace SmartSignalSchedulerTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureStorage;
    using Microsoft.SmartSignals.Scheduler;
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
            var signalExecution = new SignalExecutionInfo
            {
                SignalId = "some_signal",
                AnalysisEndTime = DateTime.UtcNow,
                AnalysisStartTime = DateTime.UtcNow.AddHours(-1)
            };
            await _signalRunsTracker.UpdateSignalRunAsync(signalExecution);
            _tableMock.Verify(m => m.ExecuteAsync(It.Is<TableOperation>(operation =>
                operation.OperationType == TableOperationType.InsertOrReplace &&
                operation.Entity.RowKey.Equals(signalExecution.SignalId) && 
                ((TrackSignalRunEntity)operation.Entity).LastSuccessfulRunStartTime.Equals(signalExecution.AnalysisStartTime) &&
                ((TrackSignalRunEntity)operation.Entity).LastSuccessfulRunEndTime.Equals(signalExecution.AnalysisEndTime))));
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
                    LastSuccessfulRunEndTime = new DateTime(now.Year, now.Month, now.Day, 0, 5, 0)
                },
                new TrackSignalRunEntity
                {
                    RowKey = "should_run",
                    LastSuccessfulRunEndTime = now.AddHours(-2)
                }
            };
            
            _tableMock.Setup(m => m.ReadPartitionAsync<TrackSignalRunEntity>("tracking")).ReturnsAsync(tableResult);

            var signalsToRun = await _signalRunsTracker.GetSignalsToRunAsync(configurations);
            Assert.AreEqual(2, signalsToRun.Count);
            Assert.AreEqual("should_run", signalsToRun.First().SignalId);
            Assert.AreEqual("should_run2", signalsToRun.Last().SignalId);

        }
    }
}
