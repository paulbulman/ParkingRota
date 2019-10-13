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
    using DataAllocation = ParkingRota.Data.Allocation;
    using ModelAllocation = ParkingRota.Business.Model.Allocation;

    public class AllocationRepositoryTests : DatabaseTests
    {
        [Fact]
        public async Task Test_GetAllocations()
        {
            // Arrange
            var user1 = await this.Seed.ApplicationUser("a@b.c");
            var user2 = await this.Seed.ApplicationUser("d@e.f");

            var firstDate = 3.November(2018);
            var lastDate = 5.November(2018);

            var matchingAllocations = new[]
            {
                this.Seed.Allocation(user1, firstDate),
                this.Seed.Allocation(user1, lastDate),
                this.Seed.Allocation(user2, firstDate)
            };

            // Should be filtered out
            this.Seed.Allocation(user1, firstDate.PlusDays(-1));
            this.Seed.Allocation(user2, lastDate.PlusDays(1));

            using (var scope = this.CreateScope())
            {
                // Act
                var result = scope.ServiceProvider
                    .GetRequiredService<IAllocationRepository>()
                    .GetAllocations(firstDate, lastDate);

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
        public async Task Test_AddAllocations()
        {
            // Arrange
            var user1 = await this.Seed.ApplicationUser("a@b.c");
            var user2 = await this.Seed.ApplicationUser("d@e.f");

            var date = 3.November(2018);
            var otherDate = 5.November(2018);

            var existingAllocation = this.Seed.Allocation(user1, date);
            var otherExistingAllocation = this.Seed.Allocation(user2, otherDate);

            var newAllocation = new ModelAllocation { ApplicationUser = user1, Date = otherDate };
            var otherNewAllocation = new ModelAllocation { ApplicationUser = user2, Date = date };

            var newAllocations = new[] { newAllocation, otherNewAllocation };

            using (var scope = this.CreateScope())
            {
                // Act
                scope.ServiceProvider
                    .GetRequiredService<IAllocationRepository>()
                    .AddAllocations(newAllocations);
            }

            // Assert
            var expectedAllocations = new[]
            {
                existingAllocation,
                otherExistingAllocation,
                new DataAllocation { ApplicationUser = newAllocation.ApplicationUser, Date = newAllocation.Date},
                new DataAllocation { ApplicationUser = otherNewAllocation.ApplicationUser, Date = otherNewAllocation.Date}
            };

            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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
    }
}