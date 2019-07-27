namespace ParkingRota.Business.ScheduledTasks
{
    using System.Linq;
    using System.Threading.Tasks;
    using Model;
    using NodaTime;

    public class BankHolidayUpdater : IScheduledTask
    {
        private readonly IBankHolidayFetcher bankHolidayFetcher;
        private readonly IBankHolidayRepository bankHolidayRepository;

        public BankHolidayUpdater(
            IBankHolidayFetcher bankHolidayFetcher,
            IBankHolidayRepository bankHolidayRepository)
        {
            this.bankHolidayFetcher = bankHolidayFetcher;
            this.bankHolidayRepository = bankHolidayRepository;
        }

        public ScheduledTaskType ScheduledTaskType => ScheduledTaskType.BankHolidayUpdater;

        public async Task Run()
        {
            var existingDates = this.bankHolidayRepository.GetBankHolidays().Select(b => b.Date);
            var allDates = await this.bankHolidayFetcher.Fetch();

            var newBankHolidays = allDates
                .Except(existingDates)
                .Select(d => new BankHoliday { Date = d })
                .ToArray();

            this.bankHolidayRepository.AddBankHolidays(newBankHolidays);
        }

        public Instant GetNextRunTime(Instant currentInstant) =>
            currentInstant
                .InZone(DateCalculator.LondonTimeZone)
                .Date
                .Next(IsoDayOfWeek.Monday)
                .AtMidnight()
                .InZoneStrictly(DateCalculator.LondonTimeZone)
                .ToInstant();
    }
}