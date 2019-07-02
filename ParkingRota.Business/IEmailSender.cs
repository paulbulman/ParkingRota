namespace ParkingRota.Business
{
    using System.Threading.Tasks;
    using EmailTemplates;

    public interface IEmailSender
    {
        bool CanSend { get; }

        Task Send(IEmailTemplate emailTemplate);
    }
}