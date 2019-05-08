namespace ParkingRota.Business.Model
{
    using System.Collections.Generic;

    public interface IRegistrationTokenRepository
    {
        IReadOnlyList<RegistrationToken> GetRegistrationTokens();

        void DeleteRegistrationToken(string token);
    }
}