namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using Data;
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.ScheduledTasks;
    using ParkingRota.Data;

    public class DailySummaryBuilder
    {
        private readonly Instant currentInstant;

        public DailySummaryBuilder() : this(29.June(2019).At(9, 15, 26).Utc())
        {
        }

        private DailySummaryBuilder(Instant currentInstant) =>
            this.currentInstant = currentInstant;

        public DailySummaryBuilder WithCurrentInstant(Instant newCurrentInstant) =>
            new DailySummaryBuilder(newCurrentInstant);

        public DailySummary Build(IApplicationDbContext context) =>
            new DailySummary(
                AllocationRepositoryTests.CreateRepository(context),
                new DateCalculator(new FakeClock(this.currentInstant), BankHolidayRepositoryTests.CreateRepository(context)),
                new EmailRepositoryBuilder().WithCurrentInstant(this.currentInstant).Build(context),
                new RequestRepositoryBuilder().WithCurrentInstant(this.currentInstant).Build(context));
    }
}