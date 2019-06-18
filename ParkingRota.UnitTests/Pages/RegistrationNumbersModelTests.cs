using System.Collections.Generic;

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


            CheckResult(expectedRecords, result);
        }

        [Fact]
        public static void Test_Get_ExcludesVisitors()
        {
            var user = new ApplicationUser
            {
                FirstName = "Colm",
                LastName = "Wilkinson",
                CarRegistrationNumber = "W789XYZ",
                IsVisitor = false
            };

            var visitorUser = new ApplicationUser
            {
                FirstName = "Guest/visitor",
                LastName = "1",
                CarRegistrationNumber = "N/A",
                IsVisitor = true
            };

            var applicationUsers = new[] { user, visitorUser };

            var mockUserManager = TestHelpers.CreateMockUserManager();
            mockUserManager
                .SetupGet(u => u.Users)
                .Returns(applicationUsers.AsQueryable());

            var model = new RegistrationNumbersModel(mockUserManager.Object);

            model.OnGet();

            var result = model.RegistrationNumberRecords;

            var expectedRecords = new[]
            {
                new RegistrationNumbersModel.RegistrationNumberRecord("Colm Wilkinson", "W789XYZ"),
            };

            CheckResult(expectedRecords, result);
        }

        private static void CheckResult(
            IReadOnlyList<RegistrationNumbersModel.RegistrationNumberRecord> expected,
            IReadOnlyList<RegistrationNumbersModel.RegistrationNumberRecord> actual)
        {
            Assert.NotNull(actual);

            Assert.Equal(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].FullName, actual[i].FullName);
                Assert.Equal(expected[i].CarRegistrationNumber, actual[i].CarRegistrationNumber);
            }
        }
    }
}