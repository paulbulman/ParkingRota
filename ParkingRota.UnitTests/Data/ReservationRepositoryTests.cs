namespace ParkingRota.UnitTests.Data
{
    using System;
    using System.Linq;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
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

            // Act
            using (var context = this.CreateContext())
            {
                var repository = new ReservationRepository(
                    context,
                    Mock.Of<IDateCalculator>(),
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

        [Fact]
        public void Test_UpdateReservations()
        {
            // Arrange
            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);

            mockDateCalculator
                .Setup(d => d.GetActiveDates())
                .Returns(new[] { 3.November(2018), 4.November(2018), 5.November(2018) });

            var user1 = new ApplicationUser();
            var user2 = new ApplicationUser();

            var existingReservationToRemove = new DataReservation { ApplicationUser = user1, Date = 3.November(2018), Order = 0 };
            var existingReservationToKeep = new DataReservation { ApplicationUser = user2, Date = 4.November(2018), Order = 1 };

            var existingReservationOutsideActivePeriod = new DataReservation { ApplicationUser = user1, Date = 6.November(2018), Order = 0 };

            this.SeedDatabase(existingReservationToRemove, existingReservationToKeep, existingReservationOutsideActivePeriod);

            // Act
            var existingReservation = new ModelReservation
            {
                ApplicationUser = existingReservationToKeep.ApplicationUser,
                Date = existingReservationToKeep.Date,
                Order = existingReservationToKeep.Order
            };

            var newReservation = new ModelReservation
            {
                ApplicationUser = existingReservationToRemove.ApplicationUser,
                Date = existingReservationToRemove.Date,
                Order = existingReservationToRemove.Order + 1
            };

            using (var context = this.CreateContext())
            {
                new ReservationRepository(context, mockDateCalculator.Object, Mock.Of<IMapper>())
                    .UpdateReservations(new[] { existingReservation, newReservation });
            }

            // Assert
            var expectedNewReservation = new DataReservation
            {
                ApplicationUser = newReservation.ApplicationUser,
                Date = newReservation.Date,
                Order = newReservation.Order
            };

            var expectedReservations = new[]
            {
                existingReservationToKeep,
                existingReservationOutsideActivePeriod,
                expectedNewReservation
            };

            using (var context = this.CreateContext())
            {
                var result = context.Reservations
                    .Include(r => r.ApplicationUser)
                    .ToArray();

                Assert.Equal(expectedReservations.Length, result.Length);

                Assert.All(
                    expectedReservations,
                    e => Assert.Contains(
                        result,
                        r => r.ApplicationUser.Id == e.ApplicationUser.Id && r.Date == e.Date && r.Order == e.Order));
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