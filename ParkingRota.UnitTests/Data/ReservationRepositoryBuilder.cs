namespace ParkingRota.UnitTests.Data
{
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Data;

    public class ReservationRepositoryBuilder
    {
        private readonly Instant currentInstant;

        public ReservationRepositoryBuilder() : this(29.June(2019).At(9, 15, 26).Utc())
        {
        }

        private ReservationRepositoryBuilder(Instant currentInstant) =>
            this.currentInstant = currentInstant;

        public ReservationRepositoryBuilder WithCurrentInstant(Instant newCurrentInstant) =>
            new ReservationRepositoryBuilder(newCurrentInstant);

        public ReservationRepository Build(IApplicationDbContext context) =>
            new ReservationRepository(
                context,
                new DateCalculator(new FakeClock(this.currentInstant), BankHolidayRepositoryTests.CreateRepository(context)),
                MapperBuilder.Build());
    }
}