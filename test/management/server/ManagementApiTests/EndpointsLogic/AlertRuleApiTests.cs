//-----------------------------------------------------------------------
// <copyright file="AlertRuleApiTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ManagementApiTests.EndpointsLogic
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.AlertRules;
    using Microsoft.Azure.Monitoring.SmartSignals.Infrastructure.Exceptions;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.EndpointsLogic;
    using Microsoft.Azure.Monitoring.SmartSignals.ManagementApi.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class AlertRuleApiTests
    {
        private Mock<IAlertRuleStore> alertRuleStoreMock;

        private IAlertRuleApi alertRuleApi;

        [TestInitialize]
        public void Initialize()
        {
            this.alertRuleStoreMock = new Mock<IAlertRuleStore>();
            this.alertRuleApi = new AlertRuleApi(this.alertRuleStoreMock.Object);
        }

        #region Adding New Alert Rule Tests

        [TestMethod]
        public async Task WhenAddingSignalHappyFlow()
        {
            var addSignalModel = new AddAlertRule()
            {
                SignalId = Guid.NewGuid().ToString(),
                ResourceType = ResourceType.ResourceGroup,
                Schedule = "0 0 */1 * *"
            };

            this.alertRuleStoreMock.Setup(s => s.AddOrReplaceAlertRuleAsync(It.IsAny<AlertRule>()))
                                                  .Returns(Task.CompletedTask);

            // This shouldn't throw any exception
            await this.alertRuleApi.AddAlertRuleAsync(addSignalModel);
        }

        [TestMethod]
        public async Task WhenAddingSignalButModelIsInvalidBecauseSignalIdIsEmptyThenThrowException()
        {
            var addSignalModel = new AddAlertRule()
            {
                SignalId = string.Empty,
                ResourceType = ResourceType.ResourceGroup,
                Schedule = string.Empty
            };

            try
            {
                await this.alertRuleApi.AddAlertRuleAsync(addSignalModel);
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
            var addSignalModel = new AddAlertRule()
            {
                SignalId = Guid.NewGuid().ToString(),
                ResourceType = ResourceType.ResourceGroup,
                Schedule = string.Empty
            };

            try
            {
                await this.alertRuleApi.AddAlertRuleAsync(addSignalModel);
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
            var addSignalModel = new AddAlertRule()
            {
                SignalId = Guid.NewGuid().ToString(),
                ResourceType = ResourceType.ResourceGroup,
                Schedule = "corrupted value"
            };

            try
            {
                await this.alertRuleApi.AddAlertRuleAsync(addSignalModel);
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
            var addSignalModel = new AddAlertRule()
            {
                SignalId = Guid.NewGuid().ToString(),
                ResourceType = ResourceType.ResourceGroup,
                Schedule = "0 0 */1 * *"
            };

            this.alertRuleStoreMock.Setup(s => s.AddOrReplaceAlertRuleAsync(It.IsAny<AlertRule>()))
                                                  .ThrowsAsync(new AlertRuleStoreException(string.Empty, new Exception()));

            try
            {
                await this.alertRuleApi.AddAlertRuleAsync(addSignalModel);
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
