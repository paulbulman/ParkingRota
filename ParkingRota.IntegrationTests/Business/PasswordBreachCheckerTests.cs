namespace ParkingRota.IntegrationTests.Business
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using ParkingRota.Business;
    using Xunit;

    public class PasswordBreachCheckerTests
    {
        [Theory]
        [InlineData("Password", true)]
        [InlineData("LetMeIn", true)]
        [InlineData("565B1390-35FC-4B7D-BF2D-B328A67CAEF3", false)]
        public async Task Test_PasswordIsBreached(string password, bool expectIsBreached)
        {
            using (var client = new HttpClient())
            {
                var checker = new PasswordBreachChecker(null, client);

                var isBreached = await checker.PasswordIsBreached(password);

                Assert.Equal(expectIsBreached, isBreached);
            }
        }
    }
}