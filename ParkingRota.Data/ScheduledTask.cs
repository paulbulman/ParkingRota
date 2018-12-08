namespace ParkingRota.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Business.Model;
    using NodaTime;

    public class ScheduledTask
    {
        public int Id { get; set; }

        public ScheduledTaskType ScheduledTaskType { get; set; }

        public Instant NextRunTime
        {
            get => DbConvert.Instant.FromDb(this.DbNextRunTime);
            set => this.DbNextRunTime = DbConvert.Instant.ToDb(value);
        }

        [Required]
        public DateTime DbNextRunTime { get; set; }
    }
}