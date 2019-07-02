﻿namespace ParkingRota.UnitTests.Data
{
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Model;
    using ParkingRota.Data;
    using Xunit;
    using DataAllocation = ParkingRota.Data.Allocation;
    using ModelAllocation = ParkingRota.Business.Model.Allocation;

    public class AllocationRepositoryTests : DatabaseTests
    {
        public static AllocationRepository CreateRepository(IApplicationDbContext context) =>
            new AllocationRepository(context, MapperBuilder.Build());

        [Fact]
        public void Test_GetAllocations()
        {
            // Arrange
            var user1 = new ApplicationUser();
            var user2 = new ApplicationUser();

            var firstDate = 3.November(2018);
            var lastDate = 5.November(2018);

            var matchingAllocations = new[]
            {
                new DataAllocation { ApplicationUser = user1, Date = firstDate },
                new DataAllocation { ApplicationUser = user1, Date = lastDate },
                new DataAllocation { ApplicationUser = user2, Date = firstDate }
            };

            var filteredOutAllocations = new[]
            {
                new DataAllocation { ApplicationUser = user1, Date = firstDate.PlusDays(-1) },
                new DataAllocation { ApplicationUser = user2, Date = lastDate.PlusDays(1) }
            };

            this.SeedDatabase(matchingAllocations.Concat(filteredOutAllocations).ToArray());

            using (var context = this.CreateContext())
            {
                // Act
                var result = CreateRepository(context).GetAllocations(firstDate, lastDate);

                // Assert
                Assert.Equal(matchingAllocations.Length, result.Count);

                foreach (var expectedAllocation in matchingAllocations)
                {
                    Assert.Single(result.Where(a =>
                        a.ApplicationUser.Id == expectedAllocation.ApplicationUser.Id &&
                        a.Date == expectedAllocation.Date));
                }
            }
        }

        [Fact]
        public void Test_AddAllocations()
        {
            // Arrange
            var user1 = new ApplicationUser();
            var user2 = new ApplicationUser();

            var date = 3.November(2018);
            var otherDate = 5.November(2018);

            var existingAllocation = new DataAllocation { ApplicationUser = user1, Date = date };
            var otherExistingAllocation = new DataAllocation { ApplicationUser = user2, Date = otherDate };

            this.SeedDatabase(existingAllocation, otherExistingAllocation);

            var newAllocation = new ModelAllocation { ApplicationUser = user1, Date = otherDate };
            var otherNewAllocation = new ModelAllocation { ApplicationUser = user2, Date = date };

            var newAllocations = new[] { newAllocation, otherNewAllocation };

            // Act
            using (var context = this.CreateContext())
            {
                CreateRepository(context).AddAllocations(newAllocations);
            }

            // Assert
            var expectedAllocations = new[]
            {
                existingAllocation,
                otherExistingAllocation,
                new DataAllocation { ApplicationUser = newAllocation.ApplicationUser, Date = newAllocation.Date},
                new DataAllocation { ApplicationUser = otherNewAllocation.ApplicationUser, Date = otherNewAllocation.Date}
            };

            using (var context = this.CreateContext())
            {
                var result = context.Allocations
                    .Include(a => a.ApplicationUser)
                    .ToArray();

                Assert.Equal(expectedAllocations.Length, result.Length);

                foreach (var expectedAllocation in expectedAllocations)
                {
                    Assert.Single(result.Where(a =>
                        a.ApplicationUser.Id == expectedAllocation.ApplicationUser.Id &&
                        a.Date == expectedAllocation.Date));
                }
            }
        }

        private void SeedDatabase(params DataAllocation[] allocations)
        {
            using (var context = this.CreateContext())
            {
                context.Allocations.AddRange(allocations);
                context.SaveChanges();
            }
        }
    }
}