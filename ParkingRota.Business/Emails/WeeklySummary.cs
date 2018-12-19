﻿namespace ParkingRota.Business.Emails
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Model;
    using NodaTime;

    public class WeeklySummary : IEmail
    {
        private readonly IReadOnlyList<Allocation> allocations;
        private readonly IReadOnlyList<Request> requests;

        public WeeklySummary(string to, IReadOnlyList<Allocation> allocations, IReadOnlyList<Request> requests)
        {
            this.allocations = allocations;
            this.requests = requests;
            this.To = to;
        }

        public string To { get; }

        public string Subject => $"Weekly provisional allocations summary for {this.FormattedDateRange}";

        public string HtmlBody
        {
            get
            {
                var body = new StringBuilder();

                body.Append($"<p>{this.Header}</p>");

                foreach (var localDate in this.OrderedDates)
                {
                    body.Append($"<p>{localDate.ForDisplay()}:</p>");
                    body.Append(DailySummary.GetHtmlSummary(
                        this.GetDailyAllocations(localDate), this.GetDailyRequests(localDate)));
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

                foreach (var localDate in this.OrderedDates)
                {
                    lines.Add($"{localDate.ForDisplay()}:");
                    lines.Add(string.Empty);
                    lines.AddRange(DailySummary.GetPlainTextSummary(
                        this.GetDailyAllocations(localDate), this.GetDailyRequests(localDate)));
                }

                lines.Add(Footer);

                return string.Join("\n", lines);
            }
        }

        private string Header => $"Provisional allocations for {this.FormattedDateRange}, before day-ahead spaces released:";

        private const string Footer =
            "Note that further spaces are released at 11am each working day, for the subsequent working day. " +
            "If you no longer need a space allocated to you, " +
            "please remove your request so that it can be given to someone else.";

        private string FormattedDateRange =>
            $"{this.OrderedDates.First().ForDisplay()} - {this.OrderedDates.Last().ForDisplay()}";

        private IReadOnlyList<LocalDate> OrderedDates => this.allocations
            .Select(a => a.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToArray();


        private IReadOnlyList<Request> GetDailyRequests(LocalDate localDate) =>
            this.requests.Where(r => r.Date == localDate).ToArray();

        private IReadOnlyList<Allocation> GetDailyAllocations(LocalDate localDate) =>
            this.allocations.Where(a => a.Date == localDate).ToArray();
    }
}