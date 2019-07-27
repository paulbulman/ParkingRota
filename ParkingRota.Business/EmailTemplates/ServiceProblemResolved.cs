namespace ParkingRota.Business.EmailTemplates
{
    using NodaTime;

    public class ServiceProblemResolved : IEmailTemplate
    {
        private readonly Instant lastRunTime;

        public ServiceProblemResolved(string to, Instant lastRunTime)
        {
            this.lastRunTime = lastRunTime;
            this.To = to;
        }

        public string To { get; }

        public string Subject => "Problem with parking rota service resolved";

        public string HtmlBody => $"<p>{this.PlainTextBody}</p>";

        public string PlainTextBody => $"The parking rota service has run successfully again at {this.lastRunTime.InZone(DateCalculator.LondonTimeZone).ForDisplay()}";
    }
}