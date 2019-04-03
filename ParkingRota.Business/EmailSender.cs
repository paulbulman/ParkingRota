namespace ParkingRota.Business
{
    using System;
    using System.Threading.Tasks;
    using Emails;
    using Model;
    using SendGrid;
    using SendGrid.Helpers.Mail;

    public class EmailSender : IEmailSender
    {
        private readonly ISystemParameterListRepository systemParameterListRepository;

        public EmailSender(ISystemParameterListRepository systemParameterListRepository) =>
            this.systemParameterListRepository = systemParameterListRepository;

        public async Task Send(IEmail email)
        {
            var apiKey = Environment.GetEnvironmentVariable("SendGridApiKey");

            if (!string.IsNullOrEmpty(apiKey))
            {
                var client = new SendGridClient(apiKey);

                var fromEmailAddress = this.systemParameterListRepository.GetSystemParameterList().FromEmailAddress;

                await client.SendEmailAsync(
                    MailHelper.CreateSingleEmail(
                        new EmailAddress(fromEmailAddress),
                        new EmailAddress(email.To),
                        email.Subject,
                        email.PlainTextBody,
                        email.HtmlBody));
            }
        }
    }
}