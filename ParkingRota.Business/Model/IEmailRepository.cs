namespace ParkingRota.Business.Model
{
    public interface IEmailRepository
    {
        void AddToQueue(string to, string subject, string htmlBody, string plainTextBody);
    }
}