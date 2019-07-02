namespace ParkingRota.Business.EmailTemplates
{
    using System.Collections.Generic;
    using System.Text.Encodings.Web;

    public class ConfirmEmailAddress : IEmailTemplate
    {
        private readonly string unencodedCallbackUrl;
        private readonly string originatingIpAddress;

        public ConfirmEmailAddress(string to, string unencodedCallbackUrl, string originatingIpAddress)
        {
            this.To = to;

            this.unencodedCallbackUrl = unencodedCallbackUrl;
            this.originatingIpAddress = originatingIpAddress;
        }

        public string To { get; }

        public string Subject => "Confirm your email address";

        public string HtmlBody
        {
            get
            {
                var encodedCallbackUrl = HtmlEncoder.Default.Encode(this.unencodedCallbackUrl);

                return
                    "<p>Someone - hopefully you - registered this email address on the Parking Rota website.</p>" +
                    $"<p>If this was you, please confirm your account by <a href='{encodedCallbackUrl}'>clicking here</a>. If not, you can disregard this email.</p>" +
                    $"<p>The request originated from IP address {this.originatingIpAddress}</p>";
            }
        }

        public string PlainTextBody
        {
            get
            {
                var lines = new List<string>();

                lines.Add("Someone - hopefully you - registered this email address on the Parking Rota website.");
                lines.Add(string.Empty);

                lines.Add(
                    "If this was you, please confirm your account by copying the following link (removing line breaks) " +
                    "into your browser. If not, you can disregard this email.");
                lines.Add(string.Empty);

                lines.Add("Confirmation link:");
                lines.Add(this.unencodedCallbackUrl);
                lines.Add(string.Empty);

                lines.Add($"The request originated from IP address {this.originatingIpAddress}");

                return string.Join("\n", lines);
            }
        }
    }
}
