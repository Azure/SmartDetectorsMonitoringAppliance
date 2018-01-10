//-----------------------------------------------------------------------
// <copyright file="EmailSenderTests.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SmartSignalSchedulerTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Exceptions;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Publisher;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared.SignalResultPresentation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using SendGrid;
    using SendGrid.Helpers.Mail;

    [TestClass]
    public class EmailSenderTests
    {
        private EmailSender emailSender;
        private Mock<ISendGridClient> sendgridClientMock;

        [TestInitialize]
        public void Setup()
        {
            var tracerMock = new Mock<ITracer>();
            this.sendgridClientMock = new Mock<ISendGridClient>();
            this.emailSender = new EmailSender(tracerMock.Object, this.sendgridClientMock.Object, "some@email.com");
        }

        [TestMethod]
        public async Task WhenNoResultItemsAreFoundThenEmailIsNotSent()
        {
            await this.emailSender.SendSignalResultEmailAsync("someSignalId", new List<SmartSignalResultItemPresentation>());
            this.sendgridClientMock.Verify(m => m.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task WhenNoEmailRecipientIsFoundThenEmailIsNotSent()
        {
            var resultItems = new List<SmartSignalResultItemPresentation>
            {
                new SmartSignalResultItemPresentation("id", "title", null, "resource", null, "someSignalId", string.Empty, DateTime.UtcNow, 0, null, null)
            };
            this.emailSender = new EmailSender(new Mock<ITracer>().Object, this.sendgridClientMock.Object, null);
            await this.emailSender.SendSignalResultEmailAsync("someSignalId", resultItems);
            this.sendgridClientMock.Verify(m => m.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(EmailSendingException))]
        public async Task WhenSendGridClientRerturnsFailStatusCodeThenExceptionIsThrown()
        {
            this.sendgridClientMock
                .Setup(m => m.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Response(HttpStatusCode.GatewayTimeout, null, null));

            var resultItems = this.CreateSignalResultList();
            await this.emailSender.SendSignalResultEmailAsync("someSignalId", resultItems);

            this.sendgridClientMock.Verify(m => m.SendEmailAsync(It.Is<SendGridMessage>(message => message.From.Email.Equals("smartsignals@microsoft.com")), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task WhenResultItemsAreFoundThenEmailIsSent()
        {
            this.sendgridClientMock
                .Setup(m => m.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Response(HttpStatusCode.Accepted, null, null));

            var resultItems = this.CreateSignalResultList();
            await this.emailSender.SendSignalResultEmailAsync("someSignalId", resultItems);

            this.sendgridClientMock.Verify(m => m.SendEmailAsync(It.Is<SendGridMessage>(message => message.From.Email.Equals("smartsignals@microsoft.com")), It.IsAny<CancellationToken>()), Times.Once);
        }

        private List<SmartSignalResultItemPresentation> CreateSignalResultList()
        {
            return new List<SmartSignalResultItemPresentation>
            {
                new SmartSignalResultItemPresentation("id", "title", null, "resource", null, "someSignalId", string.Empty, DateTime.UtcNow, 0, null, null)
            };
        }
    }
}
