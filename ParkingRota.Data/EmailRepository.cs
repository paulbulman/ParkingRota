namespace ParkingRota.Data
{
    using Business.Emails;
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

        public void AddToQueue(IEmail email)
        {
            var emailQueueItem = new EmailQueueItem
            {
                To = email.To,
                Subject = email.Subject,
                HtmlBody = email.HtmlBody,
                PlainTextBody = email.PlainTextBody,
                AddedTime = this.clock.GetCurrentInstant()
            };

            this.context.EmailQueueItems.Add(emailQueueItem);
            this.context.SaveChanges();
        }
    }
}