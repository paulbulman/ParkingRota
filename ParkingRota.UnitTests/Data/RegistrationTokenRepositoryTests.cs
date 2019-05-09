namespace ParkingRota.UnitTests.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Data;
    using Xunit;
    using DataRegistrationToken = ParkingRota.Data.RegistrationToken;
    using ModelRegistrationToken = ParkingRota.Business.Model.RegistrationToken;

    public class RegistrationTokenRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> contextOptions;

        public RegistrationTokenRepositoryTests() =>
            this.contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        [Fact]
        public void Test_GetRegistrationTokens()
        {
            // Arrange
            var existingRegistrationTokens = new[]
            {
                new DataRegistrationToken { ExpiryTime = 8.May(2019).At(15, 44, 38).Utc(), Token = "ABC" },
                new DataRegistrationToken { ExpiryTime = 9.May(2019).At(16, 32, 14).Utc(), Token = "XYZ" }
            };

            this.SeedDatabase(existingRegistrationTokens);

            var mapperConfiguration =
                new MapperConfiguration(c => c.CreateMap<DataRegistrationToken, ModelRegistrationToken>());

            var mapper = new Mapper(mapperConfiguration);

            using (var context = this.CreateContext())
            {
                // Act
                var result = new RegistrationTokenRepository(context, mapper).GetRegistrationTokens();

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
            var existingToken = new DataRegistrationToken
            {
                ExpiryTime = 8.May(2019).At(15, 44, 38).Utc(),
                Token = "ABC"
            };

            this.SeedDatabase(existingToken);

            var tokenToAdd = new ModelRegistrationToken
            {
                ExpiryTime = 10.May(2019).At(07, 15, 21).Utc(),
                Token = "XYZ"
            };

            // Act
            using (var context = this.CreateContext())
            {
                new RegistrationTokenRepository(context, Mock.Of<IMapper>()).AddRegistrationToken(tokenToAdd);
            }

            // Assert
            using (var context = this.CreateContext())
            {
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
            var tokenToKeep = new DataRegistrationToken
            {
                ExpiryTime = 8.May(2019).At(15, 44, 38).Utc(),
                Token = "ABC"
            };

            var tokenToDelete = new DataRegistrationToken
            {
                ExpiryTime = 9.May(2019).At(16, 32, 14).Utc(),
                Token = "XYZ"
            };

            var existingRegistrationTokens = new[] { tokenToKeep, tokenToDelete };

            this.SeedDatabase(existingRegistrationTokens);

            // Act
            using (var context = this.CreateContext())
            {
                new RegistrationTokenRepository(context, Mock.Of<IMapper>())
                    .DeleteRegistrationToken(tokenToDelete.Token);
            }

            // Assert
            using (var context = this.CreateContext())
            {
                var result = context.RegistrationTokens.ToArray();

                Assert.Single(result);

                Assert.Equal(tokenToKeep.Token, result[0].Token);
                Assert.Equal(tokenToKeep.ExpiryTime, result[0].ExpiryTime);
            }
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(this.contextOptions);

        private void SeedDatabase(params DataRegistrationToken[] registrationTokens)
        {
            using (var context = this.CreateContext())
            {
                context.RegistrationTokens.AddRange(registrationTokens);
                context.SaveChanges();
            }
        }
    }
}