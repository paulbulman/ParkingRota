namespace ParkingRota.Data
{
    public class RegistrationToken
    {
        public int Id { get; set; }

        public string Token { get; set; }

        public ParkingRota.Business.Model.RegistrationToken ToModel() =>
            new Business.Model.RegistrationToken
            {
                Id = this.Id,
                Token = this.Token
            };
    }
}