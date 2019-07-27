namespace ParkingRota.Business.EmailTemplates
{
    using NodaTime;

    public class ServiceProblemWarning : IEmailTemplate
    {
        private readonly Instant lastRunTime;

        public ServiceProblemWarning(string to, Instant lastRunTime)
        {
            this.lastRunTime = lastRunTime;
            this.To = to;
        }

        public string To { get; }

        public string Subject => "Problem with parking rota service";

        public string HtmlBody => $"<p>{this.PlainTextBody}</p>";

        public string PlainTextBody => $"The parking rota service has not run since {this.lastRunTime.InZone(DateCalculator.LondonTimeZone).ForDisplay()}";
    }
}