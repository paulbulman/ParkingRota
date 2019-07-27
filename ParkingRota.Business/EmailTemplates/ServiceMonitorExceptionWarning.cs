namespace ParkingRota.Business.EmailTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Text.Encodings.Web;

    public class ServiceMonitorExceptionWarning : IEmailTemplate
    {
        private readonly Exception exception;

        public ServiceMonitorExceptionWarning(string to, Exception exception)
        {
            this.exception = exception;
            this.To = to;
        }

        public string To { get; }

        public string Subject => "Problem with parking rota service monitor";

        public string HtmlBody =>
            "<p>An error was encountered whilst checking the parking rota service last run time.</p>" +
            "<p>This does not necessarily indicate a problem with the service, " +
            "but the most probable cause of the error is database/network connectivity, " +
            "which would likely affect the service as well.</p>" +
            "<p>The exception message was:<br />" +
            HtmlEncoder.Default.Encode(this.exception.Message) +
            "</p>";

        public string PlainTextBody
        {
            get
            {
                var lines = new List<string>();

                lines.Add("An error was encountered whilst checking the parking rota service last run time.");
                lines.Add(string.Empty);

                lines.Add(
                    "This does not necessarily indicate a problem with the service, " +
                    "but the most probable cause of the error is database/network connectivity, " +
                    "which would likely affect the service as well.");
                lines.Add(string.Empty);

                lines.Add("The exception message was:");
                lines.Add(this.exception.Message);

                return string.Join("\n", lines);
            }
        }
    }
}