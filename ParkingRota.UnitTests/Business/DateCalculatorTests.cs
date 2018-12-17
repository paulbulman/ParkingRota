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
        public static void Test_GetActiveDates_NextMonthEndsOnWeekend(int currentDay, int expectedFirstDay, int expectedTotalDays)
        {
            var currentLocalDateTime = new LocalDateTime(2018, 2, currentDay, 0, 0);

            var expectedFirstLocalDate = expectedFirstDay.February(2018);
            var expectedLastLocalDate = 30.March(2018);

            Check_GetActiveDates(currentLocalDateTime, expectedTotalDays, expectedFirstLocalDate, expectedLastLocalDate);
        }

        [Theory]
        [InlineData(1, 1, 43)]
        [InlineData(4, 5, 41)]
        public static void Test_GetActiveDates_NextMonthEndsOnWeeday(int currentDay, int expectedFirstDay, int expectedTotalDays)
        {
            var currentLocalDateTime = new LocalDateTime(2018, 3, currentDay, 0, 0);

            var expectedFirstLocalDate = expectedFirstDay.March(2018);
            var expectedLastLocalDate = 30.April(2018);

            Check_GetActiveDates(currentLocalDateTime, expectedTotalDays, expectedFirstLocalDate, expectedLastLocalDate);
        }

        [Fact]
        public static void Test_GetActiveDates_BankHoliday()
        {
            var bankHolidayLocalDates = new[] { 30.March(2018), 2.April(2018) };

            var currentLocalDateTime = new LocalDateTime(2018, 3, 17, 0, 0);

            const int ExpectedTotalDays = 29;
            var expectedFirstLocalDate = 19.March(2018);
            var expectedLastLocalDate = 30.April(2018);

            var result = Check_GetActiveDates(
                currentLocalDateTime,
                ExpectedTotalDays,
                expectedFirstLocalDate,
                expectedLastLocalDate,
                bankHolidayLocalDates);

            foreach (var bankHolidayLocalDate in bankHolidayLocalDates)
            {
                Assert.DoesNotContain(bankHolidayLocalDate, result);
            }
        }

        [Theory]
        [InlineData(7, 10, 8, 16, 7)]
        [InlineData(7, 11, 9, 16, 6)]
        [InlineData(7, 23, 9, 16, 6)]
        [InlineData(8, 0, 9, 23, 11)]
        [InlineData(8, 10, 9, 23, 11)]
        [InlineData(8, 11, 12, 23, 10)]
        public static void Test_GetLongLeadTimeAllocationDates(
            int currentDay,
            int currentHour,
            int expectedFirstDay,
            int expectedLastDay,
            int expectedTotalDays)
        {
            var currentLocalDateTime = new LocalDateTime(2018, 2, currentDay, currentHour, 0);

            var result = new DateCalculator(CreateMockClock(currentLocalDateTime), CreateMockBankHolidayRepository())
                .GetLongLeadTimeAllocationDates();

            Assert.Equal(expectedTotalDays, result.Count);
            Assert.Equal(expectedFirstDay.February(2018), result.First());
            Assert.Equal(expectedLastDay.February(2018), result.Last());
        }

        [Theory]
        [InlineData(8, 10, 8, 8, 1)]
        [InlineData(8, 11, 8, 9, 2)]
        [InlineData(9, 10, 9, 9, 1)]
        [InlineData(9, 11, 9, 12, 2)]
        [InlineData(10, 10, 12, 12, 1)]
        [InlineData(10, 11, 12, 12, 1)]
        public static void Test_GetShortLeadTimeAllocationDates(
            int currentDay,
            int currentHour,
            int expectedFirstDay,
            int expectedLastDay,
            int expectedTotalDays)
        {
            var currentLocalDateTime = new LocalDateTime(2018, 2, currentDay, currentHour, 0);

            var result = new DateCalculator(CreateMockClock(currentLocalDateTime), CreateMockBankHolidayRepository())
                .GetShortLeadTimeAllocationDates();

            Assert.Equal(expectedTotalDays, result.Count);
            Assert.Equal(expectedFirstDay.February(2018), result.First());
            Assert.Equal(expectedLastDay.February(2018), result.Last());
        }

        [Theory]
        [InlineData(14, 15)]
        [InlineData(15, 18)]
        [InlineData(22, 27)]
        public static void Test_GetNextWorkingDate(int currentDay, int expectedNextDay)
        {
            var currentLocalDateTime = new LocalDateTime(2017, 12, currentDay, 11, 0);

            var bankHolidayLocalDates = new[] { 25.December(2017), 26.December(2017) };

            var result = new DateCalculator(
                    CreateMockClock(currentLocalDateTime),
                    CreateMockBankHolidayRepository(bankHolidayLocalDates))
                .GetNextWorkingDate();

            var expectedNextWorkingDate = new LocalDate(2017, 12, expectedNextDay);

            Assert.Equal(expectedNextWorkingDate, result);
        }

        [Theory]
        [InlineData(15, 14)]
        [InlineData(18, 15)]
        [InlineData(27, 22)]
        public static void Test_GetPreviousWorkingDate(int day, int expectedPreviousday)
        {
            var bankHolidayLocalDates = new[] { 25.December(2017), 26.December(2017) };

            var localDate = day.December(2017);

            var result = new DateCalculator(
                    Mock.Of<IClock>(),
                    CreateMockBankHolidayRepository(bankHolidayLocalDates))
                .GetPreviousWorkingDate(localDate);

            var expectedPreviousWorkingDate = new LocalDate(2017, 12, expectedPreviousday);

            Assert.Equal(expectedPreviousWorkingDate, result);
        }

        [Theory]
        [InlineData(6, 12, 16)]
        [InlineData(7, 19, 23)]
        public static void Test_GetUpcomingLongLeadTimeDates(int currentDay, int expectedFirstDay, int expectedLastDay)
        {
            var currentLocalDateTime = new LocalDateTime(2018, 11, currentDay, 0, 0);

            var result = new DateCalculator(
                    CreateMockClock(currentLocalDateTime),
                    CreateMockBankHolidayRepository())
                .GetUpcomingLongLeadTimeAllocationDates();

            Assert.Equal(5, result.Count);

            Assert.Equal(expectedFirstDay, result.First().Day);
            Assert.Equal(expectedLastDay, result.Last().Day);
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
        private static IReadOnlyList<LocalDate> Check_GetActiveDates(
            LocalDateTime currentLocalDateTime,
            int expectedTotalDays,
            LocalDate expectedFirstLocalDate,
            LocalDate expectedLastLocalDate,
            params LocalDate[] bankHolidayLocalDates)
        {
            var result = new DateCalculator(
                CreateMockClock(currentLocalDateTime),
                CreateMockBankHolidayRepository(bankHolidayLocalDates)).GetActiveDates();

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

        private static IBankHolidayRepository CreateMockBankHolidayRepository(params LocalDate[] bankHolidayDates)
        {
            var mockBankHolidayRepository = new Mock<IBankHolidayRepository>(MockBehavior.Strict);

            mockBankHolidayRepository
                .SetupGet(r => r.BankHolidays)
                .Returns(bankHolidayDates.Select(b => new BankHoliday { Date = b }).ToArray());

            return mockBankHolidayRepository.Object;
        }
    }
}