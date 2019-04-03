namespace ParkingRota.Business
{
    using System.Threading.Tasks;
    using Emails;

    public interface IEmailSender
    {
        bool CanSend { get; }

        Task Send(IEmail email);
    }
}