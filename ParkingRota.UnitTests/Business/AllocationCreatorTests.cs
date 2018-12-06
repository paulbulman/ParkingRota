namespace ParkingRota.UnitTests.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Moq;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using Xunit;

    public static class AllocationCreatorTests
    {
        [Fact]
        public static void Test_Create()
        {
            // Arrange
            var shortLeadTimeAllocationDates = new[] { 27.February(2018), 28.February(2018) };
            var longLeadTimeAllocationDates = new[] { 1.March(2018), 2.March(2018) };

            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .Setup(d => d.GetShortLeadTimeAllocationDates())
                .Returns(shortLeadTimeAllocationDates);
            mockDateCalculator
                .Setup(d => d.GetLongLeadTimeAllocationDates())
                .Returns(longLeadTimeAllocationDates);

            var firstDate = 29.December(2017); // 60 days before first short lead time allocation date
            var lastDate = 2.March(2018);

            var requests = new[]
            {
                new Request { Date = 27.February(2018) },
                new Request { Date = 27.February(2018) },
                new Request { Date = 3.January(2018) },
                new Request { Date = 1.March(2018) }
            };

            var mockRequestRepository = new Mock<IRequestRepository>(MockBehavior.Strict);
            mockRequestRepository
                .Setup(r => r.GetRequests(firstDate, lastDate))
                .Returns(requests);

            var reservations = new[]
            {
                new Reservation { Date = 27.February(2018) },
                new Reservation { Date = 27.February(2018) },
                new Reservation { Date = 22.February(2018) },
                new Reservation { Date = 13.January(2018) }
            };

            var mockReservationRepository = new Mock<IReservationRepository>(MockBehavior.Strict);
            mockReservationRepository
                .Setup(r => r.GetReservations(firstDate, lastDate))
                .Returns(reservations);

            var originalExistingAllocations = new[]
            {
                new Allocation { Date = 27.February(2018) },
                new Allocation { Date = 27.February(2018) },
                new Allocation { Date = 31.December(2017) }
            };

            var mockAllocationRepository = new Mock<IAllocationRepository>(MockBehavior.Strict);
            mockAllocationRepository
                .Setup(a => a.GetAllocations(firstDate, lastDate))
                .Returns(originalExistingAllocations);

            var systemParameterList = new SystemParameterList();

            var mockSystemParameterListRepository = new Mock<ISystemParameterListRepository>(MockBehavior.Strict);
            mockSystemParameterListRepository.Setup(s => s.GetSystemParameterList()).Returns(systemParameterList);

            var newAllocations = shortLeadTimeAllocationDates
                .Concat(longLeadTimeAllocationDates)
                .SelectMany(d => new[] { new Allocation { Date = d }, new Allocation { Date = d } })
                .ToArray();

            var mockSingleDayAllocationCreator = new Mock<ISingleDayAllocationCreator>(MockBehavior.Strict);

            foreach (var date in shortLeadTimeAllocationDates.Concat(longLeadTimeAllocationDates))
            {
                mockSingleDayAllocationCreator
                    .Setup(c => c.Create(
                        date,
                        requests,
                        reservations,
                        It.Is(ExpectedExistingAllocations(originalExistingAllocations, newAllocations, date)),
                        systemParameterList,
                        shortLeadTimeAllocationDates.Contains(date)))
                    .Returns(newAllocations.Where(a => a.Date == date).ToArray());
            }

            mockAllocationRepository
                .Setup(a => a.AddAllocations(It.IsAny<IReadOnlyList<Allocation>>()));

            // Act
            var result = new AllocationCreator(
                mockRequestRepository.Object,
                mockReservationRepository.Object,
                mockAllocationRepository.Object,
                mockSystemParameterListRepository.Object,
                mockDateCalculator.Object,
                mockSingleDayAllocationCreator.Object).Create();

            // Assert
            Assert.Equal(newAllocations, result);

            mockAllocationRepository.Verify(r => r.AddAllocations(newAllocations), Times.Once);
        }

        private static Expression<Func<IReadOnlyList<Allocation>, bool>> ExpectedExistingAllocations(
            IReadOnlyList<Allocation> originalExistingAllocations,
            IReadOnlyList<Allocation> newAllocations,
            LocalDate date)
        {
            var expectedAllocations = originalExistingAllocations
                .Concat(newAllocations.Where(a => a.Date < date))
                .ToArray();

            return allocations =>
                allocations.Count == expectedAllocations.Length &&
                Enumerable.Range(0, expectedAllocations.Length).All(i => allocations[i] == expectedAllocations[i]);
        }
    }
}