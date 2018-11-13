namespace ParkingRota.Calendar
{
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;

    public class Calendar<T>
    {
        private Calendar(IReadOnlyList<Week<T>> weeks) => this.Weeks = weeks;

        public IReadOnlyList<Week<T>> Weeks { get; }

        public static Calendar<T> Create(IReadOnlyDictionary<LocalDate, T> data)
        {
            var weekStarts = data.Keys
                .Select(d => d.Next(IsoDayOfWeek.Monday).PlusDays(-7))
                .Distinct()
                .OrderBy(d => d);

            var weeks = weekStarts
                .Select(d => CalculateWeek(d, data))
                .ToArray();

            return new Calendar<T>(weeks);
        }

        private static Week<T> CalculateWeek(LocalDate firstDateOfWeek, IReadOnlyDictionary<LocalDate, T> data)
        {
            var dates = Enumerable
                .Range(0, 5)
                .Select(firstDateOfWeek.PlusDays);

            var days = dates
                .Select(d => new Day<T>(d, data))
                .ToArray();

            return new Week<T>(days);
        }
    }
}