namespace ParkingRota.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using Model;

    public class AllocationCreator
    {
        private readonly IRequestRepository requestRepository;
        private readonly IReservationRepository reservationRepository;
        private readonly IAllocationRepository allocationRepository;
        private readonly ISystemParameterListRepository systemParameterListRepository;
        private readonly IDateCalculator dateCalculator;
        private readonly ISingleDayAllocationCreator singleDayAllocationCreator;

        public AllocationCreator(
            IRequestRepository requestRepository,
            IReservationRepository reservationRepository,
            IAllocationRepository allocationRepository,
            ISystemParameterListRepository systemParameterListRepository,
            IDateCalculator dateCalculator,
            ISingleDayAllocationCreator singleDayAllocationCreator)
        {
            this.requestRepository = requestRepository;
            this.reservationRepository = reservationRepository;
            this.allocationRepository = allocationRepository;
            this.systemParameterListRepository = systemParameterListRepository;
            this.dateCalculator = dateCalculator;
            this.singleDayAllocationCreator = singleDayAllocationCreator;
        }

        public IReadOnlyList<Allocation> Create()
        {
            var shortLeadTimeAllocationDates = this.dateCalculator.GetShortLeadTimeAllocationDates();
            var longLeadTimeAllocationDates = this.dateCalculator.GetLongLeadTimeAllocationDates();

            var firstDate = shortLeadTimeAllocationDates.First().PlusDays(-60);
            var lastDate = longLeadTimeAllocationDates.Last();

            var requests = this.requestRepository.GetRequests(firstDate, lastDate);
            var reservations = this.reservationRepository.GetReservations(firstDate, lastDate);

            var allAllocations = this.allocationRepository.GetAllocations(firstDate, lastDate).ToList();

            var systemParameters = this.systemParameterListRepository.GetSystemParameterList();

            var newAllocationsToSave = new List<Allocation>();

            foreach (var allocationDate in shortLeadTimeAllocationDates)
            {
                var newAllocations = this.singleDayAllocationCreator.Create(
                    allocationDate, requests, reservations, allAllocations, systemParameters, shortLeadTime: true);

                allAllocations.AddRange(newAllocations);
                newAllocationsToSave.AddRange(newAllocations);
            }

            foreach (var allocationDate in longLeadTimeAllocationDates)
            {
                var newAllocations = this.singleDayAllocationCreator.Create(
                    allocationDate, requests, reservations, allAllocations, systemParameters, shortLeadTime: false);

                allAllocations.AddRange(newAllocations);
                newAllocationsToSave.AddRange(newAllocations);
            }

            this.allocationRepository.AddAllocations(newAllocationsToSave);

            return newAllocationsToSave;
        }
    }
}