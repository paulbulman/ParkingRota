namespace ParkingRota.UnitTests.Business.Model
{
    using ParkingRota.Business.Model;
    using Xunit;

    public static class ApplicationUserTests
    {
        [Theory]
        [InlineData("Jack", "Johnson", "Jack Johnson")]
        [InlineData("Sinéad", "O'Connor", "Sinéad O'Connor")]
        public static void Test_FullName(string firstName, string lastName, string expectedFullName)
        {
            var applicationUser = new ApplicationUser { FirstName = firstName, LastName = lastName };

            Assert.Equal(expectedFullName, applicationUser.FullName);
        }
    }
}