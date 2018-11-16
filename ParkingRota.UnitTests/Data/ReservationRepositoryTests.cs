namespace ParkingRota.UnitTests.Data
{
    using System;
    using System.Linq;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Model;
    using ParkingRota.Data;
    using Xunit;
    using DataReservation = ParkingRota.Data.Reservation;
    using ModelReservation = ParkingRota.Business.Model.Reservation;

    public class ReservationRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> contextOptions;

        public ReservationRepositoryTests() =>
            this.contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        [Fact]
        public void Test_GetReservations()
        {
            // Arrange
            var user1 = new ApplicationUser();
            var user2 = new ApplicationUser();

            var firstDate = 3.November(2018);
            var lastDate = 5.November(2018);

            var matchingReservations = new[]
            {
                new DataReservation { ApplicationUser = user1, Date = firstDate, Order = 1 },
                new DataReservation { ApplicationUser = user1, Date = lastDate, Order = 1 },
                new DataReservation { ApplicationUser = user2, Date = firstDate, Order = 2 }
            };

            var filteredOutReservations = new[]
            {
                new DataReservation { ApplicationUser = user1, Date = firstDate.PlusDays(-1), Order = 1 },
                new DataReservation { ApplicationUser = user2, Date = lastDate.PlusDays(1), Order = 1 }
            };

            this.SeedDatabase(matchingReservations.Concat(filteredOutReservations).ToArray());

            var mapperConfiguration = new MapperConfiguration(c =>
            {
                c.CreateMap<DataReservation, ModelReservation>();
            });

            using (var context = this.CreateContext())
            {
                // Act
                var repository = new ReservationRepository(
                    context,
                    new Mapper(mapperConfiguration));

                var result = repository.GetReservations(firstDate, lastDate);

                // Assert
                Assert.Equal(matchingReservations.Length, result.Count);

                foreach (var expectedReservation in matchingReservations)
                {
                    Assert.Single(result.Where(r =>
                        r.ApplicationUser.Id == expectedReservation.ApplicationUser.Id &&
                        r.Date == expectedReservation.Date &&
                        r.Order == expectedReservation.Order));
                }
            }
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(this.contextOptions);

        private void SeedDatabase(params DataReservation[] reservations)
        {
            using (var context = this.CreateContext())
            {
                context.Reservations.AddRange(reservations);
                context.SaveChanges();
            }
        }
    }
}