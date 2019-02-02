namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Business.ScheduledTasks;
    using Xunit;

    public static class BankHolidayUpdaterTests
    {
        [Fact]
        public static void Test_ScheduledTaskType()
        {
            var result = new BankHolidayUpdater(
                Mock.Of<IBankHolidayFetcher>(),
                Mock.Of<IBankHolidayRepository>(),
                Mock.Of<IDateCalculator>()).ScheduledTaskType;

            Assert.Equal(ScheduledTaskType.BankHolidayUpdater, result);
        }

        [Fact]
        public static async Task Test_Run()
        {
            // Arrange
            var existingBankHolidays = new[]
            {
                new BankHoliday { Date = 25.December(2018) },
                new BankHoliday { Date = 26.December(2018) }
            };

            var mockBankHolidayRepository = new Mock<IBankHolidayRepository>(MockBehavior.Strict);
            mockBankHolidayRepository.Setup(r => r.GetBankHolidays()).Returns(existingBankHolidays);
            mockBankHolidayRepository.Setup(r => r.AddBankHolidays(It.IsAny<IReadOnlyList<BankHoliday>>()));

            var newBankHolidayDates = new[] { 25.December(2019), 26.December(2019) };

            var bankHolidayDates = existingBankHolidays.Select(b => b.Date)
                .Concat(newBankHolidayDates)
                .ToArray();

            var mockBankHolidayFetcher = new Mock<IBankHolidayFetcher>(MockBehavior.Strict);
            mockBankHolidayFetcher
                .Setup(f => f.Fetch())
                .Returns(Task.FromResult((IReadOnlyList<LocalDate>)bankHolidayDates));

            // Act
            await new BankHolidayUpdater(
                mockBankHolidayFetcher.Object,
                mockBankHolidayRepository.Object,
                Mock.Of<IDateCalculator>()).Run();

            // Assert
            mockBankHolidayRepository.Verify(
                r => r.AddBankHolidays(It.Is<IReadOnlyList<BankHoliday>>(actual =>
                    actual.Select(b => b.Date).SequenceEqual(newBankHolidayDates))),
                Times.Once);
        }

        [Theory]
        [InlineData(18, 0, 25, 0)]
        [InlineData(19, 0, 25, 0)]
        [InlineData(25, 0, 31, 23)]
        public static void Test_GetNextRunTime(int currentDay, int currentHour, int expectedDay, int expectedHour)
        {
            // Arrange
            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .SetupGet(d => d.TimeZone)
                .Returns(DateTimeZoneProviders.Tzdb["Europe/London"]);

            var bankHolidayUpdater = new BankHolidayUpdater(
                Mock.Of<IBankHolidayFetcher>(),
                Mock.Of<IBankHolidayRepository>(),
                mockDateCalculator.Object);

            // Act
            var result = bankHolidayUpdater.GetNextRunTime(currentDay.March(2019).At(currentHour, 00, 00).Utc());

            // Assert
            var expected = expectedDay.March(2019).At(expectedHour, 00, 00).Utc();

            Assert.Equal(expected, result);
        }
    }
}