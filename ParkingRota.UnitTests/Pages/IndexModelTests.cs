namespace ParkingRota.UnitTests.Pages
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Pages;
    using Xunit;

    public class IndexModelTests
    {
        [Fact]
        public async Task Test_Get()
        {
            // Arrange
            var tuesday = 6.November(2018);
            var wednesday = 7.November(2018);
            var thursday = 8.November(2018);
            var friday = 9.November(2018);

            var principal = new ClaimsPrincipal();
            var loggedInUser = new ApplicationUser { FirstName = "Colm", LastName = "Wilkinson" };
            var otherUser = new ApplicationUser { FirstName = "Philip", LastName = "Quast" };
            var secondOtherUser = new ApplicationUser { FirstName = "Judy", LastName = "Kuhn" };

            // Set up mock date calculator
            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .Setup(d => d.GetActiveDates())
                .Returns(new[] { tuesday, wednesday, thursday, friday });

            // Set up request repository
            var mockRequestRepository = new Mock<IRequestRepository>(MockBehavior.Strict);
            mockRequestRepository
                .Setup(r => r.GetRequests(tuesday, friday))
                .Returns(new[]
                {
                    new Request { ApplicationUser = loggedInUser, Date = tuesday},
                    new Request { ApplicationUser = otherUser, Date = tuesday},
                    new Request { ApplicationUser = secondOtherUser, Date = tuesday},

                    new Request { ApplicationUser = otherUser, Date = wednesday},

                    new Request { ApplicationUser = loggedInUser, Date = thursday},
                    new Request { ApplicationUser = secondOtherUser, Date = thursday}
                });

            var mockAllocationsRepository = new Mock<IAllocationRepository>(MockBehavior.Strict);
            mockAllocationsRepository
                .Setup(r => r.GetAllocations(tuesday, friday))
                .Returns(new[]
                {
                    new Allocation { ApplicationUser = loggedInUser, Date = tuesday},
                    new Allocation { ApplicationUser = otherUser, Date = tuesday},

                    new Allocation { ApplicationUser = otherUser, Date = wednesday}
                });

            // Set up user manager
            var mockUserManager = TestHelpers.CreateMockUserManager(principal, loggedInUser);

            // Act
            var model = new IndexModel(
                mockDateCalculator.Object,
                mockRequestRepository.Object,
                mockAllocationsRepository.Object,
                mockUserManager.Object)
            {
                PageContext = { HttpContext = new DefaultHttpContext { User = principal } }
            };

            await model.OnGetAsync();

            // Assert
            Assert.NotNull(model.Calendar);
            Assert.Single(model.Calendar.Weeks);
            Assert.Equal(5.November(2018), model.Calendar.Weeks[0].Days[0].Date);

            Assert.Equal(new[] { tuesday, wednesday, thursday, friday }, model.Calendar.ActiveDates());

            CheckDay(model.Calendar.Data(tuesday), new[] { "Philip Quast", "Colm Wilkinson" }, new[] { "Judy Kuhn" });
            CheckDay(model.Calendar.Data(wednesday), new[] { "Philip Quast" }, new List<string>());
            CheckDay(model.Calendar.Data(thursday), new List<string>(), new[] { "Judy Kuhn", "Colm Wilkinson" });
            CheckDay(model.Calendar.Data(friday), new List<string>(), new List<string>());
        }

        private static void CheckDay(
            IndexModel.DisplayRequests displayRequests,
            IReadOnlyList<string> expectedAllocatedNames,
            IReadOnlyList<string> expectedUnallocatedNames)
        {
            Assert.NotNull(displayRequests);

            CheckRequests(displayRequests.AllocatedRequests, expectedAllocatedNames);
            CheckRequests(displayRequests.UnallocatedRequests, expectedUnallocatedNames);
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
        private static void CheckRequests(IReadOnlyList<IndexModel.DisplayRequest> actual, IReadOnlyList<string> expected)
        {
            const string LoggedInUserName = "Colm Wilkinson";

            Assert.NotNull(actual);

            Assert.Equal(actual.Count, expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], actual[i].FullName);
                Assert.Equal(expected[i] == LoggedInUserName, actual[i].IsCurrentUser);
            }
        }
    }
}