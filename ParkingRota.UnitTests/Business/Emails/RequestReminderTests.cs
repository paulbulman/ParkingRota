namespace ParkingRota.UnitTests.Business.Emails
{
    using System;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Emails;
    using Xunit;

    public static class RequestReminderTests
    {
        [Theory]
        [InlineData("a@b.c")]
        [InlineData("x@y.z")]
        public static void TestTo(string to)
        {
            var email = new RequestReminder(to, default(LocalDate), default(LocalDate));

            Assert.Equal(to, email.To);
        }

        [Fact]
        public static void TestSubject()
        {
            var email = new RequestReminder(default(string), 17.December(2018), 21.December(2018));

            Assert.Equal("No requests entered for 17 Dec - 21 Dec", email.Subject);
        }

        [Fact]
        public static void TestBody()
        {
            var email = new RequestReminder(default(string), 17.December(2018), 21.December(2018));

            const string ExpectedText = "No requests have yet been entered for 17 Dec - 21 Dec";

            Assert.True(email.HtmlBody.Contains(ExpectedText, StringComparison.InvariantCulture));
            Assert.True(email.PlainTextBody.Contains(ExpectedText, StringComparison.InvariantCulture));
        }
    }
}