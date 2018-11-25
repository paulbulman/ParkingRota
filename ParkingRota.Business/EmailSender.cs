namespace ParkingRota.Business
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Model;

    public class EmailSender : IEmailSender
    {
        private readonly IEmailRepository emailRepository;

        public EmailSender(IEmailRepository emailRepository) =>
            this.emailRepository = emailRepository;

        public Task SendEmailAsync(string email, string subject, string htmlMessage) =>
            Task.Run(() => this.emailRepository.AddToQueue(email, subject, htmlMessage, plainTextBody: null));
    }
}