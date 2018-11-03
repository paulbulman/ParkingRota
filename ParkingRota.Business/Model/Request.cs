namespace ParkingRota.Business.Model
{
    using NodaTime;

    public class Request
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        public LocalDate Date { get; set; }
    }
}