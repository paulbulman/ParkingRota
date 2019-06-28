namespace ParkingRota.Business.Model
{
    using NodaTime;

    public class SystemParameterList
    {
        public int Id { get; set; }

        public int TotalSpaces { get; set; }

        public int ReservableSpaces { get; set; }

        public decimal NearbyDistance { get; set; }

        public string FromEmailAddress { get; set; }

        public Instant LastServiceRunTime { get; set; }
    }
}