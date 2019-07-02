namespace ParkingRota.Business.EmailSenders
{
    using System;
    using System.Net.Mail;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using EmailTemplates;
    using Model;

    public class LocalPapercutEmailSender : IEmailSender
    {
        private readonly ISystemParameterListRepository systemParameterListRepository;
        
        public bool CanSend => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("UseLocalPapercutSender"));

        public LocalPapercutEmailSender(ISystemParameterListRepository systemParameterListRepository) =>
            this.systemParameterListRepository = systemParameterListRepository;

        public async Task Send(IEmailTemplate emailTemplate)
        {
            var fromEmailAddress = this.systemParameterListRepository.GetSystemParameterList().FromEmailAddress;

            var message = new MailMessage(
                fromEmailAddress,
                emailTemplate.To,
                emailTemplate.Subject,
                emailTemplate.PlainTextBody);

            message.AlternateViews.Add(
                AlternateView.CreateAlternateViewFromString(
                    emailTemplate.HtmlBody,
                    new ContentType("text/html")));

            using (var client = new SmtpClient("localhost"))
            {
                await client.SendMailAsync(message);
            }
        }
    }
}