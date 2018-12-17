namespace ParkingRota.UnitTests.Business.Emails
{
    using System;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Emails;
    using Xunit;

    public static class RequestsReminderTests
    {
        [Theory]
        [InlineData("a@b.c")]
        [InlineData("x@y.z")]
        public static void TestTo(string to)
        {
            var email = new RequestsReminder(to, default(LocalDate), default(LocalDate));

            Assert.Equal(to, email.To);
        }

        [Fact]
        public static void TestSubject()
        {
            const string ExpectedSubject = "No requests entered for 17 Dec - 21 Dec";

            var email = new RequestsReminder(default(string), 17.December(2018), 21.December(2018));

            Assert.Equal(ExpectedSubject, email.Subject);
        }

        [Fact]
        public static void TestBody()
        {
            const string ExpectedText = "No requests have yet been entered for 17 Dec - 21 Dec";

            var email = new RequestsReminder(default(string), 17.December(2018), 21.December(2018));

            Assert.True(email.HtmlBody.Contains(ExpectedText, StringComparison.InvariantCultureIgnoreCase));
            Assert.True(email.PlainTextBody.Contains(ExpectedText, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}