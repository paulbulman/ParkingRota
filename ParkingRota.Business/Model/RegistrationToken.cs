namespace ParkingRota.Business.Model
{
    using NodaTime;

    public class RegistrationToken
    {
        public int Id { get; set; }

        public string Token { get; set; }

        public Instant ExpiryTime { get; set; }
    }
}