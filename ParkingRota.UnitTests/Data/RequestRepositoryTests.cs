namespace ParkingRota.UnitTests.Data
{
    using System.Linq;
    using AutoMapper;
    using Moq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Model;
    using ParkingRota.Data;
    using Xunit;
    using Request = ParkingRota.Data.Request;

    public class RequestRepositoryTests
    {
        [Fact]
        public void Test_GetRequets()
        {
            // Arrange
            var user1 = new ApplicationUser();
            var user2 = new ApplicationUser();

            var firstDate = 3.November(2018);
            var lastDate = 5.November(2018);

            var matchingRequests = new[]
            {
                new Request { ApplicationUser = user1, Date = firstDate },
                new Request { ApplicationUser = user1, Date = lastDate },
                new Request { ApplicationUser = user2, Date = firstDate }
            };

            var filteredOutRequests = new[]
            {
                new Request { Date = firstDate.PlusDays(-1) },
                new Request { Date = lastDate.PlusDays(1) }
            };

            var mockContext = new Mock<IApplicationDbContext>(MockBehavior.Strict);

            mockContext
                .SetupGet(c => c.Requests)
                .Returns(matchingRequests.Concat(filteredOutRequests).ToDbSet);

            var mapperConfiguration = new MapperConfiguration(c =>
            {
                c.CreateMap<Request, ParkingRota.Business.Model.Request>();
            });

            // Act
            var repository = new RequestRepository(mockContext.Object, new Mapper(mapperConfiguration));
            var result = repository.GetRequests(firstDate, lastDate);

            // Assert
            Assert.Equal(matchingRequests.Length, result.Count);

            foreach (var expectedRequest in matchingRequests)
            {
                Assert.Single(result.Where(r =>
                    r.ApplicationUser == expectedRequest.ApplicationUser &&
                    r.Date == expectedRequest.Date));
            }
        }
    }
}