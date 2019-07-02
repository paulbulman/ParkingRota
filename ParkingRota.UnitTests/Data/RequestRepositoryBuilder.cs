namespace ParkingRota.UnitTests.Data
{
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Data;

    public class RequestRepositoryBuilder
    {
        private readonly Instant currentInstant;

        public RequestRepositoryBuilder() : this(29.June(2019).At(9, 15, 26).Utc())
        {
        }

        private RequestRepositoryBuilder(Instant currentInstant) =>
            this.currentInstant = currentInstant;

        public RequestRepositoryBuilder WithCurrentInstant(Instant newCurrentInstant) =>
            new RequestRepositoryBuilder(newCurrentInstant);

        public RequestRepository Build(IApplicationDbContext context) =>
            new RequestRepository(
                context,
                new DateCalculator(new FakeClock(this.currentInstant), BankHolidayRepositoryTests.CreateRepository(context)),
                MapperBuilder.Build());
    }
}