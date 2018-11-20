namespace ParkingRota.UnitTests.Pages
{
    using System.Linq;
    using System.Security.Claims;
    using ParkingRota.Business.Model;
    using ParkingRota.Pages;
    using Xunit;

    public static class OverrideRequestsModelTests
    {
        [Fact]
        public static void Test_Get()
        {
            var principal = new ClaimsPrincipal();
            var loggedInUser = new ApplicationUser { FirstName = "Colm", LastName = "Wilkinson" };
            var otherUser = new ApplicationUser { FirstName = "Philip", LastName = "Quast" };

            var applicationUsers = new[] { loggedInUser, otherUser };

            var mockUserManager = TestHelpers.CreateMockUserManager(principal, loggedInUser);
            mockUserManager
                .SetupGet(u => u.Users)
                .Returns(applicationUsers.AsQueryable());

            var model = new OverrideRequestsModel(mockUserManager.Object);

            model.OnGet(loggedInUser.Id);

            Assert.Equal(loggedInUser.Id, model.SelectedUserId);

            Assert.Equal(applicationUsers.Length, model.Users.Count);

            Assert.All(
                applicationUsers,
                u => Assert.Single(model.Users.Where(l => l.Value == u.Id && l.Text == u.FullName)));
        }
    }
}