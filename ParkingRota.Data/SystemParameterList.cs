namespace ParkingRota.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using NodaTime;

    public class SystemParameterList
    {
        public int Id { get; set; }

        public int TotalSpaces { get; set; }

        public int ReservableSpaces { get; set; }

        public decimal NearbyDistance { get; set; }

        [Required]
        public string FromEmailAddress { get; set; }

        public Instant LastServiceRunTime
        {
            get => DbConvert.Instant.FromDb(this.DbLastServiceRunTime);
            set => this.DbLastServiceRunTime = DbConvert.Instant.ToDb(value);
        }

        public DateTime DbLastServiceRunTime { get; set; }
    }
}