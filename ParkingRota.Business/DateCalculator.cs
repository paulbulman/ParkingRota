namespace ParkingRota.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using Model;
    using NodaTime;

    public interface IDateCalculator
    {
        Instant CurrentInstant { get; }

        DateTimeZone TimeZone { get; }

        IReadOnlyList<LocalDate> GetActiveDates();

        IReadOnlyList<LocalDate> GetShortLeadTimeAllocationDates();

        IReadOnlyList<LocalDate> GetLongLeadTimeAllocationDates();

        LocalDate GetCurrentDate();

        LocalDate GetNextWorkingDate();

        LocalDate GetPreviousWorkingDate(LocalDate localDate);

        IReadOnlyList<LocalDate> GetUpcomingLongLeadTimeAllocationDates();
    }

    public class DateCalculator : IDateCalculator
    {
        private readonly IBankHolidayRepository bankHolidayRepository;

        public DateCalculator(IClock clock, IBankHolidayRepository bankHolidayRepository)
        {
            this.bankHolidayRepository = bankHolidayRepository;
            this.CurrentInstant = clock.GetCurrentInstant();
        }

        public Instant CurrentInstant { get; }

        public DateTimeZone TimeZone => DateTimeZoneProviders.Tzdb["Europe/London"];

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
            var firstDate = this.GetNextWorkingDayIncluding(currentTime.Date);

            var lastDate = firstDate;

            if (firstDate == currentTime.Date && currentTime.Hour >= 11)
            {
                lastDate = lastDate.PlusDays(1);
            }

            lastDate = this.GetNextWorkingDayIncluding(lastDate);

            return new[] { firstDate, lastDate }.Distinct().ToList();
        }

        public IReadOnlyList<LocalDate> GetLongLeadTimeAllocationDates()
        {
            var currentTime = this.GetCurrentTime();

            var lastShortLeadTimeAllocationDate = this.GetShortLeadTimeAllocationDates(currentTime).Last();

            var firstDate = this.GetNextWorkingDayAfter(lastShortLeadTimeAllocationDate);

            var lastDate = GetLastLongLeadTimeAllocationDate(currentTime.Date);

            return this.DatesBetween(firstDate, lastDate);
        }

        public LocalDate GetCurrentDate() => this.GetCurrentTime().Date;

        public LocalDate GetNextWorkingDate() => this.GetNextWorkingDayAfter(this.GetCurrentDate());

        public LocalDate GetPreviousWorkingDate(LocalDate localDate)
        {
            localDate = localDate.PlusDays(-1);

            while (!this.IsWorkingDay(localDate))
            {
                localDate = localDate.PlusDays(-1);
            }

            return localDate;
        }

        public IReadOnlyList<LocalDate> GetUpcomingLongLeadTimeAllocationDates()
        {
            var lastDate = GetLastLongLeadTimeAllocationDate(this.GetNextWorkingDate());
            var firstDate = lastDate.PlusDays(-4);

            return this.DatesBetween(firstDate, lastDate);
        }

        private ZonedDateTime GetCurrentTime() => this.CurrentInstant.InZone(this.TimeZone);

        private LocalDate GetNextWorkingDayAfter(LocalDate localDate) =>
            this.GetNextWorkingDayIncluding(localDate.PlusDays(1));

        private LocalDate GetNextWorkingDayIncluding(LocalDate localDate)
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

        private static LocalDate GetLastLongLeadTimeAllocationDate(LocalDate localDate) =>
            localDate.Next(IsoDayOfWeek.Thursday).PlusWeeks(1).PlusDays(1);
    }
}