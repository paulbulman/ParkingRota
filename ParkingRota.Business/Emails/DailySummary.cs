namespace ParkingRota.Business.Emails
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Model;

    public class DailySummary : IEmail
    {
        private readonly ApplicationUser recipient;
        private readonly IReadOnlyList<Allocation> allocations;
        private readonly IReadOnlyList<Request> requests;

        public DailySummary(ApplicationUser recipient, IReadOnlyList<Allocation> allocations, IReadOnlyList<Request> requests)
        {
            this.recipient = recipient;
            this.allocations = allocations;
            this.requests = requests;
        }

        public string To => this.recipient.Email;

        public string Subject
        {
            get
            {
                var status = this.allocations.Any(a => a.ApplicationUser == this.recipient) ? "Allocated" : "INTERRUPTED";
                return $"[{status}] {this.FormattedDate} Daily allocations summary";
            }
        }

        public string HtmlBody
        {
            get
            {
                var body = new StringBuilder();

                body.Append($"<p>{this.Header}</p>");

                body.Append(GetHtmlSummary(this.recipient, this.allocations, this.requests));

                body.Append($"<p>{Footer}</p>");

                return body.ToString();
            }
        }

        public string PlainTextBody
        {
            get
            {
                var lines = new List<string>();

                lines.Add(this.Header);
                lines.Add(string.Empty);

                lines.AddRange(GetPlainTextSummary(this.recipient, this.allocations, this.requests));

                lines.Add(Footer);

                return string.Join("\n", lines);
            }
        }

        public static string GetHtmlSummary(
            ApplicationUser recipient,
            IReadOnlyList<Allocation> allocations,
            IReadOnlyList<Request> requests)
        {
            var summary = new StringBuilder();

            summary.Append($"<p>{string.Join("<br/>", GetAllocatedNames(recipient, allocations, BodyType.Html))}</p>");

            var interruptedNames = GetInterruptedNames(recipient, allocations, requests, BodyType.Html);

            if (interruptedNames.Any())
            {
                summary.Append($"<p>(Interrupted: {string.Join(", ", interruptedNames)})</p>");
            }

            return summary.ToString();
        }

        public static IReadOnlyList<string> GetPlainTextSummary(
            ApplicationUser recipient,
            IReadOnlyList<Allocation> allocations,
            IReadOnlyList<Request> requests)
        {
            var summary = new List<string>();

            summary.AddRange(GetAllocatedNames(recipient, allocations, BodyType.PlainText));
            summary.Add(string.Empty);

            var interruptedNames = GetInterruptedNames(recipient, allocations, requests, BodyType.PlainText);

            if (interruptedNames.Any())
            {
                summary.Add($"(Interrupted: {string.Join(", ", interruptedNames)})");
                summary.Add(string.Empty);
            }

            return summary;
        }

        private static IReadOnlyList<string> GetAllocatedNames(
            ApplicationUser recipient,
            IReadOnlyList<Allocation> allocations,
            BodyType bodyType)
            => allocations
                .OrderBy(a => a.ApplicationUser.LastName)
                .Select(a => GetName(recipient, a.ApplicationUser, bodyType))
                .ToArray();

        private static string GetName(ApplicationUser recipient, ApplicationUser allocatedUser, BodyType bodyType)
        {
            if (allocatedUser == recipient)
            {
                switch (bodyType)
                {
                    case BodyType.Html:
                        return $"<strong>{allocatedUser.FullName}</strong>";
                    case BodyType.PlainText:
                        return $"*{allocatedUser.FullName}*";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(bodyType));
                }
            }
            else
            {
                return allocatedUser.FullName;
            }
        }

        private static IReadOnlyList<string> GetInterruptedNames(
            ApplicationUser recipient,
            IReadOnlyList<Allocation> allocations,
            IReadOnlyList<Request> requests,
            BodyType bodyType)
            => requests
                .Where(r => allocations.All(a => a.ApplicationUser.Id != r.ApplicationUser.Id))
                .OrderBy(i => i.ApplicationUser.LastName)
                .Select(i => GetName(recipient, i.ApplicationUser, bodyType))
                .ToArray();

        private string FormattedDate => this.allocations.First().Date.ForDisplay();

        private string Header => $"Allocations for {this.FormattedDate}, after day-ahead spaces released:";

        private const string Footer =
            "If you no longer need a space allocated to you, " +
            "please remove your request so that it can be given to someone else.";

        private enum BodyType
        {
            Html,
            PlainText
        }
    }
}
