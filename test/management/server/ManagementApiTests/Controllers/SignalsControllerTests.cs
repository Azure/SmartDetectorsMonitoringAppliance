namespace ManagementApiTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Controllers;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Models;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.Exceptions;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalConfiguration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using NCrontab;

    [TestClass]
    public class SignalsControllerTests
    {
        private Mock<ISmartSignalConfigurationStore> smartSignalConfigurationStoreMock;
        private SignalsController signalsController;

        [TestInitialize]
        public void Initialize()
        {
            this.smartSignalConfigurationStoreMock = new Mock<ISmartSignalConfigurationStore>();
            this.signalsController = new SignalsController(this.smartSignalConfigurationStoreMock.Object);

            // Mock the controller request
            this.signalsController.Request = new HttpRequestMessage();
        }

        [TestMethod]
        public async Task WhenGettingSmartSignalsHappyFlow()
        {
            this.smartSignalConfigurationStoreMock.Setup(s => s.GetAllSmartSignalConfigurationsAsync())
                .ReturnsAsync(new List<SmartSignalConfiguration>()
                {
                    new SmartSignalConfiguration()
                    {
                        ResourceType = ResourceType.ResourceGroup,
                        SignalId = "signal1",
                        Schedule = CrontabSchedule.Parse("0 0 */1 * *")
                    },
                    new SmartSignalConfiguration()
                    {
                        ResourceType = ResourceType.Subscription,
                        SignalId = "signal2",
                        Schedule = CrontabSchedule.Parse("0 0 */1 * *")
                    }
                });

            var smartSignals = await this.signalsController.GetAllSmartSignals();

            Assert.AreEqual(2, smartSignals.Count());
        }

        [TestMethod]
        public async Task WhenGettingSmartSignalButStoreThrowsException()
        {
            this.smartSignalConfigurationStoreMock.Setup(s => s.GetAllSmartSignalConfigurationsAsync())
                .ThrowsAsync(new SmartSignalConfigurationStoreException(string.Empty, new Exception()));

            try
            {
                await this.signalsController.GetAllSmartSignals();
            }
            catch (HttpResponseException e)
            {
                Assert.AreEqual(HttpStatusCode.InternalServerError, e.Response.StatusCode);
                return;
            }

            Assert.Fail("When controller fails to read from signals store it should throw an exception");
        }

        [TestMethod]
        public async Task WhenAddingSignalHappyFlow()
        {
            var addSignalModel = new AddSignalModel()
            {
                ResourceType = ResourceType.ResourceGroup,
                Schedule = "0 0 */1 * *"
            };

            this.smartSignalConfigurationStoreMock.Setup(s => s.AddOrReplaceSmartSignalConfigurationAsync(It.IsAny<SmartSignalConfiguration>()))
                                                  .Returns(Task.CompletedTask);
            
            // This shouldn't throw any exception
            await this.signalsController.AddSignal(addSignalModel);
        }

        [TestMethod]
        public async Task WhenAddingSignalButModelIsInvalidBecauseScheduleValueIsEmpty()
        {
            var addSignalModel = new AddSignalModel()
            {
                ResourceType = ResourceType.ResourceGroup,
                Schedule = string.Empty
            };

            try
            {
                await this.signalsController.AddSignal(addSignalModel);
            }
            catch (HttpResponseException e)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, e.Response.StatusCode);
                return;
            }

            Assert.Fail("Invalid model should throw an exception");
        }

        [TestMethod]
        public async Task WhenAddingSignalButScheduleValueIsInvalidCronValue()
        {
            var addSignalModel = new AddSignalModel()
            {
                ResourceType = ResourceType.ResourceGroup,
                Schedule = "corrupted value"
            };

            try
            {
                await this.signalsController.AddSignal(addSignalModel);
            }
            catch (HttpResponseException e)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, e.Response.StatusCode);
                return;
            }

            Assert.Fail("Invalid CRON value should throw an exception");
        }

        [TestMethod]
        public async Task WhenAddingSignalButStoreThrowsException()
        {
            var addSignalModel = new AddSignalModel()
            {
                ResourceType = ResourceType.ResourceGroup,
                Schedule = "0 0 */1 * *"
            };

            this.smartSignalConfigurationStoreMock.Setup(s => s.AddOrReplaceSmartSignalConfigurationAsync(It.IsAny<SmartSignalConfiguration>()))
                                                  .ThrowsAsync(new SmartSignalConfigurationStoreException(string.Empty, new Exception()));

            try
            {
                await this.signalsController.AddSignal(addSignalModel);
            }
            catch (HttpResponseException e)
            {
                Assert.AreEqual(HttpStatusCode.InternalServerError, e.Response.StatusCode);
                return;
            }

            Assert.Fail("Exception coming from the Signals store should cause to an exception from the controller");
        }
    }
}
