namespace ParkingRota.Business.Model
{
    using System.Collections.Generic;
    using NodaTime;

    public interface IAllocationRepository
    {
        IReadOnlyList<Allocation> GetAllocations(LocalDate firstDate, LocalDate lastDate);

        void AddAllocations(IReadOnlyList<Allocation> allocations);
    }
}