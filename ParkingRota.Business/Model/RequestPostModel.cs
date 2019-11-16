namespace ParkingRota.Business.Model
{
    using NodaTime;

    public class RequestPostModel
    {
        public int Id { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        public LocalDate Date { get; set; }
    }
}