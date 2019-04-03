namespace ParkingRota.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Emails;
    using Microsoft.Extensions.Logging;
    using Model;

    public class EmailProcessor
    {
        private readonly IEmailRepository emailRepository;
        private readonly IEnumerable<IEmailSender> emailSenders;
        private readonly ILogger<EmailProcessor> logger;

        public EmailProcessor(
            IEmailRepository emailRepository, IEnumerable<IEmailSender> emailSenders, ILogger<EmailProcessor> logger)
        {
            this.emailRepository = emailRepository;
            this.emailSenders = emailSenders;
            this.logger = logger;
        }

        public async Task SendPending()
        {
            var sender = this.emailSenders.FirstOrDefault(s => s.CanSend);

            foreach (var emailQueueItem in this.emailRepository.GetUnsent())
            {
                if (sender != null)
                {
                    await sender.Send(
                        new Email(
                            emailQueueItem.To,
                            emailQueueItem.Subject,
                            emailQueueItem.HtmlBody,
                            emailQueueItem.PlainTextBody));
                }
                else
                {
                    this.logger.LogWarning(
                        $"Could not find an email sender to use to send email with subject {emailQueueItem.Subject} " +
                        $"to address {emailQueueItem.To}. " +
                        "This message will not be sent.");
                }

                this.emailRepository.MarkAsSent(emailQueueItem);
            }
        }

        private class Email : IEmail
        {
            public Email(string to, string subject, string htmlBody, string plainTextBody)
            {
                this.To = to;
                this.Subject = subject;
                this.HtmlBody = htmlBody;
                this.PlainTextBody = plainTextBody;
            }

            public string To { get; }

            public string Subject { get; }

            public string HtmlBody { get; }

            public string PlainTextBody { get; }
        }
    }
}