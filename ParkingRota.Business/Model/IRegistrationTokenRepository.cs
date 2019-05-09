namespace ParkingRota.Business.Model
{
    using System.Collections.Generic;

    public interface IRegistrationTokenRepository
    {
        IReadOnlyList<RegistrationToken> GetRegistrationTokens();

        void AddRegistrationToken(RegistrationToken registrationToken);

        void DeleteRegistrationToken(string token);
    }
}