namespace ParkingRota.Business.Emails
{
    using System.Collections.Generic;
    using System.Text.Encodings.Web;

    public class Signup : IEmail
    {
        private readonly string unencodedCallbackUrl;
        private readonly string originatingIpAddress;

        public Signup(string to, string unencodedCallbackUrl, string originatingIpAddress)
        {
            this.To = to;

            this.unencodedCallbackUrl = unencodedCallbackUrl;
            this.originatingIpAddress = originatingIpAddress;
        }

        public string To { get; }

        public string Subject => "Parking Rota registration";

        public string HtmlBody
        {
            get
            {
                var encodedCallbackUrl = HtmlEncoder.Default.Encode(this.unencodedCallbackUrl);

                return
                    "<p>A registration token has been generated for you to create an account on the Parking Rota website.</p>" +
                    $"<p>If you were expecting this, create an account by <a href='{encodedCallbackUrl}'>clicking here</a>." +
                    "If not, you can disregard this email. The link will be valid for 24 hours.</p>" +
                    $"<p>The request originated from IP address {this.originatingIpAddress}</p>";
            }
        }

        public string PlainTextBody
        {
            get
            {
                var lines = new List<string>();

                lines.Add("A registration token has been generated for you to create an account on the Parking Rota website.");
                lines.Add(string.Empty);

                lines.Add(
                    "If you were expecting this, create an account by copying the following link (removing line breaks) " +
                    "into your browser. If not, you can disregard this email. The link will be valid for 24 hours.");
                lines.Add(string.Empty);

                lines.Add("Registration link:");
                lines.Add(this.unencodedCallbackUrl);
                lines.Add(string.Empty);

                lines.Add($"The request originated from IP address {this.originatingIpAddress}");

                return string.Join("\n", lines);
            }
        }
    }
}