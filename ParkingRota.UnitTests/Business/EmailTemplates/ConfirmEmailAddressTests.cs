namespace ParkingRota.UnitTests.Business.EmailTemplates
{
    using System;
    using ParkingRota.Business.EmailTemplates;
    using Xunit;

    public static class ConfirmEmailAddressTests
    {
        [Theory]
        [InlineData("a@b.c")]
        [InlineData("x@y.z")]
        public static void TestTo(string to)
        {
            var email = new ConfirmEmailAddress(to, default(string), default(string));

            Assert.Equal(to, email.To);
        }

        [Fact]
        public static void TestSubject()
        {
            var email = new ConfirmEmailAddress(default(string), default(string), default(string));

            Assert.Equal("Confirm your email address", email.Subject);
        }

        [Theory]
        [InlineData("https://localhost/Identity/Account/ConfirmEmail?userId=abc&code=def", "1.2.3.4")]
        [InlineData("https://localhost/Identity/Account/ConfirmEmail?userId=xyz&code=123", "192.168.0.1")]
        public static void TestBody(string callbackUrl, string originatingIpAddress)
        {
            var email = new ConfirmEmailAddress(default(string), callbackUrl, originatingIpAddress);

            Assert.True(email.HtmlBody.Contains(callbackUrl.Replace("&", "&amp;"), StringComparison.Ordinal));
            Assert.True(email.HtmlBody.Contains(originatingIpAddress, StringComparison.OrdinalIgnoreCase));

            Assert.True(email.PlainTextBody.Contains(callbackUrl, StringComparison.Ordinal));
            Assert.True(email.PlainTextBody.Contains(originatingIpAddress, StringComparison.OrdinalIgnoreCase));
        }
    }
}