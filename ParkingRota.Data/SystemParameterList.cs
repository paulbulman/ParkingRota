namespace ParkingRota.Data
{
    using System.ComponentModel.DataAnnotations;

    public class SystemParameterList
    {
        public int Id { get; set; }

        public int TotalSpaces { get; set; }

        public int ReservableSpaces { get; set; }

        public decimal NearbyDistance { get; set; }

        [Required]
        public string FromEmailAddress { get; set; }
    }
}