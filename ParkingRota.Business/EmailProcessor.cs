namespace ParkingRota.Business
{
    using System.Threading.Tasks;
    using Emails;
    using Model;

    public class EmailProcessor
    {
        private readonly IEmailRepository emailRepository;
        private readonly IEmailSender emailSender;

        public EmailProcessor(IEmailRepository emailRepository, IEmailSender emailSender)
        {
            this.emailRepository = emailRepository;
            this.emailSender = emailSender;
        }

        public async Task SendPending()
        {
            foreach (var emailQueueItem in this.emailRepository.GetUnsent())
            {
                if (this.emailSender.CanSend)
                {
                    await this.emailSender.Send(
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