namespace ParkingRota.Business.Model
{
    using Emails;

    public interface IEmailRepository
    {
        void AddToQueue(IEmail email);
    }
}