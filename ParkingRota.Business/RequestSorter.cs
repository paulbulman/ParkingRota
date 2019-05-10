namespace ParkingRota.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Model;
    using NodaTime;

    public interface IRequestSorter
    {
        IReadOnlyList<Request> Sort(
            LocalDate date,
            IReadOnlyList<Request> requests,
            IReadOnlyList<Allocation> existingAllocations,
            IReadOnlyList<Reservation> reservations,
            SystemParameterList systemParameterList);
    }

    public class RequestSorter : IRequestSorter
    {
        public IReadOnlyList<Request> Sort(
            LocalDate date,
            IReadOnlyList<Request> requests,
            IReadOnlyList<Allocation> existingAllocations,
            IReadOnlyList<Reservation> reservations,
            SystemParameterList systemParameterList)
        {
            var requestsToSort = requests
                .Where(r => r.Date == date && IsNotAlreadyAllocated(r, existingAllocations))
                .ToArray();

            var exisingAllocationsRatios = requestsToSort.ToDictionary(
                r => r,
                r => CalculateExistingAllocationRatio(r, requests, existingAllocations, reservations));

            RandomiseMissingAllocationRatios(exisingAllocationsRatios);

            return requestsToSort
                .OrderBy(r => HasReservation(r, reservations))
                .ThenBy(r => LivesFarAway(r, systemParameterList))
                .ThenBy(r => exisingAllocationsRatios[r])
                .ToArray();
        }

        private static void RandomiseMissingAllocationRatios(IDictionary<Request, decimal?> exisingAllocationsRatios)
        {
            var random = new Random();

            var minExistingAllocationRatio = exisingAllocationsRatios.Select(r => r.Value).Min() ?? 0;
            var maxExistingAllocationRatio = exisingAllocationsRatios.Select(r => r.Value).Max() ?? 1;

            // Random.Next(minValue, maxValue) is inclusive on minValue, but we want exclusive,
            // i.e. the new values are strictly between the existing minimum and maximum.
            minExistingAllocationRatio = Math.Min(minExistingAllocationRatio + 0.01m, maxExistingAllocationRatio);

            var missingValues = exisingAllocationsRatios.Where(r => r.Value == null).ToArray();

            foreach (var exisingAllocationsRatio in missingValues)
            {
                var randomPercentage = random.Next(
                    (int)(minExistingAllocationRatio * 100),
                    (int)(maxExistingAllocationRatio * 100));

                var randomRatio = (decimal)randomPercentage / 100;

                exisingAllocationsRatios[exisingAllocationsRatio.Key] = randomRatio;
            }
        }

        private static int HasReservation(Request request, IReadOnlyList<Reservation> reservations) =>
            reservations.Any(r => r.ApplicationUser.Id == request.ApplicationUser.Id && r.Date == request.Date) ? 0 : 1;

        private static bool IsNotAlreadyAllocated(Request request, IReadOnlyList<Allocation> allocations) =>
            !allocations.Any(a => a.ApplicationUser.Id == request.ApplicationUser.Id && a.Date == request.Date);

        private static int LivesFarAway(Request request, SystemParameterList systemParameterList) =>
            request.ApplicationUser.CommuteDistance == null ||
            request.ApplicationUser.CommuteDistance > systemParameterList.NearbyDistance ? 0 : 1;

        private static decimal? CalculateExistingAllocationRatio(
            Request request,
            IReadOnlyList<Request> requests,
            IReadOnlyList<Allocation> allocations,
            IReadOnlyList<Reservation> reservations)
        {
            var earlierRequestCount = requests
                .Count(r =>
                    r.ApplicationUser.Id == request.ApplicationUser.Id &&
                    r.Date < request.Date &&
                    !ReservationExists(reservations, r.ApplicationUser.Id, r.Date));

            var earlierAllocationCount = allocations
                .Count(a =>
                    a.ApplicationUser.Id == request.ApplicationUser.Id &&
                    a.Date < request.Date &&
                    !ReservationExists(reservations, a.ApplicationUser.Id, a.Date));

            return earlierRequestCount == 0 ? (decimal?)null : (decimal)earlierAllocationCount / earlierRequestCount;
        }

        private static bool ReservationExists(IReadOnlyList<Reservation> reservations, string applicationUserId, LocalDate date) =>
            reservations.Any(r => r.ApplicationUser.Id == applicationUserId && r.Date == date);
    }
}