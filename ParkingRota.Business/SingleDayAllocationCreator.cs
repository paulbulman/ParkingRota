namespace ParkingRota.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Model;
    using NodaTime;

    public interface ISingleDayAllocationCreator
    {
        IReadOnlyList<Allocation> Create(
            LocalDate date,
            IReadOnlyList<Request> requests,
            IReadOnlyList<Reservation> reservations,
            IReadOnlyList<Allocation> existingAllocations,
            SystemParameterList systemParameterList,
            bool shortLeadTime);
    }

    public class SingleDayAllocationCreator : ISingleDayAllocationCreator
    {
        private readonly IRequestSorter sorter;

        public SingleDayAllocationCreator(IRequestSorter sorter) => this.sorter = sorter;

        public IReadOnlyList<Allocation> Create(
            LocalDate date,
            IReadOnlyList<Request> requests,
            IReadOnlyList<Reservation> reservations,
            IReadOnlyList<Allocation> existingAllocations,
            SystemParameterList systemParameterList,
            bool shortLeadTime)
        {
            var spacesToReserve = shortLeadTime ? 0 : systemParameterList.ReservableSpaces;

            var freeSpaces = systemParameterList.TotalSpaces - spacesToReserve - existingAllocations.Count(a => a.Date == date);

            var sortedRequests = this.sorter
                .Sort(date, requests, existingAllocations, reservations, systemParameterList)
                .ToArray();

            var requestsToAllocate = sortedRequests.Take(Math.Min(freeSpaces, sortedRequests.Length));

            return requestsToAllocate
                .Select(r => new Allocation
                {
                    ApplicationUser = r.ApplicationUser,
                    Date = r.Date
                })
                .ToArray();
        }
    }
}