namespace ParkingRota.UnitTests.Business
{
    using Moq;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using Xunit;

    public class RegistrationTokenValidatorTests
    {
        [Theory]
        [InlineData("A", true)]
        [InlineData("a", true)]
        [InlineData("B", true)]
        [InlineData("C", false)]
        [InlineData("", false)]
        public void Test_TokenIsValid_StringExists(string token, bool expectedIsValid)
        {
            var currentInstant = 15.September(2018).At(17, 37, 01).Utc();

            var registrationTokens = new[]
            {
                new RegistrationToken { Token = "A", ExpiryTime = currentInstant.Plus(1.Seconds()) },
                new RegistrationToken { Token = "B", ExpiryTime = currentInstant.Plus(1.Seconds()) }
            };

            var mockRegistrationTokenRepository = new Mock<IRegistrationTokenRepository>(MockBehavior.Strict);

            mockRegistrationTokenRepository
                .SetupGet(r => r.RegistrationTokens)
                .Returns(registrationTokens);

            var validator = new RegistrationTokenValidator(
                new FakeClock(currentInstant), mockRegistrationTokenRepository.Object);

            Assert.Equal(expectedIsValid, validator.TokenIsValid(token));
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(-1, false)]
        public void Test_TokenIsValid_NotExpired(int expiryOffsetSeconds, bool expectedIsValid)
        {
            const string Token = "A";

            var currentInstant = 15.September(2018).At(17, 37, 01).Utc();

            var registrationTokens = new[]
            {
                new RegistrationToken { Token = Token, ExpiryTime = currentInstant.Plus(expiryOffsetSeconds.Seconds()) }
            };

            var mockRegistrationTokenRepository = new Mock<IRegistrationTokenRepository>(MockBehavior.Strict);

            mockRegistrationTokenRepository
                .SetupGet(r => r.RegistrationTokens)
                .Returns(registrationTokens);

            var validator = new RegistrationTokenValidator(
                new FakeClock(currentInstant), mockRegistrationTokenRepository.Object);

            Assert.Equal(expectedIsValid, validator.TokenIsValid(Token));
        }
    }
}