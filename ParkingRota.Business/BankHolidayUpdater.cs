namespace ParkingRota.Business
{
    using System.Linq;
    using System.Threading.Tasks;
    using Model;

    public class BankHolidayUpdater
    {
        private readonly IBankHolidayFetcher bankHolidayFetcher;
        private readonly IBankHolidayRepository bankHolidayRepository;

        public BankHolidayUpdater(IBankHolidayFetcher bankHolidayFetcher, IBankHolidayRepository bankHolidayRepository)
        {
            this.bankHolidayFetcher = bankHolidayFetcher;
            this.bankHolidayRepository = bankHolidayRepository;
        }

        public async Task Update()
        {
            var existingDates = this.bankHolidayRepository.GetBankHolidays().Select(b => b.Date);
            var allDates = await this.bankHolidayFetcher.Fetch();

            var newBankHolidays = allDates
                .Except(existingDates)
                .Select(d => new BankHoliday { Date = d })
                .ToArray();

            this.bankHolidayRepository.AddBankHolidays(newBankHolidays);
        }
    }
}