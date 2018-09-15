namespace ParkingRota.Business
{
    using System;
    using System.Linq;
    using Model;

    public interface IRegistrationTokenValidator
    {
        bool TokenIsValid(string token);
    }

    public class RegistrationTokenValidator : IRegistrationTokenValidator
    {
        private readonly IRegistrationTokenRepository registrationTokenRepository;

        public RegistrationTokenValidator(IRegistrationTokenRepository registrationTokenRepository) =>
            this.registrationTokenRepository = registrationTokenRepository;

        public bool TokenIsValid(string token) =>
            this.registrationTokenRepository.RegistrationTokens.Any(r =>
                string.Equals(r.Token, token, StringComparison.InvariantCultureIgnoreCase));
    }
}
