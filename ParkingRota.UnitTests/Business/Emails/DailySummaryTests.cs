namespace ParkingRota.UnitTests.Business.Emails
{
    using System;
    using System.Collections.Generic;
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

            const string ExpectedSubject = "Daily allocations summary for 18 Dec";

            var email = new DailySummary(default(string), allocations, default(IReadOnlyList<Request>));

            Assert.Equal(ExpectedSubject, email.Subject);
        }

        [Fact]
        public static void TestBody()
        {
            var allocatedUser = new ApplicationUser { FirstName = "Petr", LastName = "Čech" };
            var otherAllocatedUser = new ApplicationUser { FirstName = "Héctor", LastName = "Bellerín" };

            var interruptedUser = new ApplicationUser { FirstName = "Sokratis", LastName = "Papastathopoulos" };
            var otherInterruptedUser = new ApplicationUser { FirstName = "Mohamed", LastName = "Elneny" };

            var allocations = new[]
            {
                new Allocation { ApplicationUser = allocatedUser, Date = 18.December(2018) },
                new Allocation { ApplicationUser = otherAllocatedUser, Date = 18.December(2018) },
            };

            var requests = new[]
            {
                new Request { ApplicationUser = allocatedUser, Date = 18.December(2018) },
                new Request { ApplicationUser = otherAllocatedUser, Date = 18.December(2018) },
                new Request { ApplicationUser = interruptedUser, Date = 18.December(2018) },
                new Request { ApplicationUser = otherInterruptedUser, Date = 18.December(2018) },
            };

            var email = new DailySummary(default(string), allocations, requests);

            Assert.True(email.HtmlBody.Contains(allocatedUser.FullName, StringComparison.InvariantCultureIgnoreCase));
            Assert.True(email.HtmlBody.Contains(otherAllocatedUser.FullName, StringComparison.InvariantCultureIgnoreCase));
            Assert.True(email.PlainTextBody.Contains(allocatedUser.FullName, StringComparison.InvariantCultureIgnoreCase));
            Assert.True(email.PlainTextBody.Contains(otherAllocatedUser.FullName, StringComparison.InvariantCultureIgnoreCase));

            var interruptedText = $"(Interrupted: {otherInterruptedUser.FullName}, {interruptedUser.FullName})";

            Assert.True(email.HtmlBody.Contains(interruptedText, StringComparison.InvariantCultureIgnoreCase));
            Assert.True(email.PlainTextBody.Contains(interruptedText, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}