namespace ParkingRota.Business
{
    using System;
    using System.Linq;
    using Model;
    using NodaTime;

    public interface IRegistrationTokenValidator
    {
        bool TokenIsValid(string token);
    }

    public class RegistrationTokenValidator : IRegistrationTokenValidator
    {
        private readonly IClock clock;
        private readonly IRegistrationTokenRepository registrationTokenRepository;

        public RegistrationTokenValidator(IClock clock, IRegistrationTokenRepository registrationTokenRepository)
        {
            this.clock = clock;
            this.registrationTokenRepository = registrationTokenRepository;
        }

        public bool TokenIsValid(string token) =>
            this.registrationTokenRepository.GetRegistrationTokens().Any(r =>
                string.Equals(r.Token, token, StringComparison.InvariantCultureIgnoreCase) &&
                this.clock.GetCurrentInstant() < r.ExpiryTime);
    }
}
