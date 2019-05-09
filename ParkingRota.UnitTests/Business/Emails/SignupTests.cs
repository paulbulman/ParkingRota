namespace ParkingRota.UnitTests.Business.Emails
{
    using System;
    using ParkingRota.Business.Emails;
    using Xunit;

    public static class SignupTests
    {
        [Theory]
        [InlineData("a@b.c")]
        [InlineData("x@y.z")]
        public static void TestTo(string to)
        {
            var email = new Signup(to, default(string), default(string));

            Assert.Equal(to, email.To);
        }

        [Fact]
        public static void TestSubject()
        {
            var email = new Signup(default(string), default(string), default(string));

            Assert.Equal("Parking Rota registration", email.Subject);
        }

        [Theory]
        [InlineData("https://localhost/Identity/Account/Register?registrationToken=abc", "1.2.3.4")]
        [InlineData("https://localhost/Identity/Account/Register?registrationToken=xyz", "192.168.0.1")]
        public static void TestBody(string callbackUrl, string originatingIpAddress)
        {
            var email = new Signup(default(string), callbackUrl, originatingIpAddress);

            Assert.True(email.HtmlBody.Contains(callbackUrl.Replace("&", "&amp;"), StringComparison.Ordinal));
            Assert.True(email.HtmlBody.Contains(originatingIpAddress, StringComparison.OrdinalIgnoreCase));

            Assert.True(email.PlainTextBody.Contains(callbackUrl, StringComparison.Ordinal));
            Assert.True(email.PlainTextBody.Contains(originatingIpAddress, StringComparison.OrdinalIgnoreCase));
        }
    }
}