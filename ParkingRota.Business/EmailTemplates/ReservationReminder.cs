namespace ParkingRota.Business.EmailTemplates
{
    using NodaTime;

    public class ReservationReminder : IEmailTemplate
    {
        private readonly LocalDate localDate;

        public ReservationReminder(string to, LocalDate localDate)
        {
            this.localDate = localDate;
            this.To = to;
        }

        public string To { get; }

        public string Subject => $"No reservations entered for {this.localDate.ForDisplayWithDayOfWeek()}";

        public string HtmlBody => $"<p>{this.PlainTextBody}</p>";

        public string PlainTextBody =>
            $"No reservations have yet been entered for {this.localDate.ForDisplayWithDayOfWeek()}." +
            " If no spaces need reserving for this date then you can ignore this message." +
            " Otherwise, you should enter reservations by 11am to ensure spaces are allocated accordingly.";
    }
}