namespace ParkingRota.UnitTests.Pages
{
    using System.Linq;
    using ParkingRota.Business.Model;
    using ParkingRota.Pages;
    using Xunit;

    public static class RegistrationNumbersModelTests
    {
        [Fact]
        public static void Test_Get()
        {
            var mockUserManager = TestHelpers.CreateMockUserManager();

            var user = new ApplicationUser { FirstName = "Colm", LastName = "Wilkinson", CarRegistrationNumber = "W789XYZ" };
            var otherUser = new ApplicationUser { FirstName = "Philip", LastName = "Quast", CarRegistrationNumber = "AB12CDE" };

            var applicationUsers = new[] { user, otherUser };

            mockUserManager
                .SetupGet(u => u.Users)
                .Returns(applicationUsers.AsQueryable());

            var model = new RegistrationNumbersModel(mockUserManager.Object);

            model.OnGet();

            var result = model.ApplicationUsers;

            Assert.NotNull(result);

            Assert.Equal(applicationUsers.Length, result.Count);

            Assert.All(
                applicationUsers,
                expected => Assert.Single(
                    result.Where(
                        actual =>
                            actual.FirstName == expected.FirstName &&
                            actual.LastName == expected.LastName &&
                            actual.CarRegistrationNumber == expected.CarRegistrationNumber)));

            Assert.Equal(otherUser.CarRegistrationNumber, result.First().CarRegistrationNumber);
        }
    }
}