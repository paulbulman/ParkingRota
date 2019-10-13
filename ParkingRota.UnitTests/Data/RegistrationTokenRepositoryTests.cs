namespace ParkingRota.UnitTests.Data
{
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Model;
    using ParkingRota.Data;
    using Xunit;
    using ModelRegistrationToken = ParkingRota.Business.Model.RegistrationToken;

    public class RegistrationTokenRepositoryTests : DatabaseTests
    {
        [Fact]
        public void Test_GetRegistrationTokens()
        {
            // Arrange
            var existingRegistrationTokens = new[]
            {
                this.Seed.RegistrationToken("ABC", 8.May(2019).At(15, 44, 38).Utc()),
                this.Seed.RegistrationToken("XYZ", 9.May(2019).At(16, 32, 14).Utc())
            };

            using (var scope = this.CreateScope())
            {
                // Act
                var result = scope.ServiceProvider
                    .GetRequiredService<IRegistrationTokenRepository>()
                    .GetRegistrationTokens();

                // Assert
                Assert.Equal(existingRegistrationTokens.Length, result.Count);

                foreach (var existingRegistrationToken in existingRegistrationTokens)
                {
                    Assert.Single(
                        result.Where(r =>
                            r.Token == existingRegistrationToken.Token &&
                            r.ExpiryTime == existingRegistrationToken.ExpiryTime));
                }
            }
        }

        [Fact]
        public void Test_AddRegistrationToken()
        {
            // Arrange
            var existingToken = this.Seed.RegistrationToken("ABC", 8.May(2019).At(15, 44, 38).Utc());

            var tokenToAdd = new ModelRegistrationToken
            {
                ExpiryTime = 10.May(2019).At(07, 15, 21).Utc(),
                Token = "XYZ"
            };

            using (var scope = this.CreateScope())
            {
                // Act
                scope.ServiceProvider
                    .GetRequiredService<IRegistrationTokenRepository>()
                    .AddRegistrationToken(tokenToAdd);
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var expectedTokens = new[] { existingToken.Token, tokenToAdd.Token };

                var result = context.RegistrationTokens.ToArray();

                Assert.Equal(expectedTokens.Length, result.Length);

                foreach (var expectedToken in expectedTokens)
                {
                    Assert.Single(result.Where(t => t.Token == expectedToken));
                }

                var savedToken = result.Single(t => t.Token == tokenToAdd.Token);

                Assert.Equal(tokenToAdd.ExpiryTime, savedToken.ExpiryTime);
                Assert.NotEqual(0, savedToken.Id);
            }
        }

        [Fact]
        public void Test_DeleteRegistrationToken()
        {
            // Arrange
            var tokenToKeep = this.Seed.RegistrationToken("ABC", 8.May(2019).At(15, 44, 38).Utc());
            var tokenToDelete = this.Seed.RegistrationToken("XYZ", 9.May(2019).At(16, 32, 14).Utc());

            // Act
            using (var scope = this.CreateScope())
            {
                // Act
                scope.ServiceProvider
                    .GetRequiredService<IRegistrationTokenRepository>()
                    .DeleteRegistrationToken(tokenToDelete.Token);
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var result = context.RegistrationTokens.ToArray();

                Assert.Single(result);

                Assert.Equal(tokenToKeep.Token, result[0].Token);
                Assert.Equal(tokenToKeep.ExpiryTime, result[0].ExpiryTime);
            }
        }
    }
}