namespace ParkingRota.Business.Model
{
    using System.Collections.Generic;
    using Emails;

    public interface IEmailRepository
    {
        void AddToQueue(IEmail email);

        IReadOnlyList<EmailQueueItem> GetUnsent();

        void MarkAsSent(EmailQueueItem email);
    }
}