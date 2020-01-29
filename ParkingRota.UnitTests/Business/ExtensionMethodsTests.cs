namespace ParkingRota.UnitTests.Business
{
    using NodaTime;
    using ParkingRota.Business;
    using Xunit;

    public static class ExtensionMethodsTests
    {
        [Theory]
        [InlineData(2018, 11, 7, "07 Nov")]
        [InlineData(2019, 3, 2, "02 Mar")]
        public static void Test_LocalDate_ForDisplay(int year, int month, int day, string expectedText)
        {
            var localDate = new LocalDate(year, month, day);

            Assert.Equal(expectedText, localDate.ForDisplay());
        }

        [Theory]
        [InlineData(2018, 11, 7, "Wed 07 Nov")]
        [InlineData(2019, 3, 2, "Sat 02 Mar")]
        public static void Test_LocalDate_ForDisplayWithDayOfWeek(int year, int month, int day, string expectedText)
        {
            var localDate = new LocalDate(year, month, day);

            Assert.Equal(expectedText, localDate.ForDisplayWithDayOfWeek());
        }

        [Theory]
        [InlineData(2018, 11, 7, "2018-11-07")]
        [InlineData(2019, 3, 2, "2019-03-02")]
        public static void Test_LocalDate_ForRoundTrip(int year, int month, int day, string expectedText)
        {
            var localDate = new LocalDate(year, month, day);

            Assert.Equal(expectedText, localDate.ForRoundTrip());
        }

        [Theory]
        [InlineData(2018, 11, 7, 15, 22, 1, "15:22:01 on 07 Nov")]
        [InlineData(2019, 10, 27, 0, 2, 3, "01:02:03 on 27 Oct")]
        [InlineData(2019, 10, 27, 1, 2, 3, "01:02:03 on 27 Oct")]
        public static void Test_ZonedDateTime_ForDisplay(int year, int month, int day, int hour, int minute, int second, string expectedText)
        {
            var zonedDateTime = new ZonedDateTime(Instant.FromUtc(year, month, day, hour, minute, second), DateCalculator.LondonTimeZone);

            Assert.Equal(expectedText, zonedDateTime.ForDisplay());
        }
    }
}