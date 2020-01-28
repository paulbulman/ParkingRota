﻿namespace ParkingRota.UnitTests.Business.EmailTemplates
{
    using System;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.EmailTemplates;
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
            var email = new ReservationReminder(default(string), 15.December(2018));

            Assert.Equal("No reservations entered for Sat 15 Dec", email.Subject);
        }

        [Fact]
        public static void TestBody()
        {
            var email = new ReservationReminder(default(string), 3.January(2019));

            const string ExpectedText = "No reservations have yet been entered for Thu 03 Jan";

            Assert.True(email.HtmlBody.Contains(ExpectedText, StringComparison.InvariantCulture));
            Assert.True(email.PlainTextBody.Contains(ExpectedText, StringComparison.InvariantCulture));
        }
    }
}