namespace ParkingRota.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Business.Emails;
    using Business.Model;
    using NodaTime;

    public class EmailRepository : IEmailRepository
    {
        private readonly IApplicationDbContext context;
        private readonly IClock clock;
        private readonly IMapper mapper;

        public EmailRepository(IApplicationDbContext context, IClock clock, IMapper mapper)
        {
            this.context = context;
            this.clock = clock;
            this.mapper = mapper;
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

        public IReadOnlyList<Business.Model.EmailQueueItem> GetRecent()
        {
            var threshold = this.clock.GetCurrentInstant().Minus(Duration.FromMinutes(30)).ToDateTimeUtc();

            return this.context.EmailQueueItems
                .Where(e => e.DbAddedTime >= threshold)
                .OrderBy(e => e.AddedTime)
                .ToArray()
                .Select(this.mapper.Map<Business.Model.EmailQueueItem>)
                .ToArray();
        }

        public IReadOnlyList<Business.Model.EmailQueueItem> GetUnsent() =>
            this.context.EmailQueueItems
                .Where(e => e.DbSentTime == null)
                .OrderBy(e => e.AddedTime)
                .ToArray()
                .Select(this.mapper.Map<Business.Model.EmailQueueItem>)
                .ToArray();

        public void MarkAsSent(Business.Model.EmailQueueItem email)
        {
            this.context.EmailQueueItems.Single(e => e.Id == email.Id).SentTime = this.clock.GetCurrentInstant();
            this.context.SaveChanges();
        }
    }
}