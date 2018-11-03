namespace ParkingRota.Business.Model
{
    using System.Collections.Generic;
    using NodaTime;

    public interface IRequestRepository
    {
        IReadOnlyList<Request> GetRequests(LocalDate firstDate, LocalDate lastDate);
    }
}