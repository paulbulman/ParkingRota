namespace ParkingRota.Business.Model
{
    using System.Collections.Generic;
    using EmailTemplates;

    public interface IEmailRepository
    {
        void AddToQueue(IEmailTemplate emailTemplate);

        IReadOnlyList<EmailQueueItem> GetRecent();

        IReadOnlyList<EmailQueueItem> GetUnsent();

        void MarkAsSent(EmailQueueItem email);
    }
}