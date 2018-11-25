namespace ParkingRota.Data
{
    using Business.Model;
    using NodaTime;

    public class EmailRepository : IEmailRepository
    {
        private readonly IApplicationDbContext context;
        private readonly IClock clock;

        public EmailRepository(IApplicationDbContext context, IClock clock)
        {
            this.context = context;
            this.clock = clock;
        }

        public void AddToQueue(string to, string subject, string htmlBody, string plainTextBody)
        {
            var emailQueueItem = new EmailQueueItem
            {
                To = to,
                Subject = subject,
                HtmlBody = htmlBody,
                PlainTextBody = plainTextBody,
                AddedTime = this.clock.GetCurrentInstant()
            };

            this.context.EmailQueueItems.Add(emailQueueItem);
            this.context.SaveChanges();
        }
    }
}