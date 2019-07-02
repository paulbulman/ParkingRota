namespace ParkingRota.Business
{
    using System;
    using System.Threading.Tasks;
    using EmailTemplates;
    using Model;
    using SendGrid;
    using SendGrid.Helpers.Mail;

    public class SendGridEmailSender : IEmailSender
    {
        private readonly ISystemParameterListRepository systemParameterListRepository;

        public SendGridEmailSender(ISystemParameterListRepository systemParameterListRepository) =>
            this.systemParameterListRepository = systemParameterListRepository;

        public bool CanSend => !string.IsNullOrEmpty(ApiKey);

        private static string ApiKey => Environment.GetEnvironmentVariable("SendGridApiKey");

        public async Task Send(IEmailTemplate emailTemplate)
        {
            var client = new SendGridClient(ApiKey);

            var fromEmailAddress = this.systemParameterListRepository.GetSystemParameterList().FromEmailAddress;

            await client.SendEmailAsync(
                MailHelper.CreateSingleEmail(
                    new EmailAddress(fromEmailAddress),
                    new EmailAddress(emailTemplate.To),
                    emailTemplate.Subject,
                    emailTemplate.PlainTextBody,
                    emailTemplate.HtmlBody));
        }
    }
}