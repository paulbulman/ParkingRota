namespace ParkingRota.UnitTests.Business.Emails
{
    using System;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Emails;
    using Xunit;

    public static class ReservationReminderTests
    {
        [Theory]
        [InlineData("a@b.c")]
        [InlineData("x@y.z")]
        public static void TestTo(string to)
        {
            var email = new ReservationReminder(to, default(LocalDate));

            Assert.Equal(to, email.To);
        }

        [Fact]
        public static void TestSubject()
        {
            var email = new ReservationReminder(default(string), 11.December(2018));

            Assert.Equal("No reservations entered for 11 Dec", email.Subject);
        }

        [Fact]
        public static void TestBody()
        {
            var email = new ReservationReminder(default(string), 1.January(2019));

            const string ExpectedText = "No reservations have yet been entered for 01 Jan";

            Assert.True(email.HtmlBody.Contains(ExpectedText, StringComparison.InvariantCulture));
            Assert.True(email.PlainTextBody.Contains(ExpectedText, StringComparison.InvariantCulture));
        }
    }
}