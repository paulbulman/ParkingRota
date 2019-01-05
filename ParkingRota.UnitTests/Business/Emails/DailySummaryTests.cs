namespace ParkingRota.UnitTests.Business.Emails
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Emails;
    using ParkingRota.Business.Model;
    using Xunit;

    public static class DailySummaryTests
    {
        [Theory]
        [InlineData("a@b.c")]
        [InlineData("x@y.z")]
        public static void TestTo(string to)
        {
            var email = new DailySummary(to, default(IReadOnlyList<Allocation>), default(IReadOnlyList<Request>));

            Assert.Equal(to, email.To);
        }

        [Fact]
        public static void TestSubject()
        {
            var allocations = new[] { new Allocation { Date = 18.December(2018) } };

            var email = new DailySummary(default(string), allocations, default(IReadOnlyList<Request>));

            Assert.Equal("Daily allocations summary for 18 Dec", email.Subject);
        }

        [Fact]
        public static void TestBody()
        {
            var allocatedUsers = Create.Users("Petr Čech", "Héctor Bellerín");
            var interruptedUsers = Create.Users("Sokratis Papastathopoulos", "Mohamed Elneny");

            var allUsers = allocatedUsers.Concat(interruptedUsers);

            var allocations = Create.Allocations(allocatedUsers, default(LocalDate));
            var requests = Create.Requests(allUsers, default(LocalDate));

            var email = new DailySummary(default(string), allocations, requests.ToArray());

            Assert.All(allocatedUsers, a => email.HtmlBody.Contains(a.FullName, StringComparison.InvariantCulture));
            Assert.All(allocatedUsers, a => email.PlainTextBody.Contains(a.FullName, StringComparison.InvariantCulture));

            const string ExpectedInterruptedText = "(Interrupted: Mohamed Elneny, Sokratis Papastathopoulos)";

            Assert.True(email.HtmlBody.Contains(ExpectedInterruptedText, StringComparison.InvariantCulture));
            Assert.True(email.PlainTextBody.Contains(ExpectedInterruptedText, StringComparison.InvariantCulture));
        }
    }
}