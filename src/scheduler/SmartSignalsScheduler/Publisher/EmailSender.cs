//-----------------------------------------------------------------------
// <copyright file="EmailSender.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Publisher
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared;
    using Microsoft.Azure.Monitoring.SmartSignals.RuntimeShared.SignalResultPresentation;
    using Microsoft.Azure.Monitoring.SmartSignals.Scheduler.Exceptions;
    using Microsoft.Azure.Monitoring.SmartSignals.Shared;
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using Unity.Attributes;

    /// <summary>
    /// This class is responsible for sending Smart Signal results Email
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private ITracer tracer;
        private ISendGridClient sendGridClient;
        private string emailRecipient;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSender"/> class.
        /// </summary>
        /// <param name="tracer">The tracer to use.</param>
        [InjectionConstructor]
        public EmailSender(ITracer tracer)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);

            this.emailRecipient = ConfigurationReader.ReadConfig("EmailRecipient", false);
            var apiKey = ConfigurationReader.ReadConfig("SendgridApiKey", false);
            this.sendGridClient = string.IsNullOrWhiteSpace(apiKey) ? null : new SendGridClient(apiKey);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSender"/> class.
        /// </summary>
        /// <param name="tracer">The tracer to use.</param>
        /// <param name="sendGridClient">The send grid client.</param>
        /// <param name="emailRecipient">The email recipient.</param>
        public EmailSender(ITracer tracer, ISendGridClient sendGridClient, string emailRecipient)
        {
            this.tracer = Diagnostics.EnsureArgumentNotNull(() => tracer);

            this.sendGridClient = sendGridClient;
            this.emailRecipient = emailRecipient;
        }

        /// <summary>
        /// Sends the Smart Signal result Email.
        /// </summary>
        /// <param name="signalId">The signal ID.</param>
        /// <param name="smartSignalResultItems">The Smart Signal result items.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendSignalResultEmailAsync(string signalId, IList<SmartSignalResultItemPresentation> smartSignalResultItems)
        {
            if (this.sendGridClient == null)
            {
                this.tracer.TraceWarning("SendGrid API key was not found, not sending email");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.emailRecipient))
            {
                this.tracer.TraceWarning("Email recipient was not provided, not sending email");
                return;
            }

            if (smartSignalResultItems == null || !smartSignalResultItems.Any())
            {
                this.tracer.TraceInformation($"no result items to publish for signal {signalId}");
                return;
            }

            this.tracer.TraceInformation($"Sending signal result email to {this.emailRecipient} for signal {signalId}");

            // TODO: Parse the smart signal result to an HTML and add a link to the SiRA UI
            var msg = new SendGridMessage
            {
                From = new EmailAddress("smartsignals@microsoft.com", "Smart Signals"),
                Subject = $"Found new {smartSignalResultItems.Count} result items for signal {signalId}",
                PlainTextContent = "Found new result items!",
                HtmlContent = "<strong>Found new result items!</strong>"
            };

            msg.AddTo(new EmailAddress(this.emailRecipient));
            var response = await this.sendGridClient.SendEmailAsync(msg);

            if (!IsSuccessStatusCode(response.StatusCode))
            {
                string content = response.Body != null ? await response.Body.ReadAsStringAsync() : string.Empty;
                var message = $"Failed to send signal results to {this.emailRecipient} for signal {signalId}. Fail StatusCode: {response.StatusCode}. Content: {content}.";
                throw new EmailSendingException(message);
            }

            this.tracer.TraceInformation($"Sent signal result email successfuly to {this.emailRecipient} for signal {signalId}");
        }

        /// <summary>
        /// Checks if the status code is a success status code
        /// </summary>
        /// <param name="statusCode">The status code</param>
        /// <returns>True if the status code is a success status code, false otherwise</returns>
        private static bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            return (int)statusCode >= 200 && (int)statusCode <= 299;
        }
    }
}
