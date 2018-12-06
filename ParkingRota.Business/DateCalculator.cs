namespace ParkingRota.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using Model;
    using NodaTime;

    public interface IDateCalculator
    {
        IReadOnlyList<LocalDate> GetActiveDates();

        IReadOnlyList<LocalDate> GetShortLeadTimeAllocationDates();

        IReadOnlyList<LocalDate> GetLongLeadTimeAllocationDates();
    }

    public class DateCalculator : IDateCalculator
    {
        private readonly IBankHolidayRepository bankHolidayRepository;

        private readonly Instant currentInstant;

        public DateCalculator(IClock clock, IBankHolidayRepository bankHolidayRepository)
        {
            this.bankHolidayRepository = bankHolidayRepository;
            this.currentInstant = clock.GetCurrentInstant();
        }

        public IReadOnlyList<LocalDate> GetActiveDates()
        {
            var currentDate = this.GetCurrentDate();

            var firstDayOfThisMonth = new LocalDate(currentDate.Year, currentDate.Month, 1);
            var firstDayOfSubsequentMonth = firstDayOfThisMonth.PlusMonths(2);
            var lastDayOfNextMonth = firstDayOfSubsequentMonth.PlusDays(-1);

            return this.DatesBetween(currentDate, lastDayOfNextMonth);
        }

        public IReadOnlyList<LocalDate> GetShortLeadTimeAllocationDates() =>
            this.GetShortLeadTimeAllocationDates(this.GetCurrentTime());

        private IReadOnlyList<LocalDate> GetShortLeadTimeAllocationDates(ZonedDateTime currentTime)
        {
            var firstDate = this.GetWorkingDay(currentTime.Date);

            var lastDate = firstDate;

            if (firstDate == currentTime.Date && currentTime.Hour >= 11)
            {
                lastDate = lastDate.PlusDays(1);
            }

            lastDate = this.GetWorkingDay(lastDate);

            return new[] { firstDate, lastDate }.Distinct().ToList();
        }

        public IReadOnlyList<LocalDate> GetLongLeadTimeAllocationDates()
        {
            var currentTime = this.GetCurrentTime();

            var lastShortLeadTimeAllocationDate = this.GetShortLeadTimeAllocationDates(currentTime).Last();

            var firstDate = this.GetWorkingDay(lastShortLeadTimeAllocationDate.PlusDays(1));

            var lastDate = GetLastLongLeadTimeAllocationDate(currentTime.Date);

            return this.DatesBetween(firstDate, lastDate);
        }

        private LocalDate GetCurrentDate() => this.GetCurrentTime().Date;

        private ZonedDateTime GetCurrentTime() =>
            this.currentInstant.InZone(DateTimeZoneProviders.Tzdb["Europe/London"]);

        private LocalDate GetWorkingDay(LocalDate localDate)
        {
            while (!this.IsWorkingDay(localDate))
            {
                localDate = localDate.PlusDays(1);
            }

            return localDate;
        }

        private IReadOnlyList<LocalDate> DatesBetween(LocalDate firstDate, LocalDate lastDate) =>
            Enumerable.Range(0, Period.Between(firstDate, lastDate, PeriodUnits.Days).Days + 1)
                .Select(firstDate.PlusDays)
                .Where(this.IsWorkingDay)
                .ToArray();

        private bool IsWorkingDay(LocalDate date) =>
            date.DayOfWeek != IsoDayOfWeek.Saturday &&
            date.DayOfWeek != IsoDayOfWeek.Sunday &&
            this.bankHolidayRepository.BankHolidays.All(b => b.Date != date);

        private static LocalDate GetLastLongLeadTimeAllocationDate(LocalDate currentDate) =>
            currentDate.Next(IsoDayOfWeek.Thursday).PlusWeeks(1).PlusDays(1);
    }
}