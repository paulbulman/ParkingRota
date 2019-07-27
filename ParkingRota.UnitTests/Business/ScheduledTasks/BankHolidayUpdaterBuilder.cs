namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Data;
    using Moq;
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.ScheduledTasks;
    using ParkingRota.Data;

    public class BankHolidayUpdaterBuilder
    {
        private readonly Instant currentInstant;
        private readonly IReadOnlyList<LocalDate> returnedBankHolidayDates;

        public BankHolidayUpdaterBuilder() : this(29.June(2019).At(9, 15, 26).Utc(), new List<LocalDate>())
        {
        }

        private BankHolidayUpdaterBuilder(Instant currentInstant, IReadOnlyList<LocalDate> returnedBankHolidayDates)
        {
            this.currentInstant = currentInstant;
            this.returnedBankHolidayDates = returnedBankHolidayDates;
        }

        public BankHolidayUpdaterBuilder WithCurrentInstant(Instant newCurrentInstant) =>
            new BankHolidayUpdaterBuilder(newCurrentInstant, this.returnedBankHolidayDates);

        public BankHolidayUpdaterBuilder WithReturnedBankHolidayDates(IReadOnlyList<LocalDate> newReturnedBankHolidayDates) =>
            new BankHolidayUpdaterBuilder(this.currentInstant, newReturnedBankHolidayDates);

        public BankHolidayUpdater Build(IApplicationDbContext context)
        {
            var mockBankHolidayFetcher = new Mock<IBankHolidayFetcher>(MockBehavior.Strict);
            mockBankHolidayFetcher
                .Setup(f => f.Fetch())
                .Returns(Task.FromResult(this.returnedBankHolidayDates));

            return new BankHolidayUpdater(
                mockBankHolidayFetcher.Object,
                BankHolidayRepositoryTests.CreateRepository(context));
        }
    }
}