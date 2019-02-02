namespace ParkingRota.Business.Model
{
    public enum ScheduledTaskType
    {
        ReservationReminder = 0,
        RequestReminder = 1,
        DailySummary = 2,
        WeeklySummary = 3,
        BankHolidayUpdater = 4
    }
}