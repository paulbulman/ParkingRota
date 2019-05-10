namespace ParkingRota.UnitTests.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using Xunit;

    public class RequestSorterTests
    {
        private static readonly LocalDate AllocationDate = 24.February(2018);

        private readonly IReadOnlyList<ApplicationUser> users;

        private readonly IList<Request> requests;
        private readonly IList<Allocation> allocations;
        private readonly IList<Reservation> reservations;

        private readonly SystemParameterList systemParameter;

        public RequestSorterTests()
        {
            this.users = new[]
            {
                new ApplicationUser { Id = Guid.NewGuid().ToString(), CommuteDistance = 2 },
                new ApplicationUser { Id = Guid.NewGuid().ToString(), CommuteDistance = 4 },
                new ApplicationUser { Id = Guid.NewGuid().ToString(), CommuteDistance = 6 },
                new ApplicationUser { Id = Guid.NewGuid().ToString(), CommuteDistance = 8 },
                new ApplicationUser { Id = Guid.NewGuid().ToString(), CommuteDistance = 10 }
            };

            this.requests = new List<Request>();
            this.allocations = new List<Allocation>();
            this.reservations = new List<Reservation>();

            this.systemParameter = new SystemParameterList { NearbyDistance = 4 };
        }

        private IEnumerable<ApplicationUser> NearbyUsers =>
            this.users.Where(u => u.CommuteDistance <= this.systemParameter.NearbyDistance);

        private IEnumerable<ApplicationUser> FarAwayUsers =>
            this.users.Where(u => u.CommuteDistance > this.systemParameter.NearbyDistance);

        [Fact]
        public void Test_Sort_ReturnsReservationsFirst()
        {
            var nearbyUser = this.NearbyUsers.First();
            var farAwayUser = this.FarAwayUsers.First();

            this.AddRequest(nearbyUser, AllocationDate.PlusDays(-1), allocated: true);
            this.AddRequest(nearbyUser, AllocationDate.PlusDays(-2), allocated: true);

            this.AddRequest(farAwayUser, AllocationDate.PlusDays(-1));
            this.AddRequest(farAwayUser, AllocationDate.PlusDays(-2));

            var lowerPriorityRequest = this.AddRequest(farAwayUser);
            var higherPriorityRequest = this.AddRequest(nearbyUser, reserved: true);

            this.CheckSortOrder(new[] { higherPriorityRequest, lowerPriorityRequest });
        }

        [Fact]
        public void Test_Sort_IgnoresReservationsForOtherDays()
        {
            var nearbyUser = this.NearbyUsers.First();
            var farAwayUser = this.FarAwayUsers.First();

            this.AddRequest(nearbyUser, AllocationDate.PlusDays(-1), allocated: true, reserved: true);
            this.AddRequest(nearbyUser, AllocationDate.PlusDays(-2), allocated: true, reserved: true);

            this.AddRequest(farAwayUser, AllocationDate.PlusDays(-1));
            this.AddRequest(farAwayUser, AllocationDate.PlusDays(-2));

            var lowerPriorityRequest = this.AddRequest(nearbyUser);
            var higherPriorityRequest = this.AddRequest(farAwayUser);

            this.CheckSortOrder(new[] { higherPriorityRequest, lowerPriorityRequest });
        }

        [Fact]
        public void Test_Sort_IgnoresFutureData()
        {
            var nearbyUser = this.NearbyUsers.First();
            var otherNearbyUser = this.NearbyUsers.Skip(1).First();

            this.AddRequest(nearbyUser, AllocationDate.PlusDays(-1), allocated: true);

            this.AddRequest(nearbyUser, AllocationDate.PlusDays(-2));
            this.AddRequest(otherNearbyUser, AllocationDate.PlusDays(-2));

            this.AddRequest(otherNearbyUser, AllocationDate.PlusDays(1), allocated: true);
            this.AddRequest(otherNearbyUser, AllocationDate.PlusDays(2), allocated: true);

            var lowerPriorityRequest = this.AddRequest(nearbyUser);
            var higherPriorityRequest = this.AddRequest(otherNearbyUser);

            this.CheckSortOrder(new[] { higherPriorityRequest, lowerPriorityRequest });
        }

        [Fact]
        public void Test_Sort_ReturnsNearbyLast()
        {
            var nearbyUser = this.NearbyUsers.First();
            var farAwayUser = this.FarAwayUsers.First();

            this.AddRequest(nearbyUser, AllocationDate.PlusDays(-1));
            this.AddRequest(nearbyUser, AllocationDate.PlusDays(-2));

            this.AddRequest(farAwayUser, AllocationDate.PlusDays(-1), allocated: true);
            this.AddRequest(farAwayUser, AllocationDate.PlusDays(-2), allocated: true);

            var lowerPriorityRequest = this.AddRequest(nearbyUser);
            var higherPriorityRequest = this.AddRequest(farAwayUser);

            this.CheckSortOrder(new[] { higherPriorityRequest, lowerPriorityRequest });
        }

        [Fact]
        public void Test_Sort_TreatsNoDistanceAsFarAway()
        {
            var nearbyUser = this.NearbyUsers.First();
            var farAwayUser = this.FarAwayUsers.First();

            farAwayUser.CommuteDistance = null;

            this.AddRequest(nearbyUser, AllocationDate.PlusDays(-1));
            this.AddRequest(nearbyUser, AllocationDate.PlusDays(-2));

            this.AddRequest(farAwayUser, AllocationDate.PlusDays(-1), allocated: true);
            this.AddRequest(farAwayUser, AllocationDate.PlusDays(-2), allocated: true);

            var lowerPriorityRequest = this.AddRequest(nearbyUser);
            var higherPriorityRequest = this.AddRequest(farAwayUser);

            this.CheckSortOrder(new[] { higherPriorityRequest, lowerPriorityRequest });
        }

        [Fact]
        public void Test_Sort_ReturnsMostInterruptedFirst()
        {
            var lessInterruptedUser = this.NearbyUsers.First();
            var moreInterruptedUser = this.NearbyUsers.Skip(1).First();

            this.AddRequest(lessInterruptedUser, date: AllocationDate.PlusDays(-1), allocated: true);
            this.AddRequest(moreInterruptedUser, date: AllocationDate.PlusDays(-1));

            var lowerPriorityRequest = this.AddRequest(lessInterruptedUser);
            var higherPriorityRequest = this.AddRequest(moreInterruptedUser);

            this.CheckSortOrder(new[] { higherPriorityRequest, lowerPriorityRequest });
        }

        [Fact]
        public void Test_Sort_ReservationsDoNotAffectInterruptedRatio()
        {
            var lessInterruptedUser = this.NearbyUsers.First();
            var moreInterruptedUser = this.NearbyUsers.Skip(1).First();

            this.AddRequest(lessInterruptedUser, date: AllocationDate.PlusDays(-2));
            this.AddRequest(moreInterruptedUser, date: AllocationDate.PlusDays(-2));

            this.AddRequest(lessInterruptedUser, date: AllocationDate.PlusDays(-1), allocated: true);
            this.AddRequest(moreInterruptedUser, date: AllocationDate.PlusDays(-1), allocated: true, reserved: true);

            var lowerPriorityRequest = this.AddRequest(lessInterruptedUser);
            var higherPriorityRequest = this.AddRequest(moreInterruptedUser);

            this.CheckSortOrder(new[] { higherPriorityRequest, lowerPriorityRequest });
        }

        [Fact]
        public void Test_Sort_ReturnsUsersWithNoPreviousRequests()
        {
            var lessInterruptedUser = this.FarAwayUsers.First();
            var moreInterruptedUser = this.FarAwayUsers.Skip(1).First();
            var userWithNoRequests = this.FarAwayUsers.Skip(2).First();

            this.AddRequest(lessInterruptedUser, date: AllocationDate.PlusDays(-3));
            this.AddRequest(lessInterruptedUser, date: AllocationDate.PlusDays(-2), allocated: true);
            this.AddRequest(lessInterruptedUser, date: AllocationDate.PlusDays(-1), allocated: true);

            this.AddRequest(moreInterruptedUser, date: AllocationDate.PlusDays(-3));
            this.AddRequest(moreInterruptedUser, date: AllocationDate.PlusDays(-2));
            this.AddRequest(moreInterruptedUser, date: AllocationDate.PlusDays(-1), allocated: true);

            var lowerPriorityRequest = this.AddRequest(lessInterruptedUser);
            var higherPriorityRequest = this.AddRequest(moreInterruptedUser);

            var newRequest = this.AddRequest(userWithNoRequests);

            // Users with no previous requests should end up somewhere in the middle of the sort order at random.
            // In this case that must therefore be in position 2 out of { 1, 2, 3 }.
            this.CheckSortOrder(new[] { higherPriorityRequest, newRequest, lowerPriorityRequest });
        }

        [Fact]
        public void Test_Sort_ExcludesAlreadyAllocatedOnSameDay()
        {
            this.AddRequest(this.users[0], allocated: true);

            var unallocatedRequest = this.AddRequest(this.users[1]);

            this.CheckSortOrder(new[] { unallocatedRequest });
        }

        [Fact]
        public void Test_Sort_IncludesAlreadyAllocatedOnDifferentDay()
        {
            var user = this.users[0];

            this.AddRequest(user, date: AllocationDate.PlusDays(-1), allocated: true);
            var unallocatedRequest = this.AddRequest(user);

            this.CheckSortOrder(new[] { unallocatedRequest });
        }

        [Fact]
        public void Test_Sort_ExcludesOtherDates()
        {
            var user = this.users[0];

            this.AddRequest(user, AllocationDate.PlusDays(-1));
            this.AddRequest(user, AllocationDate.PlusDays(-2));

            this.CheckSortOrder(new Request[0]);
        }

        private Request AddRequest(
            ApplicationUser user,
            LocalDate? date = null,
            bool allocated = false,
            bool reserved = false)
        {
            var request = new Request { ApplicationUser = user, Date = date ?? AllocationDate };

            this.requests.Add(request);

            if (allocated)
            {
                this.allocations.Add(new Allocation { ApplicationUser = user, Date = date ?? AllocationDate });
            }

            if (reserved)
            {
                this.reservations.Add(new Reservation { ApplicationUser = user, Date = date ?? AllocationDate });
            }

            return request;
        }

        private void CheckSortOrder(IReadOnlyList<Request> expectedOrder)
        {
            var result = new RequestSorter().Sort(
                AllocationDate,
                this.requests.ToArray(),
                this.allocations.ToArray(),
                this.reservations.ToArray(),
                this.systemParameter);

            Assert.Equal(expectedOrder, result);
        }
    }
}