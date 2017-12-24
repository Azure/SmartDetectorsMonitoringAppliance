namespace SmartSignalSchedulerTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler.SignalRunTracker;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.AlertRules;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureStorage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage.Table;
    using Moq;
    using NCrontab;

    [TestClass]
    public class SignalRunTrackerTest
    {
        private SignalRunsTracker signalRunsTracker;
        private Mock<ICloudTableWrapper> tableMock;

        [TestInitialize]
        public void Setup()
        {
            this.tableMock = new Mock<ICloudTableWrapper>();
            var tableClientMock = new Mock<ICloudTableClientWrapper>();
            tableClientMock.Setup(m => m.GetTableReference(It.IsAny<string>())).Returns(this.tableMock.Object);

            var tracerMock = new Mock<ITracer>();
            this.signalRunsTracker = new SignalRunsTracker(tableClientMock.Object, tracerMock.Object);
        }

        [TestMethod]
        public async Task WhenUpdatingSignalRunThenUpdateIsCalledCorrectly()
        {
            var signalExecution = new SignalExecutionInfo
            {
                RuleId = "some_rule",
                SignalId = "some_signal",
                LastExecutionTime = DateTime.UtcNow.AddHours(-1),
                CurrentExecutionTime = DateTime.UtcNow.AddMinutes(-1)
            };
            await this.signalRunsTracker.UpdateSignalRunAsync(signalExecution);
            this.tableMock.Verify(m => m.ExecuteAsync(It.Is<TableOperation>(operation =>
                operation.OperationType == TableOperationType.InsertOrReplace &&
                operation.Entity.RowKey.Equals(signalExecution.RuleId) && 
                ((TrackSignalRunEntity)operation.Entity).SignalId.Equals(signalExecution.SignalId) &&
                ((TrackSignalRunEntity)operation.Entity).LastSuccessfulExecutionTime.Equals(signalExecution.CurrentExecutionTime))));
        }

        [TestMethod]
        public async Task WhenGettingSignalsToRunWithRulesThenOnlyValidSignalsAreReturned()
        {
            var rules = new List<AlertRule>
            {
                new AlertRule
                {
                    Id = "should_not_run_rule",
                    SignalId = "should_not_run_signal",
                    Schedule = CrontabSchedule.Parse("0 0 */1 * *") // once a day at midnight
                },
                new AlertRule
                {
                    Id = "should_run_rule",
                    SignalId = "should_run_signal",
                    Schedule = CrontabSchedule.Parse("0 */1 * * *") // every round hour
                },
                new AlertRule
                {
                    Id = "should_run_rule2",
                    SignalId = "should_run_signal2",
                    Schedule = CrontabSchedule.Parse("0 0 */1 * *") // once a day at midnight
                }
            };

            // create a table tracking result where 1 signal never ran, 1 signal that ran today and 1 signal that ran 2 hours ago
            var now = DateTime.UtcNow;
            var tableResult = new List<TrackSignalRunEntity>
            {
                new TrackSignalRunEntity
                {
                    RowKey = "should_not_run_rule",
                    SignalId = "should_not_run_signal",
                    LastSuccessfulExecutionTime = new DateTime(now.Year, now.Month, now.Day, 0, 5, 0)
                },
                new TrackSignalRunEntity
                {
                    RowKey = "should_run_rule",
                    SignalId = "should_run_signal",
                    LastSuccessfulExecutionTime = now.AddHours(-2)
                }
            };
            
            this.tableMock.Setup(m => m.ReadPartitionAsync<TrackSignalRunEntity>("tracking")).ReturnsAsync(tableResult);

            var signalsToRun = await this.signalRunsTracker.GetSignalsToRunAsync(rules);
            Assert.AreEqual(2, signalsToRun.Count);
            Assert.AreEqual("should_run_signal", signalsToRun.First().SignalId);
            Assert.AreEqual("should_run_signal2", signalsToRun.Last().SignalId);
        }
    }
}
