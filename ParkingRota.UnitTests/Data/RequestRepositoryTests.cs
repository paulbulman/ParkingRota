namespace ParkingRota.UnitTests.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Data;
    using Xunit;
    using DataAllocation = ParkingRota.Data.Allocation;
    using DataRequest = ParkingRota.Data.Request;
    using ModelRequest = ParkingRota.Business.Model.Request;

    public class RequestRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> contextOptions;

        public RequestRepositoryTests() =>
            this.contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        [Fact]
        public void Test_GetRequests()
        {
            // Arrange
            var user1 = new ApplicationUser();
            var user2 = new ApplicationUser();

            var firstDate = 3.November(2018);
            var lastDate = 5.November(2018);

            var matchingRequests = new[]
            {
                new DataRequest { ApplicationUser = user1, Date = firstDate },
                new DataRequest { ApplicationUser = user1, Date = lastDate },
                new DataRequest { ApplicationUser = user2, Date = firstDate }
            };

            var filteredOutRequests = new[]
            {
                new DataRequest { ApplicationUser = user1, Date = firstDate.PlusDays(-1) },
                new DataRequest { ApplicationUser = user2, Date = lastDate.PlusDays(1) }
            };

            this.SeedDatabase(matchingRequests.Concat(filteredOutRequests).ToArray(), new List<DataAllocation>());

            var mapperConfiguration = new MapperConfiguration(c =>
            {
                c.CreateMap<DataRequest, ModelRequest>();
            });

            using (var context = this.CreateContext())
            {
                // Act
                var repository = new RequestRepository(
                    context,
                    Mock.Of<IDateCalculator>(),
                    new Mapper(mapperConfiguration));

                var result = repository.GetRequests(firstDate, lastDate);

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
        public void Test_UpdateRequests()
        {
            // Arrange
            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);

            mockDateCalculator
                .Setup(d => d.GetActiveDates())
                .Returns(new[] { 3.November(2018), 4.November(2018), 5.November(2018) });

            var user = new ApplicationUser();

            var existingRequestToRemove = new DataRequest { ApplicationUser = user, Date = 3.November(2018) };
            var existingRequestToKeep = new DataRequest { ApplicationUser = user, Date = 5.November(2018) };

            var existingRequestOutsideActivePeriod = new DataRequest { ApplicationUser = user, Date = 6.November(2018) };

            var otherUserRequest = new DataRequest { ApplicationUser = new ApplicationUser(), Date = 3.November(2018) };

            var requests = new[]
            {
                existingRequestToRemove,
                existingRequestToKeep,
                existingRequestOutsideActivePeriod,
                otherUserRequest
            };

            var allocations = requests
                .Select(r => new DataAllocation { ApplicationUser = r.ApplicationUser, Date = r.Date })
                .ToArray();

            this.SeedDatabase(requests, allocations);

            // Act
            var existingRequest = new ModelRequest { ApplicationUser = user, Date = existingRequestToKeep.Date };
            var newRequest = new ModelRequest { ApplicationUser = user, Date = 4.November(2018) };

            using (var context = this.CreateContext())
            {
                new RequestRepository(context, mockDateCalculator.Object, Mock.Of<IMapper>())
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

            using (var context = this.CreateContext())
            {
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

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(this.contextOptions);

        private void SeedDatabase(IReadOnlyList<DataRequest> requests, IReadOnlyList<DataAllocation> allocations)
        {
            using (var context = this.CreateContext())
            {
                context.Requests.AddRange(requests);
                context.Allocations.AddRange(allocations);

                context.SaveChanges();
            }
        }
    }
}