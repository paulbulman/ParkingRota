namespace ParkingRota.Business.Model
{
    using System.Collections.Generic;

    public interface IBankHolidayRepository
    {
        IReadOnlyList<BankHoliday> GetBankHolidays();
    }
}