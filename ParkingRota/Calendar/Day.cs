namespace ParkingRota.Calendar
{
    using NodaTime;

    public class Day
    {
        internal Day(LocalDate date, bool isActive)
        {
            this.Date = date;
            this.IsActive = isActive;
        }

        public LocalDate Date { get; }

        public bool IsActive { get; }
    }
}