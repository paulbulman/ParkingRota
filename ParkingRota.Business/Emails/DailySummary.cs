namespace ParkingRota.Business.Emails
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Model;

    public class DailySummary : IEmail
    {
        private readonly IReadOnlyList<Allocation> allocations;
        private readonly IReadOnlyList<Request> requests;

        public DailySummary(string to, IReadOnlyList<Allocation> allocations, IReadOnlyList<Request> requests)
        {
            this.allocations = allocations;
            this.requests = requests;
            this.To = to;
        }

        public string To { get; }

        public string Subject => $"Daily allocations summary for {this.FormattedDate}";

        public string HtmlBody
        {
            get
            {
                var body = new StringBuilder();

                body.Append($"<p>{this.Header}</p>");

                body.Append(GetHtmlSummary(this.allocations, this.requests));

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

                lines.AddRange(GetPlainTextSummary(this.allocations, this.requests));

                lines.Add(Footer);

                return string.Join("\n", lines);
            }
        }

        public static string GetHtmlSummary(IReadOnlyList<Allocation> allocations, IReadOnlyList<Request> requests)
        {
            var summary = new StringBuilder();

            summary.Append($"<p>{string.Join("<br/>", GetAllocatedNames(allocations))}</p>");

            var interruptedNames = GetInterruptedNames(allocations, requests);

            if (interruptedNames.Any())
            {
                summary.Append($"<p>(Interrupted: {string.Join(", ", interruptedNames)})</p>");
            }

            return summary.ToString();
        }

        public static IReadOnlyList<string> GetPlainTextSummary(
            IReadOnlyList<Allocation> allocations,
            IReadOnlyList<Request> requests)
        {
            var summary = new List<string>();

            summary.AddRange(GetAllocatedNames(allocations));
            summary.Add(string.Empty);

            var interruptedNames = GetInterruptedNames(allocations, requests);

            if (interruptedNames.Any())
            {
                summary.Add($"(Interrupted: {string.Join(", ", interruptedNames)})");
                summary.Add(string.Empty);
            }

            return summary;
        }

        private static IReadOnlyList<string> GetAllocatedNames(IReadOnlyList<Allocation> allocations) => allocations
            .OrderBy(a => a.ApplicationUser.LastName)
            .Select(a => a.ApplicationUser.FullName)
            .ToArray();

        private static IReadOnlyList<string> GetInterruptedNames(
                IReadOnlyList<Allocation> allocations, IReadOnlyList<Request> requests) => requests
            .Where(r => allocations.All(a => a.ApplicationUser.Id != r.ApplicationUser.Id))
            .OrderBy(i => i.ApplicationUser.LastName)
            .Select(i => i.ApplicationUser.FullName)
            .ToArray();

        private string FormattedDate => this.allocations.First().Date.ForDisplay();

        private string Header => $"Allocations for {this.FormattedDate}, after day-ahead spaces released:";

        private const string Footer =
            "If you no longer need a space allocated to you, " +
            "please remove your request so that it can be given to someone else.";
    }
}