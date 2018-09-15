namespace ParkingRota.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using NodaTime;

    public class RegistrationToken
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public Instant ExpiryTime
        {
            get => DbConvert.Instant.FromDb(this.DbExpiryTime);
            set => this.DbExpiryTime = DbConvert.Instant.ToDb(value);
        }

        [Required]
        public DateTime DbExpiryTime { get; set; }
    }
}