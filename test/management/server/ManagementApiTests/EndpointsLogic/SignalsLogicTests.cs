namespace ManagementApiTests.EndpointsLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Responses;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Models;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalConfiguration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SignalsControllerTests
    {
        private Mock<ISmartSignalConfigurationStore> smartSignalConfigurationStoreMock;
        private Mock<ISmartSignalsRepository> smartSignalsRepository;

        private ISignalsLogic signalsLogic;

        [TestInitialize]
        public void Initialize()
        {
            this.smartSignalConfigurationStoreMock = new Mock<ISmartSignalConfigurationStore>();
            this.smartSignalsRepository = new Mock<ISmartSignalsRepository>();
            this.signalsLogic = new SignalsLogic(this.smartSignalsRepository.Object, this.smartSignalConfigurationStoreMock.Object);
        }

        #region Getting Signals Tests

        [TestMethod]
        public async Task WhenGettingAllSignalsHappyFlow()
        {
            this.smartSignalsRepository.Setup(repository => repository.ReadAllSignalsMetadataAsync())
                                       .ReturnsAsync(() => new List<SmartSignalMetadata>()
                {
                    new SmartSignalMetadata("someId", "someName", "someDescription", "someVersion", "someAssemblyName", "someClassName")
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
                                       .ThrowsAsync(new SmartSignalConfigurationStoreException("some message", new Exception()));

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

        #region Adding New Signal Version Tests

        [TestMethod]
        public async Task WhenAddingSignalHappyFlow()
        {
            var addSignalModel = new AddSignalVersion()
            {
                SignalId = Guid.NewGuid().ToString(),
                ResourceType = ResourceType.ResourceGroup,
                Schedule = "0 0 */1 * *"
            };

            this.smartSignalConfigurationStoreMock.Setup(s => s.AddOrReplaceSmartSignalConfigurationAsync(It.IsAny<SmartSignalConfiguration>()))
                                                  .Returns(Task.CompletedTask);
            
            // This shouldn't throw any exception
            await this.signalsLogic.AddSignalVersionAsync(addSignalModel);
        }

        [TestMethod]
        public async Task WhenAddingSignalButModelIsInvalidBecauseSignalIdIsEmptyThenThrowException()
        {
            var addSignalModel = new AddSignalVersion()
            {
                SignalId = string.Empty,
                ResourceType = ResourceType.ResourceGroup,
                Schedule = string.Empty
            };

            try
            {
                await this.signalsLogic.AddSignalVersionAsync(addSignalModel);
            }
            catch (SmartSignalsManagementApiException e)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, e.StatusCode);
                return;
            }

            Assert.Fail("Invalid model should throw an exception");
        }

        [TestMethod]
        public async Task WhenAddingSignalButModelIsInvalidBecauseScheduleValueIsEmptyThenThrowException()
        {
            var addSignalModel = new AddSignalVersion()
            {
                SignalId = Guid.NewGuid().ToString(),
                ResourceType = ResourceType.ResourceGroup,
                Schedule = string.Empty
            };

            try
            {
                await this.signalsLogic.AddSignalVersionAsync(addSignalModel);
            }
            catch (SmartSignalsManagementApiException e)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, e.StatusCode);
                return;
            }

            Assert.Fail("Invalid model should throw an exception");
        }

        [TestMethod]
        public async Task WhenAddingSignalButScheduleValueIsInvalidCronValueThenThrowException()
        {
            var addSignalModel = new AddSignalVersion()
            {
                SignalId = Guid.NewGuid().ToString(),
                ResourceType = ResourceType.ResourceGroup,
                Schedule = "corrupted value"
            };

            try
            {
                await this.signalsLogic.AddSignalVersionAsync(addSignalModel);
            }
            catch (SmartSignalsManagementApiException e)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, e.StatusCode);
                return;
            }

            Assert.Fail("Invalid CRON value should throw an exception");
        }

        [TestMethod]
        public async Task WhenAddingSignalButStoreThrowsExceptionThenThrowTheWrappedException()
        {
            var addSignalModel = new AddSignalVersion()
            {
                SignalId = Guid.NewGuid().ToString(),
                ResourceType = ResourceType.ResourceGroup,
                Schedule = "0 0 */1 * *"
            };

            this.smartSignalConfigurationStoreMock.Setup(s => s.AddOrReplaceSmartSignalConfigurationAsync(It.IsAny<SmartSignalConfiguration>()))
                                                  .ThrowsAsync(new SmartSignalConfigurationStoreException(string.Empty, new Exception()));

            try
            {
                await this.signalsLogic.AddSignalVersionAsync(addSignalModel);
            }
            catch (SmartSignalsManagementApiException e)
            {
                Assert.AreEqual(HttpStatusCode.InternalServerError, e.StatusCode);
                return;
            }

            Assert.Fail("Exception coming from the Signals store should cause to an exception from the controller");
        }

        #endregion
    }
}
