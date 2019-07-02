namespace ParkingRota.UnitTests.Data
{
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Data;

    public class EmailRepositoryBuilder
    {
        private readonly Instant currentInstant;

        public EmailRepositoryBuilder() : this(29.June(2019).At(9, 15, 26).Utc())
        {
        }

        private EmailRepositoryBuilder(Instant currentInstant) =>
            this.currentInstant = currentInstant;

        public EmailRepositoryBuilder WithCurrentInstant(Instant newCurrentInstant) =>
            new EmailRepositoryBuilder(newCurrentInstant);

        public EmailRepository Build(IApplicationDbContext context) =>
            new EmailRepository(context, new FakeClock(this.currentInstant), MapperBuilder.Build());
    }
}