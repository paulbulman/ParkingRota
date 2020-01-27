namespace ParkingRota.Business
{
    using NodaTime;
    using NodaTime.Text;

    public static class ExtensionMethods
    {
        public static string ForDisplay(this LocalDate localDate) =>
            LocalDatePattern.CreateWithCurrentCulture("dd MMM").Format(localDate);

        public static string ForDisplayWithDayOfWeek(this LocalDate localDate) =>
            LocalDatePattern.CreateWithCurrentCulture("ddd dd MMM").Format(localDate);

        public static string ForRoundTrip(this LocalDate localDate) =>
            LocalDatePattern.Iso.Format(localDate);

        public static string ForDisplay(this ZonedDateTime zonedDateTime) =>
            $"{ZonedDateTimePattern.CreateWithCurrentCulture("HH:mm:ss", DateTimeZoneProviders.Tzdb).Format(zonedDateTime)} on " +
            $"{ZonedDateTimePattern.CreateWithCurrentCulture("dd MMM", DateTimeZoneProviders.Tzdb).Format(zonedDateTime)}";
    }
}