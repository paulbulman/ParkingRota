namespace ParkingRota.UnitTests.Business.Emails
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Emails;
    using ParkingRota.Business.Model;
    using Xunit;

    public static class WeeklySummaryTests
    {
        [Theory]
        [InlineData("a@b.c")]
        [InlineData("x@y.z")]
        public static void TestTo(string to)
        {
            var email = new WeeklySummary(to, default(IReadOnlyList<Allocation>), default(IReadOnlyList<Request>));

            Assert.Equal(to, email.To);
        }

        [Fact]
        public static void TestSubject()
        {
            var allocations = new[]
            {
                new Allocation { Date = 21.December(2018) },
                new Allocation { Date = 17.December(2018) }
            };

            const string ExpectedSubject = "Weekly provisional allocations summary for 17 Dec - 21 Dec";

            var email = new WeeklySummary(default(string), allocations, default(IReadOnlyList<Request>));

            Assert.Equal(ExpectedSubject, email.Subject);
        }

        [Fact]
        public static void TestBody()
        {
            var allocatedUser = new ApplicationUser { FirstName = "Petr", LastName = "Čech" };
            var otherAllocatedUser = new ApplicationUser { FirstName = "Héctor", LastName = "Bellerín" };

            var otherDateAllocatedUser = new ApplicationUser { FirstName = "Henrikh", LastName = "Mkhitaryan" };
            var otherDateOtherAllocatedUser = new ApplicationUser { FirstName = "Laurent", LastName = "Koscielny" };

            var interruptedUser = new ApplicationUser { FirstName = "Sokratis", LastName = "Papastathopoulos" };
            var otherInterruptedUser = new ApplicationUser { FirstName = "Mohamed", LastName = "Elneny" };

            var otherDateInterruptedUser = new ApplicationUser { FirstName = "Aaron", LastName = "Ramsey" };
            var otherDateOtherInterruptedUser = new ApplicationUser { FirstName = "Alexandre", LastName = "Lacazette" };

            var allocations = new[]
            {
                new Allocation { ApplicationUser = otherDateAllocatedUser, Date = 21.December(2018) },
                new Allocation { ApplicationUser = allocatedUser, Date = 17.December(2018) },
                new Allocation { ApplicationUser = otherDateOtherAllocatedUser, Date = 21.December(2018) },
                new Allocation { ApplicationUser = otherAllocatedUser, Date = 17.December(2018) }
            };

            var requests = new[]
            {
                new Request { ApplicationUser = otherDateAllocatedUser, Date = 21.December(2018) },
                new Request { ApplicationUser = allocatedUser, Date = 17.December(2018) },
                new Request { ApplicationUser = otherDateOtherAllocatedUser, Date = 21.December(2018) },
                new Request { ApplicationUser = otherAllocatedUser, Date = 17.December(2018) },

                new Request { ApplicationUser = otherDateInterruptedUser, Date = 21.December(2018) },
                new Request { ApplicationUser = interruptedUser, Date = 17.December(2018) },
                new Request { ApplicationUser = otherDateOtherInterruptedUser, Date = 21.December(2018) },
                new Request { ApplicationUser = otherInterruptedUser, Date = 17.December(2018) }
            };

            var email = new WeeklySummary(default(string), allocations, requests);

            var interruptedText = $"(Interrupted: {otherInterruptedUser.FullName}, {interruptedUser.FullName})";
            var otherDateInterruptedText = $"(Interrupted: {otherDateOtherInterruptedUser.FullName}, {otherDateInterruptedUser.FullName})";

            var expectedValues = new[]
            {
                "17 Dec:", otherAllocatedUser.FullName, allocatedUser.FullName, interruptedText,
                "21 Dec:", otherDateOtherAllocatedUser.FullName, otherDateAllocatedUser.FullName, otherDateInterruptedText
            };

            Check_TextAppearsInOrder(expectedValues, email.HtmlBody);
            Check_TextAppearsInOrder(expectedValues, email.PlainTextBody);
        }

        private static void Check_TextAppearsInOrder(IReadOnlyList<string> expectedValues, string result)
        {
            var expectedValuePositions = expectedValues
                .Select(v => result.IndexOf(v, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            Assert.All(expectedValuePositions, p => Assert.NotEqual(-1, p));

            Assert.Equal(expectedValuePositions, expectedValuePositions.OrderBy(i => i));
        }
    }
}