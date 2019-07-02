namespace ParkingRota.UnitTests
{
    using Data;
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Data;

    public class RegistrationTokenValidatorBuilder
    {
        private readonly Instant currentInstant;

        public RegistrationTokenValidatorBuilder() : this(29.June(2019).At(9, 15, 26).Utc())
        {
        }

        private RegistrationTokenValidatorBuilder(Instant currentInstant) =>
            this.currentInstant = currentInstant;

        public RegistrationTokenValidatorBuilder WithCurrentInstant(Instant newCurrentInstant) =>
            new RegistrationTokenValidatorBuilder(newCurrentInstant);

        public RegistrationTokenValidator Build(IApplicationDbContext context) =>
            new RegistrationTokenValidator(
                new FakeClock(this.currentInstant),
                RegistrationTokenRepositoryTests.CreateRepository(context));
    }
}