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
    using DataRequest = ParkingRota.Data.Request;
    using ModelRequest = ParkingRota.Business.Model.Request;

    public class RequestRepositoryTests : DatabaseTests
    {
        [Fact]
        public async Task Test_GetRequests()
        {
            // Arrange
            var user1 = await this.Seed.ApplicationUser("a@b.c");
            var user2 = await this.Seed.ApplicationUser("d@e.f");

            var firstDate = 6.November(2018);
            var lastDate = 8.November(2018);

            var matchingRequests = new[]
            {
                this.Seed.Request(user1, firstDate),
                this.Seed.Request(user1, lastDate),
                this.Seed.Request(user2, firstDate)
            };

            // Should be filtered out
            this.Seed.Request(user1, firstDate.PlusDays(-1));
            this.Seed.Request(user2, lastDate.PlusDays(1));

            using (var scope = this.CreateScope())
            {
                // Act
                var result = scope.ServiceProvider
                    .GetRequiredService<IRequestRepository>()
                    .GetRequests(firstDate, lastDate);

                // Assert
                Assert.Equal(matchingRequests.Length, result.Count);

                foreach (var expectedRequest in matchingRequests)
                {
                    Assert.Single(result.Where(r =>
                        r.ApplicationUser.Id == expectedRequest.ApplicationUser.Id &&
                        r.Date == expectedRequest.Date));
                }
            }
        }

        [Fact]
        public async Task Test_UpdateRequests()
        {
            // Arrange
            this.SetClock(6.November(2018).At(11, 0, 0).Utc());

            var user = await this.Seed.ApplicationUser("a@b.c");
            var otherUser = await this.Seed.ApplicationUser("d@e.f");

            var existingRequestToRemove = this.Seed.Request(user, 6.November(2018));
            this.Seed.Allocation(user, 6.November(2018));

            var existingRequestToKeep = this.Seed.Request(user, 8.November(2018));
            this.Seed.Allocation(user, 8.November(2018));

            var existingRequestOutsideActivePeriod = this.Seed.Request(user, 5.November(2018));
            this.Seed.Allocation(user, 5.November(2018));

            var otherUserRequest = this.Seed.Request(otherUser, 6.November(2018));
            this.Seed.Allocation(otherUser, 6.November(2018));

            // Act
            var existingRequest = new ModelRequest { ApplicationUser = user, Date = existingRequestToKeep.Date };
            var newRequest = new ModelRequest { ApplicationUser = user, Date = 7.November(2018) };

            using (var scope = this.CreateScope())
            {
                // Act
                scope.ServiceProvider
                    .GetRequiredService<IRequestRepository>()
                    .UpdateRequests(user, new[] { existingRequest, newRequest });
            }

            // Assert
            var expectedRequests = new[]
            {
                existingRequestToKeep,
                existingRequestOutsideActivePeriod,
                otherUserRequest,
                new DataRequest { ApplicationUser = user, Date = newRequest.Date }
            };

            var expectedAllocations = expectedRequests
                .Take(3)
                .Select(r => new DataAllocation { ApplicationUser = r.ApplicationUser, Date = r.Date })
                .ToArray();

            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var requestsResult = context.Requests
                    .Include(r => r.ApplicationUser)
                    .ToArray();

                Assert.Equal(expectedRequests.Length, requestsResult.Length);

                Assert.All(
                    expectedRequests,
                    e => Assert.Contains(
                        requestsResult,
                        r => r.ApplicationUser.Id == e.ApplicationUser.Id && r.Date == e.Date));

                var allocationsResult = context.Allocations
                    .Include(r => r.ApplicationUser)
                    .ToArray();

                Assert.Equal(expectedAllocations.Length, allocationsResult.Length);

                Assert.All(
                    expectedAllocations,
                    e => Assert.Contains(
                        allocationsResult,
                        a => a.ApplicationUser.Id == e.ApplicationUser.Id && a.Date == e.Date));
            }
        }
    }
}