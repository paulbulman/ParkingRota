namespace ParkingRota.UnitTests.Business.Emails
{
    using System;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Emails;
    using ParkingRota.Business.Model;
    using Xunit;

    public static class SingleAllocationTests
    {
        [Theory]
        [InlineData("a@b.c")]
        [InlineData("x@y.z")]
        public static void TestTo(string to)
        {
            var email = new SingleAllocation(new Allocation { ApplicationUser = new ApplicationUser { Email = to } });

            Assert.Equal(to, email.To);
        }

        [Fact]
        public static void TestSubject()
        {
            var email = new SingleAllocation(new Allocation { Date = 18.December(2018) });

            Assert.Equal("Space available on 18 Dec", email.Subject);
        }

        [Fact]
        public static void TestBody()
        {
            var email = new SingleAllocation(new Allocation { Date = 18.December(2018) });

            const string ExpectedBody = "A space has been allocated to you for 18 Dec.";

            Assert.True(email.HtmlBody.Contains(ExpectedBody, StringComparison.OrdinalIgnoreCase));
            Assert.True(email.PlainTextBody.Contains(ExpectedBody, StringComparison.OrdinalIgnoreCase));
        }
    }
}