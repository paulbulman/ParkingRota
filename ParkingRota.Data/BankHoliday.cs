namespace ParkingRota.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using NodaTime;

    public class BankHoliday
    {
        public int Id { get; set; }

        public LocalDate Date
        {
            get => DbConvert.LocalDate.FromDb(this.DbDate);
            set => this.DbDate = DbConvert.LocalDate.ToDb(value);
        }

        [Required]
        public DateTime DbDate { get; set; }
    }
}