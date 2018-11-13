namespace ParkingRota.UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using ParkingRota.Calendar;

    public static class TestHelpers
    {
        public static LocalDateTime At(this LocalDate localDate, int hour, int minute, int second) =>
            localDate.At(new LocalTime(hour, minute, second));

        public static Instant Utc(this LocalDateTime localDateTime) => localDateTime.InUtc().ToInstant();

        public static IReadOnlyList<LocalDate> ActiveDates<T>(this Calendar<T> calendar) =>
            calendar.Weeks
                .SelectMany(w => w.Days)
                .Where(d => d.IsActive)
                .Select(d => d.Date)
                .ToArray();

        public static T Data<T>(this Calendar<T> calendar, LocalDate date) =>
            calendar.Weeks
                .SelectMany(w => w.Days)
                .Single(d => d.Date == date)
                .Data;
    }
}