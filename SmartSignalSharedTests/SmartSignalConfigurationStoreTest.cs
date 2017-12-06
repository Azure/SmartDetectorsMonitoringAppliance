﻿namespace SmartSignalSharedTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.AzureStorage;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalConfiguration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage.Table;
    using Moq;
    using NCrontab;

    [TestClass]
    public class SmartSignalConfigurationStoreTest
    {
        private SmartSignalConfigurationStore _configurationStore;
        private Mock<ICloudTableWrapper> _tableMock;

        [TestInitialize]
        public void Setup()
        {
            _tableMock = new Mock<ICloudTableWrapper>();
            var tableClientMock = new Mock<ICloudTableClientWrapper>();
            tableClientMock.Setup(m => m.GetTableReference(It.IsAny<string>())).Returns(_tableMock.Object);

            var tracerMock = new Mock<ITracer>();
            _configurationStore = new SmartSignalConfigurationStore(tableClientMock.Object, tracerMock.Object);
        }

        [TestMethod]
        public async Task WhenUpdatingSignalConfigurationThenUpdateIsCalledCorrectly()
        {
            const string cronSchedule = "0 1 * * *";

            var configToUpdate = new SmartSignalConfiguration
            {
                SignalId = "signalId",
                Schedule = CrontabSchedule.Parse(cronSchedule),
                ResourceType = ResourceType.VirtualMachine
            };

            await _configurationStore.AddOrReplaceSmartSignalConfigurationAsync(configToUpdate);

            _tableMock.Verify(m => m.ExecuteAsync(It.Is<TableOperation>(operation =>
                operation.OperationType == TableOperationType.InsertOrReplace &&
                operation.Entity.RowKey.Equals(configToUpdate.SignalId) &&
                ((SmartConfigurationEntity)operation.Entity).CrontabSchedule.Equals(cronSchedule) &&
                ((SmartConfigurationEntity)operation.Entity).ResourceType.Equals(configToUpdate.ResourceType))));
        }

        [TestMethod]
        public async Task WhenGettingAllConfigurationsThenTableIsCalledCorrectly()
        {
            // Create configuration entites in the table
            var configurationEntities = new List<SmartConfigurationEntity>
            {
                new SmartConfigurationEntity
                {
                    RowKey = "signal1",
                    CrontabSchedule = "0 0 * * *",
                    ResourceType = ResourceType.VirtualMachine
                },
                new SmartConfigurationEntity
                {
                    RowKey = "signal2",
                    CrontabSchedule = "0 * * * *",
                    ResourceType = ResourceType.Subscription
                }
            };

            _tableMock.Setup(m => m.ReadPartitionAsync<SmartConfigurationEntity>("configurations")).ReturnsAsync(configurationEntities);

            var returnedConfigurations = await _configurationStore.GetAllSmartSignalConfigurationsAsync();
            Assert.AreEqual(2, returnedConfigurations.Count);

            var firstConfiguration = returnedConfigurations.First();
            Assert.AreEqual("signal1", firstConfiguration.SignalId);
            Assert.AreEqual("0 0 * * *", firstConfiguration.Schedule.ToString());
            Assert.AreEqual(ResourceType.VirtualMachine, firstConfiguration.ResourceType);

            var lastConfiguration = returnedConfigurations.Last();
            Assert.AreEqual("signal2", lastConfiguration.SignalId);
            Assert.AreEqual("0 * * * *", lastConfiguration.Schedule.ToString());
            Assert.AreEqual(ResourceType.Subscription, lastConfiguration.ResourceType);
        }
    }
}
