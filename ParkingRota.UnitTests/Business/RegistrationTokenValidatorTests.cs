namespace ParkingRota.UnitTests.Business
{
    using Data;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using Xunit;

    public class RegistrationTokenValidatorTests : DatabaseTests
    {
        [Theory]
        [InlineData("A", true)]
        [InlineData("a", true)]
        [InlineData("B", true)]
        [InlineData("C", false)]
        [InlineData("", false)]
        public void Test_TokenIsValid_StringExists(string token, bool expectedIsValid)
        {
            // Arrange
            var currentInstant = 15.September(2018).At(17, 37, 01).Utc();
            this.SetClock(currentInstant);

            this.Seed.RegistrationToken("A", currentInstant.Plus(1.Seconds()));
            this.Seed.RegistrationToken("B", currentInstant.Plus(1.Seconds()));

            using (var scope = this.CreateScope())
            {
                // Act
                var result = scope.ServiceProvider
                    .GetRequiredService<IRegistrationTokenValidator>()
                    .TokenIsValid(token);

                // Assert
                Assert.Equal(expectedIsValid, result);
            }
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(-1, false)]
        public void Test_TokenIsValid_NotExpired(int expiryOffsetSeconds, bool expectedIsValid)
        {
            const string Token = "A";

            var currentInstant = 15.September(2018).At(17, 37, 01).Utc();
            this.SetClock(currentInstant);

            this.Seed.RegistrationToken(Token, currentInstant.Plus(expiryOffsetSeconds.Seconds()));

            using (var scope = this.CreateScope())
            {
                // Act
                var result = scope.ServiceProvider
                    .GetRequiredService<IRegistrationTokenValidator>()
                    .TokenIsValid(Token);

                // Assert
                Assert.Equal(expectedIsValid, result);
            }
        }
    }
}