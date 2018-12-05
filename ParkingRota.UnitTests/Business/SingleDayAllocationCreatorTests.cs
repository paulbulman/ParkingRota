using System.Collections.Generic;

namespace ParkingRota.UnitTests.Business
{
    using System;
    using System.Linq;
    using Moq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using Xunit;

    public class SingleDayAllocationCreatorTests
    {
        public static IEnumerable<object[]> AllocateData
        {
            get
            {
                // Long lead time, some (but not all) requests can be allocated
                yield return new object[]
                {
                    new TestData
                    {
                        ShortLeadTime = false,

                        TotalUsers = 5,
                        UsersWithReservations = new List<int>(),

                        TotalNewRequests = 4,
                        SortOrder = new[] { 1, 3, 0, 2 },

                        TotalSpaces = 4,
                        ReservableSpaces = 1,
                        TotalAlreadyAllocatedRequests = 1,

                        TotalExpectedAllocations = 2
                    }
                };

                // Long lead time, all requests can be allocated
                yield return new object[]
                {
                    new TestData
                    {
                        ShortLeadTime = false,

                        TotalUsers = 5,
                        UsersWithReservations = new List<int>(),

                        TotalNewRequests = 4,
                        SortOrder = new[] { 1, 3, 0, 2 },

                        TotalSpaces = 10,
                        ReservableSpaces = 1,
                        TotalAlreadyAllocatedRequests = 1,

                        TotalExpectedAllocations = 4
                    }
                };

                // Long lead time, no requests can be allocated
                yield return new object[]
                {
                    new TestData
                    {
                        ShortLeadTime = false,

                        TotalUsers = 5,
                        UsersWithReservations = new List<int>(),

                        TotalNewRequests = 4,
                        SortOrder = new[] { 1, 3, 0, 2 },

                        TotalSpaces = 2,
                        ReservableSpaces = 1,
                        TotalAlreadyAllocatedRequests = 1,

                        TotalExpectedAllocations = 0
                    }
                };

                // Day ahead, some (but not all) requests can be allocated
                yield return new object[]
                {
                    new TestData
                    {
                        ShortLeadTime = true,

                        TotalUsers = 6,
                        UsersWithReservations = new[] { 3, 5 },

                        TotalNewRequests = 4,
                        SortOrder = new[] { 3, 1, 0, 2 },

                        TotalSpaces = 4,
                        ReservableSpaces = 2,
                        TotalAlreadyAllocatedRequests = 2,

                        TotalExpectedAllocations = 2
                    }
                };

                // Day ahead, all requests can be allocated by using previously-reserved spaces
                yield return new object[]
                {
                    new TestData
                    {
                        ShortLeadTime = true,

                        TotalUsers = 6,
                        UsersWithReservations = new[] { 3 },

                        TotalNewRequests = 4,
                        SortOrder = new[] { 3, 1, 0, 2 },

                        TotalSpaces = 6,
                        ReservableSpaces = 2,
                        TotalAlreadyAllocatedRequests = 2,

                        TotalExpectedAllocations = 4
                    }
                };

                // Day ahead, no requests can be allocated
                yield return new object[]
                {
                    new TestData
                    {
                        ShortLeadTime = true,

                        TotalUsers = 4,
                        UsersWithReservations = new[] { 3, 1 },

                        TotalNewRequests = 2,
                        SortOrder = new[] { 1, 0 },

                        TotalSpaces = 2,
                        ReservableSpaces = 2,
                        TotalAlreadyAllocatedRequests = 2,

                        TotalExpectedAllocations = 0
                    }
                };
            }
        }

        [Theory, MemberData(nameof(AllocateData))]
        public static void Test_Create(TestData testData)
        {
            var allocationDate = 26.February(2018);

            var otherUsers = new[]
            {
                new ApplicationUser { Id = Guid.NewGuid().ToString() },
                new ApplicationUser { Id = Guid.NewGuid().ToString() }
            };

            var otherDateRequests = new[]
            {
                new Request { Date = 25.February(2018), ApplicationUser = otherUsers[0] },
                new Request { Date = 27.February(2018), ApplicationUser = otherUsers[1] }
            };

            var otherDateReservations = new[]
            {
                new Reservation { Date = 25.February(2018), ApplicationUser = otherUsers[0] },
                new Reservation { Date = 27.February(2018), ApplicationUser = otherUsers[1] }
            };

            var otherDateAllocations = new[]
            {
                new Allocation { Date = 25.February(2018), ApplicationUser = otherUsers[0] },
                new Allocation { Date = 27.February(2018), ApplicationUser = otherUsers[1] }
            };

            // Arrange
            var users = Enumerable
                .Range(0, testData.TotalUsers)
                .Select(x => new ApplicationUser { Id = Guid.NewGuid().ToString() })
                .ToArray();

            var newRequests = Enumerable
                .Range(0, testData.TotalNewRequests)
                .Select(x => new Request { ApplicationUser = users[x], Date = allocationDate })
                .ToArray();

            var alreadyAllocatedRequests = Enumerable
                .Range(testData.TotalNewRequests, testData.TotalAlreadyAllocatedRequests)
                .Select(x => new Request { ApplicationUser = users[x], Date = allocationDate })
                .ToArray();

            var existingAllocations = Enumerable
                .Range(testData.TotalNewRequests, testData.TotalAlreadyAllocatedRequests)
                .Select(x => new Allocation { ApplicationUser = users[x], Date = allocationDate })
                .Concat(otherDateAllocations)
                .ToArray();

            var allRequests = newRequests
                .Concat(alreadyAllocatedRequests)
                .Concat(otherDateRequests)
                .ToArray();

            var reservations = testData.UsersWithReservations
                .Select(i => new Reservation { ApplicationUser = users[i], Date = allocationDate })
                .Concat(otherDateReservations)
                .ToArray();

            var systemParameter = new SystemParameterList
            {
                ReservableSpaces = testData.ReservableSpaces,
                TotalSpaces = testData.TotalSpaces
            };

            var mockSorter = new Mock<IRequestSorter>(MockBehavior.Strict);

            var sortedRequests = testData.SortOrder.Select(o => newRequests[o]).ToArray();

            mockSorter
                .Setup(s => s.Sort(allocationDate, allRequests, existingAllocations, reservations, systemParameter))
                .Returns(sortedRequests);

            var expectedAllocatedRequests = sortedRequests
                .Take(testData.TotalExpectedAllocations)
                .ToArray();

            // Act
            var singleDayAllocationCreator = new SingleDayAllocationCreator(mockSorter.Object);

            var result = singleDayAllocationCreator.Create(
                allocationDate, allRequests, reservations, existingAllocations, systemParameter, testData.ShortLeadTime);

            // Assert
            Assert.Equal(expectedAllocatedRequests.Length, result.Count);

            foreach (var expectedAllocatedRequest in expectedAllocatedRequests)
            {
                Assert.NotNull(result.SingleOrDefault(a =>
                    a.ApplicationUser == expectedAllocatedRequest.ApplicationUser &&
                    a.Date == expectedAllocatedRequest.Date));
            }
        }

        public class TestData
        {
            public bool ShortLeadTime { get; set; }

            public int TotalUsers { get; set; }

            public int TotalNewRequests { get; set; }

            public int TotalAlreadyAllocatedRequests { get; set; }

            public int ReservableSpaces { get; set; }

            public int TotalSpaces { get; set; }

            public IReadOnlyList<int> UsersWithReservations { get; set; }

            public IReadOnlyList<int> SortOrder { get; set; }

            public int TotalExpectedAllocations { get; set; }
        }
    }
}