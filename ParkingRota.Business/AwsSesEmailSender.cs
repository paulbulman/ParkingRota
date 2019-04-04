namespace ParkingRota.Business
{
    using System;
    using System.Net;
    using System.Net.Mail;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Emails;
    using Model;

    public class AwsSesEmailSender : IEmailSender
    {
        private readonly ISystemParameterListRepository systemParameterListRepository;

        public AwsSesEmailSender(ISystemParameterListRepository systemParameterListRepository) =>
            this.systemParameterListRepository = systemParameterListRepository;

        public bool CanSend =>
            !string.IsNullOrEmpty(Host) &&
            !string.IsNullOrEmpty(Username) &&
            !string.IsNullOrEmpty(Password) &&
            !string.IsNullOrEmpty(ConfigSet);

        private static string Host => Environment.GetEnvironmentVariable("SmtpHost");

        private static string Username => Environment.GetEnvironmentVariable("SmtpUsername");

        private static string Password => Environment.GetEnvironmentVariable("SmtpPassword");

        private static string ConfigSet => Environment.GetEnvironmentVariable("SmtpConfigSet");

        public async Task Send(IEmail email)
        {
            var fromEmailAddress = this.systemParameterListRepository.GetSystemParameterList().FromEmailAddress;

            var message = new MailMessage(fromEmailAddress, email.To, email.Subject, email.PlainTextBody);

            message.AlternateViews.Add(
                AlternateView.CreateAlternateViewFromString(
                    email.HtmlBody,
                    new ContentType("text/html")));

            message.Headers.Add("X-SES-CONFIGURATION-SET", ConfigSet);

            const int Port = 587;
            using (var client = new SmtpClient(Host, Port))
            {
                client.Credentials = new NetworkCredential(Username, Password);
                client.EnableSsl = true;

                // Ensure we stay within AWS sending rate limit
                Thread.Sleep(TimeSpan.FromMilliseconds(100));

                await client.SendMailAsync(message);
            }
        }
    }
}