namespace ParkingRota.UnitTests
{
    using NodaTime;

    public static class TestHelpers
    {
        public static LocalDateTime At(this LocalDate localDate, int hour, int minute, int second) =>
            localDate.At(new LocalTime(hour, minute, second));

        public static Instant Utc(this LocalDateTime localDateTime) => localDateTime.InUtc().ToInstant();
    }
}