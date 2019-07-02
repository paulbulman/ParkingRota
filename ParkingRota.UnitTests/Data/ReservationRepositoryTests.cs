namespace ParkingRota.UnitTests.Data
{
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Model;
    using Xunit;
    using DataReservation = ParkingRota.Data.Reservation;
    using ModelReservation = ParkingRota.Business.Model.Reservation;

    public class ReservationRepositoryTests : DatabaseTests
    {
        [Fact]
        public void Test_GetReservations()
        {
            // Arrange
            var user1 = new ApplicationUser();
            var user2 = new ApplicationUser();

            var firstDate = 6.November(2018);
            var lastDate = 8.November(2018);

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

            using (var context = this.CreateContext())
            {
                // Act
                var result = new ReservationRepositoryBuilder()
                    .Build(context)
                    .GetReservations(firstDate, lastDate);

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
            var user1 = new ApplicationUser();
            var user2 = new ApplicationUser();

            var existingReservationToRemove = new DataReservation { ApplicationUser = user1, Date = 6.November(2018), Order = 0 };
            var existingReservationToKeep = new DataReservation { ApplicationUser = user2, Date = 8.November(2018), Order = 1 };

            var existingReservationOutsideActivePeriod = new DataReservation { ApplicationUser = user1, Date = 5.November(2018), Order = 0 };

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
                new ReservationRepositoryBuilder()
                    .WithCurrentInstant(6.November(2018).At(10, 0, 0).Utc())
                    .Build(context)
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