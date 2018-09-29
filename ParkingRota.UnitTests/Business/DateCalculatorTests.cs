namespace ParkingRota.UnitTests.Business
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Moq;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using Xunit;

    public static class DateCalculatorTests
    {
        private static readonly DateTimeZone UkTimeZone = DateTimeZoneProviders.Tzdb["Europe/London"];

        [Theory]
        [InlineData(1, 1, 42)]
        [InlineData(4, 5, 40)]
        public static void Test_ActiveDates_NextMonthEndsOnWeekend(int currentDay, int expectedFirstDay, int expectedTotalDays)
        {
            var currentLocalDateTime = new LocalDateTime(2018, 2, currentDay, 0, 0);

            var expectedFirstLocalDate = expectedFirstDay.February(2018);
            var expectedLastLocalDate = 30.March(2018);

            Check_ActiveDates(currentLocalDateTime, expectedTotalDays, expectedFirstLocalDate, expectedLastLocalDate);
        }

        [Theory]
        [InlineData(1, 1, 43)]
        [InlineData(4, 5, 41)]
        public static void Test_ActiveDates_NextMonthEndsOnWeeday(int currentDay, int expectedFirstDay, int expectedTotalDays)
        {
            var currentLocalDateTime = new LocalDateTime(2018, 3, currentDay, 0, 0);

            var expectedFirstLocalDate = expectedFirstDay.March(2018);
            var expectedLastLocalDate = 30.April(2018);

            Check_ActiveDates(currentLocalDateTime, expectedTotalDays, expectedFirstLocalDate, expectedLastLocalDate);
        }

        [Fact]
        public static void Test_ActiveDates_BankHoliday()
        {
            var bankHolidayLocalDates = new[] { 30.March(2018), 2.April(2018) };
            var bankHolidays = bankHolidayLocalDates.Select(b => new BankHoliday { Date = b }).ToArray();

            var currentLocalDateTime = new LocalDateTime(2018, 3, 17, 0, 0);

            const int ExpectedTotalDays = 29;
            var expectedFirstLocalDate = 19.March(2018);
            var expectedLastLocalDate = 30.April(2018);

            var result = Check_ActiveDates(
                currentLocalDateTime, ExpectedTotalDays, expectedFirstLocalDate, expectedLastLocalDate, bankHolidays);

            foreach (var bankHoliday in bankHolidayLocalDates)
            {
                Assert.DoesNotContain(bankHoliday, result);
            }
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
        private static IReadOnlyList<LocalDate> Check_ActiveDates(
            LocalDateTime currentLocalDateTime,
            int expectedTotalDays,
            LocalDate expectedFirstLocalDate,
            LocalDate expectedLastLocalDate,
            params BankHoliday[] bankHolidays)
        {
            var result = new DateCalculator(
                CreateMockClock(currentLocalDateTime),
                CreateMockBankHolidayRepository(bankHolidays)).ActiveDates;

            Assert.Equal(expectedTotalDays, result.Count);
            Assert.Equal(expectedFirstLocalDate, result.First());
            Assert.Equal(expectedLastLocalDate, result.Last());

            return result;
        }

        private static IClock CreateMockClock(LocalDateTime currentLocalDateTime)
        {
            var mockClock = new Mock<IClock>(MockBehavior.Strict);

            mockClock
                .Setup(c => c.GetCurrentInstant())
                .Returns(currentLocalDateTime.InZoneStrictly(UkTimeZone).ToInstant());

            return mockClock.Object;
        }

        private static IBankHolidayRepository CreateMockBankHolidayRepository(params BankHoliday[] bankHolidays)
        {
            var mockBankHolidayRepository = new Mock<IBankHolidayRepository>(MockBehavior.Strict);

            mockBankHolidayRepository
                .SetupGet(r => r.BankHolidays)
                .Returns(bankHolidays);

            return mockBankHolidayRepository.Object;
        }
    }
}