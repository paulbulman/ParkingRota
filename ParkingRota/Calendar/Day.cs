namespace ParkingRota.Calendar
{
    using System.Collections.Generic;
    using NodaTime;

    public class Day<T>
    {
        private readonly IReadOnlyDictionary<LocalDate, T> data;

        internal Day(LocalDate date, IReadOnlyDictionary<LocalDate, T> data)
        {
            this.data = data;
            this.Date = date;
        }

        public LocalDate Date { get; }

        public bool IsActive => this.data.ContainsKey(this.Date);

        public T Data => this.data.ContainsKey(this.Date) ? this.data[this.Date] : default(T);
    }
}