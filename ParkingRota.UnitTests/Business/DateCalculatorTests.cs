namespace ParkingRota.UnitTests.Business
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Data;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using Xunit;

    public class DateCalculatorTests : DatabaseTests
    {
        [Theory]
        [InlineData(1, 1, 42)]
        [InlineData(4, 5, 40)]
        public void Test_GetActiveDates_NextMonthEndsOnWeekend(int currentDay, int expectedFirstDay, int expectedTotalDays)
        {
            this.SetClock(currentDay.February(2018).AtMidnight().Utc());

            var expectedFirstLocalDate = expectedFirstDay.February(2018);
            var expectedLastLocalDate = 30.March(2018);

            this.Check_GetActiveDates(expectedTotalDays, expectedFirstLocalDate, expectedLastLocalDate);
        }

        [Theory]
        [InlineData(1, 1, 43)]
        [InlineData(4, 5, 41)]
        public void Test_GetActiveDates_NextMonthEndsOnWeekday(int currentDay, int expectedFirstDay, int expectedTotalDays)
        {
            this.SetClock(currentDay.March(2018).AtMidnight().Utc());

            var expectedFirstLocalDate = expectedFirstDay.March(2018);
            var expectedLastLocalDate = 30.April(2018);

            this.Check_GetActiveDates(expectedTotalDays, expectedFirstLocalDate, expectedLastLocalDate);
        }

        [Fact]
        public void Test_GetActiveDates_BankHoliday()
        {
            this.SetClock(17.March(2018).AtMidnight().Utc());

            var bankHolidays = new[]
            {
                this.Seed.BankHoliday(30.March(2018)),
                this.Seed.BankHoliday(2.April(2018))
            };

            const int ExpectedTotalDays = 29;
            var expectedFirstLocalDate = 19.March(2018);
            var expectedLastLocalDate = 30.April(2018);

            var result = this.Check_GetActiveDates(ExpectedTotalDays, expectedFirstLocalDate, expectedLastLocalDate);

            foreach (var bankHoliday in bankHolidays)
            {
                Assert.DoesNotContain(bankHoliday.Date, result);
            }
        }

        [Theory]
        [InlineData(7, 10, 8, 16, 7)]
        [InlineData(7, 11, 9, 16, 6)]
        [InlineData(7, 23, 9, 16, 6)]
        [InlineData(8, 0, 9, 23, 11)]
        [InlineData(8, 10, 9, 23, 11)]
        [InlineData(8, 11, 12, 23, 10)]
        public void Test_GetLongLeadTimeAllocationDates(
            int currentDay,
            int currentHour,
            int expectedFirstDay,
            int expectedLastDay,
            int expectedTotalDays)
        {
            this.SetClock(currentDay.February(2018).At(currentHour, 0, 0).Utc());

            using (var scope = this.CreateScope())
            {
                var result = scope.ServiceProvider
                    .GetRequiredService<IDateCalculator>()
                    .GetLongLeadTimeAllocationDates();

                Assert.Equal(expectedTotalDays, result.Count);
                Assert.Equal(expectedFirstDay.February(2018), result.First());
                Assert.Equal(expectedLastDay.February(2018), result.Last());
            }
        }

        [Theory]
        [InlineData(8, 10, 8, 8, 1)]
        [InlineData(8, 11, 8, 9, 2)]
        [InlineData(9, 10, 9, 9, 1)]
        [InlineData(9, 11, 9, 12, 2)]
        [InlineData(10, 10, 12, 12, 1)]
        [InlineData(10, 11, 12, 12, 1)]
        public void Test_GetShortLeadTimeAllocationDates(
            int currentDay,
            int currentHour,
            int expectedFirstDay,
            int expectedLastDay,
            int expectedTotalDays)
        {
            this.SetClock(currentDay.February(2018).At(currentHour, 0, 0).Utc());

            using (var scope = this.CreateScope())
            {
                var result = scope.ServiceProvider
                    .GetRequiredService<IDateCalculator>()
                    .GetShortLeadTimeAllocationDates();

                Assert.Equal(expectedTotalDays, result.Count);
                Assert.Equal(expectedFirstDay.February(2018), result.First());
                Assert.Equal(expectedLastDay.February(2018), result.Last());
            }
        }

        [Theory]
        [InlineData(11, 17, 21, 5)]
        [InlineData(12, 17, 21, 5)]
        [InlineData(13, 24, 28, 3)]
        public void Test_GetWeeklySummaryDates(
            int currentDay,
            int expectedFirstDay,
            int expectedLastDay,
            int expectedTotalDays)
        {
            this.SetClock(currentDay.December(2018).AtMidnight().Utc());

            this.Seed.BankHoliday(25.December(2018));
            this.Seed.BankHoliday(26.December(2018));

            using (var scope = this.CreateScope())
            {
                var result = scope.ServiceProvider
                    .GetRequiredService<IDateCalculator>()
                    .GetWeeklySummaryDates();

                Assert.Equal(expectedTotalDays, result.Count);
                Assert.Equal(expectedFirstDay.December(2018), result.First());
                Assert.Equal(expectedLastDay.December(2018), result.Last());
            }
        }

        [Theory]
        [InlineData(14, 15)]
        [InlineData(15, 18)]
        [InlineData(22, 27)]
        public void Test_GetNextWorkingDate(int currentDay, int expectedNextDay)
        {
            this.SetClock(currentDay.December(2017).At(11, 00, 00).Utc());

            this.Seed.BankHoliday(25.December(2017));
            this.Seed.BankHoliday(26.December(2017));

            using (var scope = this.CreateScope())
            {
                var result = scope.ServiceProvider
                    .GetRequiredService<IDateCalculator>()
                    .GetNextWorkingDate();

                var expectedNextWorkingDate = new LocalDate(2017, 12, expectedNextDay);

                Assert.Equal(expectedNextWorkingDate, result);
            }
        }

        [Fact]
        public void Test_GetUpcomingLongLeadTimeDates_NewDates()
        {
            this.SetClock(7.November(2018).AtMidnight().Utc());

            using (var scope = this.CreateScope())
            {
                var result = scope.ServiceProvider
                    .GetRequiredService<IDateCalculator>()
                    .GetUpcomingLongLeadTimeAllocationDates();

                Assert.Equal(5, result.Count);

                Assert.Equal(19.November(2018), result.First());
                Assert.Equal(23.November(2018), result.Last());
            }
        }

        [Fact]
        public void Test_GetUpcomingLongLeadTimeDates_NoNewDates()
        {
            this.SetClock(6.November(2018).AtMidnight().Utc());
            
            using (var scope = this.CreateScope())
            {
                var result = scope.ServiceProvider
                    .GetRequiredService<IDateCalculator>()
                    .GetUpcomingLongLeadTimeAllocationDates();

                Assert.Empty(result);
            }
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
        private IReadOnlyList<LocalDate> Check_GetActiveDates(
            int expectedTotalDays,
            LocalDate expectedFirstLocalDate,
            LocalDate expectedLastLocalDate)
        {
            using (var scope = this.CreateScope())
            {
                var result = scope.ServiceProvider
                    .GetRequiredService<IDateCalculator>()
                    .GetActiveDates();

                Assert.Equal(expectedTotalDays, result.Count);
                Assert.Equal(expectedFirstLocalDate, result.First());
                Assert.Equal(expectedLastLocalDate, result.Last());

                return result;
            }
        }
    }
}