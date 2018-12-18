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

                body.Append($"<p>{string.Join("<br/>", this.AllocatedNames)}</p>");

                if (this.InterruptedNames.Any())
                {
                    body.Append($"<p>(Interrupted: {string.Join(", ", this.InterruptedNames)})</p>");
                }

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

                lines.AddRange(this.AllocatedNames);
                lines.Add(string.Empty);

                if (this.InterruptedNames.Any())
                {
                    lines.Add($"(Interrupted: {string.Join(", ", this.InterruptedNames)})");
                    lines.Add(string.Empty);
                }

                lines.Add(Footer);

                return string.Join("\n", lines);
            }
        }

        private IEnumerable<string> AllocatedNames => this.allocations
            .OrderBy(a => a.ApplicationUser.LastName)
            .Select(a => a.ApplicationUser.FullName);

        private IEnumerable<string> InterruptedNames => this.requests
            .Where(r => this.allocations.All(a => a.ApplicationUser.Id != r.ApplicationUser.Id))
            .OrderBy(i => i.ApplicationUser.LastName)
            .Select(i => i.ApplicationUser.FullName);

        private string FormattedDate => this.allocations.First().Date.ForDisplay();

        private string Header => $"Allocations for {this.FormattedDate}, after day-ahead spaces released:";

        private const string Footer =
            "If you no longer need a space allocated to you, " +
            "please remove your request so that it can be given to someone else.";
    }
}