namespace ParkingRota.IntegrationTests.Business
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using Xunit;

    public class BankHolidayFetcherTests
    {
        [Fact]
        public async Task Test_EnglishBankHolidayIsReturned()
        {
            using (var client = new HttpClient())
            {
                var fetcher = new BankHolidayFetcher(client);

                var result = await fetcher.Fetch();

                Assert.Contains(25.December(2018), result);
            }
        }

        [Fact]
        public async Task Test_NorthernIrishBankHolidayIsNotReturned()
        {
            using (var client = new HttpClient())
            {
                var fetcher = new BankHolidayFetcher(client);

                var result = await fetcher.Fetch();

                Assert.DoesNotContain(12.July(2018), result);
            }
        }
    }
}