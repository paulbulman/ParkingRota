namespace ParkingRota.Business
{
    using System.Threading.Tasks;
    using Emails;

    public interface IEmailSender
    {
        Task Send(IEmail email);
    }
}