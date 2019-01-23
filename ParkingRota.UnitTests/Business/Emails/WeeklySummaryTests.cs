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
            var recipient = new ApplicationUser { Email = to };

            var email = new WeeklySummary(recipient, default(IReadOnlyList<Allocation>), default(IReadOnlyList<Request>));

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

            var email = new WeeklySummary(new ApplicationUser(), allocations, default(IReadOnlyList<Request>));

            Assert.Equal("Weekly provisional allocations summary for 17 Dec - 21 Dec", email.Subject);
        }

        [Fact]
        public static void TestBody()
        {
            var recipient = Create.User("Pierre-Emerick Aubameyang");

            var firstDateAllocatedUsers = Create.Users("Petr Čech", "Héctor Bellerín").Concat(new[] { recipient });
            var firstDateInterruptedUsers = Create.Users("Sokratis Papastathopoulos", "Mohamed Elneny");

            var secondDateAllocatedUsers = Create.Users("Henrikh Mkhitaryan", "Laurent Koscielny");
            var secondDateInterruptedUsers = Create.Users("Aaron Ramsey", "Alexandre Lacazette").Concat(new[] { recipient });

            var allFirstDateUsers = firstDateAllocatedUsers.Concat(firstDateInterruptedUsers);
            var allSecondDateUsers = secondDateAllocatedUsers.Concat(secondDateInterruptedUsers);

            var allocations = Create.Allocations(secondDateAllocatedUsers, 21.December(2018))
                .Concat(Create.Allocations(firstDateAllocatedUsers, 17.December(2018)));

            var requests = Create.Requests(allSecondDateUsers, 21.December(2018))
                .Concat(Create.Requests(allFirstDateUsers, 17.December(2018)));

            var email = new WeeklySummary(recipient, allocations.ToArray(), requests.ToArray());

            var expectedHtmlValues = new[]
            {
                "17 Dec:", "<strong>Pierre-Emerick Aubameyang</strong>", "Héctor Bellerín", "Petr Čech", "(Interrupted: Mohamed Elneny, Sokratis Papastathopoulos)",
                "21 Dec:", "Laurent Koscielny", "Henrikh Mkhitaryan", "(Interrupted: <strong>Pierre-Emerick Aubameyang</strong>, Alexandre Lacazette, Aaron Ramsey)"
            };

            var expectedPlainTextValues = new[]
            {
                "17 Dec:", "*Pierre-Emerick Aubameyang*", "Héctor Bellerín", "Petr Čech", "(Interrupted: Mohamed Elneny, Sokratis Papastathopoulos)",
                "21 Dec:", "Laurent Koscielny", "Henrikh Mkhitaryan", "(Interrupted: *Pierre-Emerick Aubameyang*, Alexandre Lacazette, Aaron Ramsey)"
            };

            Check_TextAppearsInOrder(expectedHtmlValues, email.HtmlBody);
            Check_TextAppearsInOrder(expectedPlainTextValues, email.PlainTextBody);
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