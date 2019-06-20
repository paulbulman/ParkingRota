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
            var recipient = new ApplicationUser { Email = to };

            var email = new DailySummary(
                recipient, default(IReadOnlyList<Allocation>), default(IReadOnlyList<Request>));

            Assert.Equal(to, email.To);
        }

        [Fact]
        public static void TestSubject_RecipientAllocated()
        {
            var date = 18.December(2018);

            var recipient = Create.User("Pierre-Emerick Aubameyang");

            var allocatedUsers = Create.Users("Petr Čech", "Héctor Bellerín").Concat(new[] { recipient });
            var interruptedUsers = Create.Users("Sokratis Papastathopoulos", "Mohamed Elneny");

            var allUsers = allocatedUsers.Concat(interruptedUsers);

            var allocations = Create.Allocations(allocatedUsers, date);
            var requests = Create.Requests(allUsers, date);

            var email = new DailySummary(recipient, allocations, requests);

            Assert.Equal("[Allocated] 18 Dec Daily allocations summary", email.Subject);
        }

        [Fact]
        public static void TestSubject_RecipientInterrupted()
        {
            var date = 18.December(2018);

            var recipient = Create.User("Pierre-Emerick Aubameyang");

            var allocatedUsers = Create.Users("Petr Čech", "Héctor Bellerín");
            var interruptedUsers = Create.Users("Sokratis Papastathopoulos", "Mohamed Elneny").Concat(new[] { recipient });

            var allUsers = allocatedUsers.Concat(interruptedUsers);

            var allocations = Create.Allocations(allocatedUsers, date);
            var requests = Create.Requests(allUsers, date);

            var email = new DailySummary(recipient, allocations, requests);

            Assert.Equal("[INTERRUPTED] 18 Dec Daily allocations summary", email.Subject);
        }

        [Fact]
        public static void TestBody_RecipientAllocated()
        {
            var recipient = Create.User("Pierre-Emerick Aubameyang");

            var allocatedUsers = Create.Users("Petr Čech", "Héctor Bellerín").Concat(new[] { recipient });
            var interruptedUsers = Create.Users("Sokratis Papastathopoulos", "Mohamed Elneny");

            var allUsers = allocatedUsers.Concat(interruptedUsers);

            var allocations = Create.Allocations(allocatedUsers, default(LocalDate));
            var requests = Create.Requests(allUsers, default(LocalDate));

            var email = new DailySummary(recipient, allocations, requests);

            Assert.All(allocatedUsers, a => email.HtmlBody.Contains(a.FullName, StringComparison.InvariantCulture));
            Assert.All(allocatedUsers, a => email.PlainTextBody.Contains(a.FullName, StringComparison.InvariantCulture));

            Assert.True(email.HtmlBody.Contains($"<strong>{recipient.FullName}</strong>", StringComparison.InvariantCulture));
            Assert.True(email.PlainTextBody.Contains($"*{recipient.FullName}*", StringComparison.InvariantCulture));

            const string ExpectedInterruptedText = "(Interrupted: Mohamed Elneny, Sokratis Papastathopoulos)";

            Assert.True(email.HtmlBody.Contains(ExpectedInterruptedText, StringComparison.InvariantCulture));
            Assert.True(email.PlainTextBody.Contains(ExpectedInterruptedText, StringComparison.InvariantCulture));
        }

        [Fact]
        public static void TestBody_RecipientInterrupted()
        {
            var recipient = Create.User("Pierre-Emerick Aubameyang");

            var allocatedUsers = Create.Users("Petr Čech", "Héctor Bellerín");
            var interruptedUsers = Create.Users("Sokratis Papastathopoulos", "Mohamed Elneny").Concat(new[] { recipient });

            var allUsers = allocatedUsers.Concat(interruptedUsers);

            var allocations = Create.Allocations(allocatedUsers, default(LocalDate));
            var requests = Create.Requests(allUsers, default(LocalDate));

            var email = new DailySummary(recipient, allocations, requests);

            Assert.All(allocatedUsers, a => email.HtmlBody.Contains(a.FullName, StringComparison.InvariantCulture));
            Assert.All(allocatedUsers, a => email.PlainTextBody.Contains(a.FullName, StringComparison.InvariantCulture));

            const string ExpectedHtmlInterruptedText =
                "(Interrupted: <strong>Pierre-Emerick Aubameyang</strong>, Mohamed Elneny, Sokratis Papastathopoulos)";
            const string ExpectedPlainInterruptedText =
                "(Interrupted: *Pierre-Emerick Aubameyang*, Mohamed Elneny, Sokratis Papastathopoulos)";

            Assert.True(email.HtmlBody.Contains(ExpectedHtmlInterruptedText, StringComparison.InvariantCulture));
            Assert.True(email.PlainTextBody.Contains(ExpectedPlainInterruptedText, StringComparison.InvariantCulture));
        }
    }
}