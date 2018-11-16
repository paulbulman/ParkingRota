namespace ParkingRota.Business.Model
{
    using NodaTime;

    public class Reservation
    {
        public int Id { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        public LocalDate Date { get; set; }

        public int Order { get; set; }
    }
}