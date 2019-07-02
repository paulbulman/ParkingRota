namespace ParkingRota.Business.EmailSenders
{
    using System.Threading.Tasks;
    using EmailTemplates;

    public interface IEmailSender
    {
        bool CanSend { get; }

        Task Send(IEmailTemplate emailTemplate);
    }
}