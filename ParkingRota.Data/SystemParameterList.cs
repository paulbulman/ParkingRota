namespace ParkingRota.Data
{
    public class SystemParameterList
    {
        public int Id { get; set; }

        public int TotalSpaces { get; set; }

        public int ReservableSpaces { get; set; }

        public decimal NearbyDistance { get; set; }
    }
}