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

            var user = new ApplicationUser
            {
                FirstName = "Colm",
                LastName = "Wilkinson",
                CarRegistrationNumber = "W789XYZ"
            };

            var otherUser = new ApplicationUser
            {
                FirstName = "Philip",
                LastName = "Quast",
                CarRegistrationNumber = "Z987YXW",
                AlternativeCarRegistrationNumber = "AB12CDE",
            };

            var applicationUsers = new[] { user, otherUser };

            mockUserManager
                .SetupGet(u => u.Users)
                .Returns(applicationUsers.AsQueryable());

            var model = new RegistrationNumbersModel(mockUserManager.Object);

            model.OnGet();

            var result = model.RegistrationNumberRecords;

            var expectedRecords = new[]
            {
                new RegistrationNumbersModel.RegistrationNumberRecord("Philip Quast", "AB12CDE"),
                new RegistrationNumbersModel.RegistrationNumberRecord("Colm Wilkinson", "W789XYZ"),
                new RegistrationNumbersModel.RegistrationNumberRecord("Philip Quast", "Z987YXW"),
            };

            Assert.NotNull(result);

            Assert.Equal(expectedRecords.Length, result.Count);

            for (var i = 0; i < expectedRecords.Length; i++)
            {
                Assert.Equal(expectedRecords[i].FullName, result[i].FullName);
                Assert.Equal(expectedRecords[i].CarRegistrationNumber, result[i].CarRegistrationNumber);
            }
        }
    }
}