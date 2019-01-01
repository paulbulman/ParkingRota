namespace ParkingRota.Business.Model
{
    using NodaTime;

    public class EmailQueueItem
    {
        public int Id { get; set; }

        public string To { get; set; }

        public string Subject { get; set; }

        public string HtmlBody { get; set; }

        public string PlainTextBody { get; set; }

        public Instant AddedTime { get; set; }

        public Instant? SentTime { get; set; }
    }
}