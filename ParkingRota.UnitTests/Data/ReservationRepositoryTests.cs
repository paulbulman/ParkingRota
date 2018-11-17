namespace ParkingRota.UnitTests.Data
{
    using System;
    using System.Collections.Generic;
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

            // Act
            IReadOnlyList<ModelReservation> result;
            using (var context = this.CreateContext())
            {
                result = CreateRepository(context).GetReservations(firstDate, lastDate);
            }

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

        [Fact]
        public void Test_DeleteReservations()
        {
            // Arrange
            var user1 = new ApplicationUser();
            var user2 = new ApplicationUser();

            this.SeedDatabase(user1, user2);

            var firstDate = 3.November(2018);
            var lastDate = 5.November(2018);

            var existingReservation = new ModelReservation { ApplicationUser = user1, Date = firstDate, Order = 0 };
            var alreadyDeletedReservation = new ModelReservation { ApplicationUser = user1, Date = lastDate, Order = 0 };

            var matchingReservation = new DataReservation
            {
                ApplicationUserId = existingReservation.ApplicationUser.Id,
                Date = existingReservation.Date,
                Order = existingReservation.Order
            };

            var otherReservations = new[]
            {
                new DataReservation
                {
                    ApplicationUserId = user2.Id,
                    Date = existingReservation.Date,
                    Order = existingReservation.Order
                },
                new DataReservation
                {
                    ApplicationUserId = existingReservation.ApplicationUser.Id,
                    Date = existingReservation.Date.PlusDays(-1),
                    Order = existingReservation.Order
                },
                new DataReservation
                {
                    ApplicationUserId = existingReservation.ApplicationUser.Id,
                    Date = existingReservation.Date,
                    Order = existingReservation.Order - 1
                }
            };

            this.SeedDatabase(matchingReservation);
            this.SeedDatabase(otherReservations);

            // Act
            using (var context = this.CreateContext())
            {
                CreateRepository(context).RemoveReservations(new[] { existingReservation, alreadyDeletedReservation });
            }

            // Assert
            using (var context = this.CreateContext())
            {
                var remainingReservations = context.Reservations.Include(r => r.ApplicationUser).ToArray();

                Assert.Equal(otherReservations.Length, remainingReservations.Length);

                foreach (var expectedReservation in otherReservations)
                {
                    Assert.Single(remainingReservations.Where(r =>
                        r.ApplicationUserId == expectedReservation.ApplicationUserId &&
                        r.Date == expectedReservation.Date &&
                        r.Order == expectedReservation.Order));
                }
            }
        }

        [Fact]
        public void Test_AddReservations()
        {
            // Arrange
            var user1 = new ApplicationUser();
            var user2 = new ApplicationUser();

            this.SeedDatabase(user1, user2);

            var firstDate = 3.November(2018);
            var lastDate = 5.November(2018);

            var existingReservation = new ModelReservation { ApplicationUser = user1, Date = firstDate, Order = 0 };

            this.SeedDatabase(new DataReservation
            {
                ApplicationUserId = existingReservation.ApplicationUser.Id,
                Date = existingReservation.Date,
                Order = existingReservation.Order
            });

            var allReservations = new[]
            {
                existingReservation,
                new ModelReservation { ApplicationUser = user1, Date = firstDate, Order = 1 },
                new ModelReservation { ApplicationUser = user2, Date = firstDate, Order = 2 },
                new ModelReservation { ApplicationUser = user2, Date = lastDate, Order = 0 }
            };

            // Act
            using (var context = this.CreateContext())
            {
                CreateRepository(context).AddReservations(allReservations);
            }

            // Assert
            using (var context = this.CreateContext())
            {
                var savedReservations = context.Reservations.Include(r => r.ApplicationUser).ToArray();

                Assert.Equal(allReservations.Length, savedReservations.Length);

                foreach (var expectedReservation in allReservations)
                {
                    Assert.Single(savedReservations.Where(r =>
                        r.ApplicationUser.Id == expectedReservation.ApplicationUser.Id &&
                        r.Date == expectedReservation.Date &&
                        r.Order == expectedReservation.Order));
                }
            }
        }

        private static ReservationRepository CreateRepository(IApplicationDbContext context) =>
            new ReservationRepository(
                context,
                new Mapper(new MapperConfiguration(c => { c.CreateMap<DataReservation, ModelReservation>(); })));

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(this.contextOptions);

        private void SeedDatabase(params DataReservation[] reservations)
        {
            using (var context = this.CreateContext())
            {
                context.Reservations.AddRange(reservations);
                context.SaveChanges();
            }
        }

        private void SeedDatabase(params ApplicationUser[] applicationUsers)
        {
            using (var context = this.CreateContext())
            {
                context.Users.AddRange(applicationUsers);
                context.SaveChanges();
            }
        }
    }
}