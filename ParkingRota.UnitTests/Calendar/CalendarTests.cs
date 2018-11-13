namespace ParkingRota.UnitTests.Calendar
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Calendar;
    using Xunit;

    public static class CalendarTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public static void Test_Create_SingleActiveDay(bool dataValue)
        {
            var fullWeek = new[] { 7.May(2018), 8.May(2018), 9.May(2018), 10.May(2018), 11.May(2018) };

            foreach (var date in fullWeek)
            {
                var result = Calendar<bool>.Create(new[] { date }.ToDictionary(d => d, d => dataValue));

                Assert.Equal(1, result.Weeks.Count);

                var days = result.Weeks.Single().Days;

                Assert.Equal(fullWeek, days.Select(d => d.Date));

                Assert.All(days, d => Assert.Equal(d.Date == date, d.IsActive));
                Assert.All(days.Where(d => d.IsActive), d => Assert.Equal(dataValue, d.Data));
            }
        }

        [Fact]
        public static void Test_Create_MultipleWeeks()
        {
            var partialPreviousWeek = new[] { 4.May(2018) };
            var fullWeek = new[] { 7.May(2018), 8.May(2018), 9.May(2018), 10.May(2018), 11.May(2018) };
            var partialFutureWeek = new[] { 14.May(2018), 17.May(2018) };

            var allWeeks = new[] { partialPreviousWeek, fullWeek, partialFutureWeek };

            var misorderedActiveDates = allWeeks
                .OrderByDescending(w => w.Length)
                .SelectMany(w => w)
                .ToDictionary(d => d, d => d.ForRoundTrip());

            var result = Calendar<string>.Create(misorderedActiveDates);

            Assert.Equal(allWeeks.Length, result.Weeks.Count);

            var fullWeekDates = new[] { 7.May(2018), 8.May(2018), 9.May(2018), 10.May(2018), 11.May(2018) };
            var partialPreviousWeekDates = new[] { 30.April(2018), 1.May(2018), 2.May(2018), 3.May(2018), 4.May(2018) };
            var partialFutureWeekDates = new[] { 14.May(2018), 15.May(2018), 16.May(2018), 17.May(2018), 18.May(2018) };

            Check_Week(partialPreviousWeekDates, partialPreviousWeek, result.Weeks[0]);
            Check_Week(fullWeekDates, fullWeek, result.Weeks[1]);
            Check_Week(partialFutureWeekDates, partialFutureWeek, result.Weeks[2]);
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
        private static void Check_Week(
            IReadOnlyList<LocalDate> expectedDates,
            IReadOnlyList<LocalDate> expectedActiveDates,
            Week<string> actual)
        {
            Assert.Equal(expectedDates, actual.Days.Select(d => d.Date));
            Assert.Equal(expectedActiveDates, actual.Days.Where(d => d.IsActive).Select(d => d.Date));

            Assert.All(actual.Days.Where(d => d.IsActive), d => Assert.Equal(d.Date.ForRoundTrip(), d.Data));
        }
    }
}