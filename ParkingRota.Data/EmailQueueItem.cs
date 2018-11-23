namespace ParkingRota.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using NodaTime;

    public class EmailQueueItem
    {
        public int Id { get; set; }

        [Required]
        public string To { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string HtmlBody { get; set; }

        [Required]
        public string PlainTextBody { get; set; }

        [Required]
        public Instant AddedTime
        {
            get => DbConvert.Instant.FromDb(this.DbAddedTime);
            set => this.DbAddedTime = DbConvert.Instant.ToDb(value);
        }

        [Required]
        public DateTime DbAddedTime { get; set; }

        public Instant SentTime
        {
            get => DbConvert.Instant.FromDb(this.DbSentTime);
            set => this.DbSentTime = DbConvert.Instant.ToDb(value);
        }

        public DateTime DbSentTime { get; set; }
    }
}