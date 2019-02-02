namespace ParkingRota.Business.Model
{
    using System.Collections.Generic;

    public interface IBankHolidayRepository
    {
        IReadOnlyList<BankHoliday> GetBankHolidays();

        void AddBankHolidays(IReadOnlyList<BankHoliday> newBankHolidays);
    }
}