namespace ParkingRota.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Emails;
    using Model;

    public class EmailProcessor
    {
        private readonly IEmailRepository emailRepository;
        private readonly IEnumerable<IEmailSender> emailSenders;

        public EmailProcessor(IEmailRepository emailRepository, IEnumerable<IEmailSender> emailSenders)
        {
            this.emailRepository = emailRepository;
            this.emailSenders = emailSenders;
        }

        public async Task SendPending()
        {
            foreach (var emailQueueItem in this.emailRepository.GetUnsent())
            {
                var sender = this.emailSenders.FirstOrDefault(s => s.CanSend);

                if (sender != null)
                {
                    await sender.Send(
                        new Email(
                            emailQueueItem.To,
                            emailQueueItem.Subject,
                            emailQueueItem.HtmlBody,
                            emailQueueItem.PlainTextBody));
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