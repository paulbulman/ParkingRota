namespace ParkingRota.UnitTests.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
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
                            r.ExpiryTime == existingRegistrationToken.ExpiryTime &&
                            r.Token == existingRegistrationToken.Token));
                }
            }
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(this.contextOptions);

        private void SeedDatabase(IEnumerable<DataRegistrationToken> registrationTokens)
        {
            using (var context = this.CreateContext())
            {
                context.RegistrationTokens.AddRange(registrationTokens);
                context.SaveChanges();
            }
        }
    }
}