namespace ParkingRota.Business
{
    public interface IRegistrationTokenValidator
    {
        bool TokenIsValid(string token);
    }
}
