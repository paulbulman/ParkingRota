namespace ParkingRota.UnitTests.Business
{
    using Moq;
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
        public void Test_TokenIsValid(string token, bool expectedIsValid)
        {
            var registrationTokens = new[]
            {
                new RegistrationToken { Token = "A" },
                new RegistrationToken { Token = "B" }
            };

            var mockRegistrationTokenRepository = new Mock<IRegistrationTokenRepository>(MockBehavior.Strict);

            mockRegistrationTokenRepository
                .SetupGet(r => r.RegistrationTokens)
                .Returns(registrationTokens);

            var validator = new RegistrationTokenValidator(mockRegistrationTokenRepository.Object);

            Assert.Equal(expectedIsValid, validator.TokenIsValid(token));
        }
    }
}