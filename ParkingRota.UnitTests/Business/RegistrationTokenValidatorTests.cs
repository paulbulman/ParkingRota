namespace ParkingRota.UnitTests.Business
{
    using Data;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Data;
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

            var registrationTokens = new[]
            {
                new RegistrationToken { Token = "A", ExpiryTime = currentInstant.Plus(1.Seconds()) },
                new RegistrationToken { Token = "B", ExpiryTime = currentInstant.Plus(1.Seconds()) }
            };

            this.SeedDatabase(registrationTokens);

            using (var context = this.CreateContext())
            {
                // Act
                var result = new RegistrationTokenValidatorBuilder()
                    .WithCurrentInstant(currentInstant)
                    .Build(context)
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

            var registrationToken = new RegistrationToken
            {
                Token = Token,
                ExpiryTime = currentInstant.Plus(expiryOffsetSeconds.Seconds())
            };
            
            this.SeedDatabase(registrationToken);

            using (var context = this.CreateContext())
            {
                // Act
                var result = new RegistrationTokenValidatorBuilder()
                    .WithCurrentInstant(currentInstant)
                    .Build(context)
                    .TokenIsValid(Token);

                // Assert
                Assert.Equal(expectedIsValid, result);
            }
        }

        private void SeedDatabase(params RegistrationToken[] registrationTokens)
        {
            using (var context = this.CreateContext())
            {
                context.RegistrationTokens.AddRange(registrationTokens);
                context.SaveChanges();
            }
        }
    }
}