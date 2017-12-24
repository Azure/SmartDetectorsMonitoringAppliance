namespace ManagementApiTests.EndpointsLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Responses;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SignalApiTests
    {
        private Mock<ISmartSignalsRepository> smartSignalsRepository;

        private ISignalApi signalsLogic;

        [TestInitialize]
        public void Initialize()
        {
            this.smartSignalsRepository = new Mock<ISmartSignalsRepository>();
            this.signalsLogic = new SignalApi(this.smartSignalsRepository.Object);
        }

        #region Getting Signals Tests

        [TestMethod]
        public async Task WhenGettingAllSignalsHappyFlow()
        {
            this.smartSignalsRepository.Setup(repository => repository.ReadAllSignalsMetadataAsync())
                                       .ReturnsAsync(() => new List<SmartSignalMetadata>()
                {
                    new SmartSignalMetadata("someId", "someName", "someDescription", "someVersion", "someAssemblyName", "someClassName", new List<ResourceType> { ResourceType.ResourceGroup })
                });

            ListSmartSignalsResponse response = await this.signalsLogic.GetAllSmartSignalsAsync();

            Assert.AreEqual(1, response.Signals.Count);
            Assert.AreEqual("someId", response.Signals.First().Id);
            Assert.AreEqual("someName", response.Signals.First().Name);
        }

        [TestMethod]
        public async Task WhenGettingAllSignalsButSignalsRepositoryThrowsExceptionThenThrowsWrappedException()
        {
            this.smartSignalsRepository.Setup(repository => repository.ReadAllSignalsMetadataAsync())
                                       .ThrowsAsync(new AlertRuleStoreException("some message", new Exception()));

            try
            {
                await this.signalsLogic.GetAllSmartSignalsAsync();
            }
            catch (SmartSignalsManagementApiException)
            {
                return;
            }

            Assert.Fail("Exception from the signals store should cause to management API exception");
        }

        #endregion
    }
}
