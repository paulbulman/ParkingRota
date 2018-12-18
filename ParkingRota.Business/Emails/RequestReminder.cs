namespace ParkingRota.Business.Emails
{
    using NodaTime;

    public class RequestReminder : IEmail
    {
        private readonly LocalDate firstDate;
        private readonly LocalDate lastDate;

        public RequestReminder(string to, LocalDate firstDate, LocalDate lastDate)
        {
            this.To = to;

            this.firstDate = firstDate;
            this.lastDate = lastDate;
        }

        public string To { get; }

        public string Subject => $"No requests entered for {this.firstDate.ForDisplay()} - {this.lastDate.ForDisplay()}";

        public string HtmlBody => $"<p>{this.PlainTextBody}</p>";

        public string PlainTextBody =>
            $"No requests have yet been entered for {this.firstDate.ForDisplay()} - {this.lastDate.ForDisplay()}." +
            " If you do not need parking during this period you can ignore this message." +
            " Otherwise, you should enter requests by the end of today to have them taken into account.";
    }
}