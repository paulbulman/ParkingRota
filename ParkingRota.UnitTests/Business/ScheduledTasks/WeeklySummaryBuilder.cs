namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using Data;
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.ScheduledTasks;
    using ParkingRota.Data;

    public class WeeklySummaryBuilder
    {
        private readonly Instant currentInstant;

        public WeeklySummaryBuilder() : this(29.June(2019).At(9, 15, 26).Utc())
        {
        }

        private WeeklySummaryBuilder(Instant currentInstant) =>
            this.currentInstant = currentInstant;

        public WeeklySummaryBuilder WithCurrentInstant(Instant newCurrentInstant) =>
            new WeeklySummaryBuilder(newCurrentInstant);

        public WeeklySummary Build(IApplicationDbContext context) =>
            new WeeklySummary(
                AllocationRepositoryTests.CreateRepository(context),
                new DateCalculator(new FakeClock(this.currentInstant), BankHolidayRepositoryTests.CreateRepository(context)),
                new EmailRepositoryBuilder().WithCurrentInstant(this.currentInstant).Build(context),
                new RequestRepositoryBuilder().WithCurrentInstant(this.currentInstant).Build(context));
    }
}