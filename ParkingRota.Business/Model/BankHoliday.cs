namespace ParkingRota.Business.Model
{
    using NodaTime;

    public class BankHoliday
    {
        public int Id { get; set; }

        public LocalDate Date { get; set; }
    }
}