namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Emails;
    using ParkingRota.Business.Model;
    using Xunit;
    using RequestsReminder = ParkingRota.Business.ScheduledTasks.RequestsReminder;

    public static class RequestsReminderTests
    {
        [Fact]
        public static void Test_ScheduledTaskType()
        {
            var result = new RequestsReminder(
                Mock.Of<IDateCalculator>(),
                Mock.Of<IEmailRepository>(),
                Mock.Of<IRequestRepository>(),
                TestHelpers.CreateMockUserManager().Object).ScheduledTaskType;

            Assert.Equal(ScheduledTaskType.RequestReminder, result);
        }

        [Fact]
        public static async Task Test_Run_RequestsNotEntered()
        {
            // Arrange
            var date = 12.December(2018);

            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .Setup(d => d.GetCurrentDate())
                .Returns(date);
            mockDateCalculator
                .Setup(d => d.GetUpcomingLongLeadTimeAllocationDates())
                .Returns(new[] { 24.December(2018), 27.December(2018), 28.December(2018) });

            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);
            mockEmailRepository.Setup(e => e.AddToQueue(It.IsAny<IEmail>()));

            var earliestRequestDate = 12.November(2018);
            var latestRequestDate = 28.December(2018);

            var userWithRequests = new ApplicationUser();
            var userWithoutRequests = new ApplicationUser { Email = "a@b.c" };
            var otherUserWithoutRequests = new ApplicationUser { Email = "x@y.z" };

            var existingRequests = new[]
            {
                new Request { ApplicationUser = userWithRequests, Date = 28.December(2018) },
                new Request { ApplicationUser = userWithoutRequests, Date = 21.December(2018) },
                new Request { ApplicationUser = otherUserWithoutRequests, Date = 21.December(2018) }
            };

            var mockRequestRepository = new Mock<IRequestRepository>(MockBehavior.Strict);
            mockRequestRepository
                .Setup(r => r.GetRequests(earliestRequestDate, latestRequestDate))
                .Returns(existingRequests);

            var users = new[] { userWithRequests, userWithoutRequests, otherUserWithoutRequests };

            var mockUserManager = TestHelpers.CreateMockUserManager();
            mockUserManager.SetupGet(m => m.Users).Returns(users.AsQueryable);

            // Act
            var requestsReminder = new RequestsReminder(
                mockDateCalculator.Object,
                mockEmailRepository.Object,
                mockRequestRepository.Object,
                mockUserManager.Object);

            await requestsReminder.Run();

            // Assert
            foreach (var expectedApplicationUser in new[] { userWithoutRequests, otherUserWithoutRequests })
            {
                mockEmailRepository.Verify(
                    r => r.AddToQueue(
                        It.Is<ParkingRota.Business.Emails.RequestsReminder>(e => e.To == expectedApplicationUser.Email)),
                    Times.Once);
            }
        }

        [Fact]
        public static async Task Test_Run_RequestsAlreadyEntered()
        {
            // Arrange
            var date = 12.December(2018);

            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .Setup(d => d.GetCurrentDate())
                .Returns(date);
            mockDateCalculator
                .Setup(d => d.GetUpcomingLongLeadTimeAllocationDates())
                .Returns(new[] { 24.December(2018), 27.December(2018), 28.December(2018) });

            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);

            var earliestRequestDate = 12.November(2018);
            var latestRequestDate = 28.December(2018);

            var mockRequestRepository = new Mock<IRequestRepository>(MockBehavior.Strict);
            mockRequestRepository
                .Setup(r => r.GetRequests(earliestRequestDate, latestRequestDate))
                .Returns(new[] { new Request { ApplicationUser = new ApplicationUser(), Date = 28.December(2018) } });

            // Act and assert: mock strict on email repository ensures nothing has been done.
            var requestsReminder = new RequestsReminder(
                mockDateCalculator.Object,
                mockEmailRepository.Object,
                mockRequestRepository.Object,
                TestHelpers.CreateMockUserManager().Object);

            await requestsReminder.Run();
        }

        [Fact]
        public static async Task Test_Run_InactiveUser()
        {
            // Arrange
            var date = 12.December(2018);

            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .Setup(d => d.GetCurrentDate())
                .Returns(date);
            mockDateCalculator
                .Setup(d => d.GetUpcomingLongLeadTimeAllocationDates())
                .Returns(new[] { 24.December(2018), 27.December(2018), 28.December(2018) });

            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);

            var earliestRequestDate = 12.November(2018);
            var latestRequestDate = 28.December(2018);

            var mockRequestRepository = new Mock<IRequestRepository>(MockBehavior.Strict);
            mockRequestRepository
                .Setup(r => r.GetRequests(earliestRequestDate, latestRequestDate))
                .Returns(new List<Request>());

            // Act and assert: mock strict on email repository ensures nothing has been done.
            var requestsReminder = new RequestsReminder(
                mockDateCalculator.Object,
                mockEmailRepository.Object,
                mockRequestRepository.Object,
                TestHelpers.CreateMockUserManager().Object);

            await requestsReminder.Run();
        }
    }
}