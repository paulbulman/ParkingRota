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

        public string Subject { get; set; }

        public string HtmlBody { get; set; }

        public string PlainTextBody { get; set; }

        [Required]
        public Instant AddedTime
        {
            get => DbConvert.Instant.FromDb(this.DbAddedTime);
            set => this.DbAddedTime = DbConvert.Instant.ToDb(value);
        }

        [Required]
        public DateTime DbAddedTime { get; set; }
    }
}