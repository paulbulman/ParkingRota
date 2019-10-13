namespace ParkingRota.UnitTests.Data
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Model;
    using ParkingRota.Data;
    using Xunit;
    using DataReservation = ParkingRota.Data.Reservation;
    using ModelReservation = ParkingRota.Business.Model.Reservation;

    public class ReservationRepositoryTests : DatabaseTests
    {
        [Fact]
        public async Task Test_GetReservations()
        {
            // Arrange
            var user1 = await this.Seed.ApplicationUser("a@b.c");
            var user2 = await this.Seed.ApplicationUser("d@e.f");

            var firstDate = 6.November(2018);
            var lastDate = 8.November(2018);

            var matchingReservations = new[]
            {
                this.Seed.Reservation(user1, firstDate, 0),
                this.Seed.Reservation(user1, lastDate, 0),
                this.Seed.Reservation(user2, firstDate, 1)
            };

            // Should be filtered out
            this.Seed.Reservation(user1, firstDate.PlusDays(-1), 0);
            this.Seed.Reservation(user2, lastDate.PlusDays(1), 0);

            using (var scope = this.CreateScope())
            {
                // Act
                var result = scope.ServiceProvider
                    .GetRequiredService<IReservationRepository>()
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
        public async Task Test_UpdateReservations()
        {
            // Arrange
            this.SetClock(6.November(2018).At(10, 0, 0).Utc());

            var user1 = await this.Seed.ApplicationUser("a@b.c");
            var user2 = await this.Seed.ApplicationUser("d@e.f");

            var existingReservationToRemove = Seed.Reservation(user1, 6.November(2018), 0);
            var existingReservationToKeep = Seed.Reservation(user2, 8.November(2018), 1);

            var existingReservationOutsideActivePeriod = Seed.Reservation(user1, 5.November(2018), 0);

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

            using (var scope = this.CreateScope())
            {
                // Act
                scope.ServiceProvider
                    .GetRequiredService<IReservationRepository>()
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

            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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
    }
}