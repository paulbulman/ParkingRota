namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using Xunit;
    using DataAllocation = ParkingRota.Data.Allocation;
    using DataRequest = ParkingRota.Data.Request;

    public class DailySummaryTests : DatabaseTests
    {
        [Fact]
        public void Test_ScheduledTaskType()
        {
            using (var context = this.CreateContext())
            {
                var result = new DailySummaryBuilder()
                    .Build(context)
                    .ScheduledTaskType;

                Assert.Equal(ScheduledTaskType.DailySummary, result);
            }
        }

        [Fact]
        public async void Test_Run()
        {
            // Arrange
            var nextWorkingDate = 28.December(2018);

            var allocatedUser = new ApplicationUser { Email = "a@b.c" };
            var interruptedUser = new ApplicationUser { Email = "x@y.z" };

            var allocations = new[] { new DataAllocation { ApplicationUser = allocatedUser, Date = nextWorkingDate } };
            var requests = new[]
            {
                new DataRequest { ApplicationUser = allocatedUser, Date = nextWorkingDate },
                new DataRequest { ApplicationUser = interruptedUser, Date = nextWorkingDate }
            };

            this.SeedDatabase(requests, allocations);

            // Act
            using (var context = this.CreateContext())
            {
                await new DailySummaryBuilder()
                    .WithCurrentInstant(27.December(2018).At(11, 0, 0).Utc())
                    .Build(context)
                    .Run();
            }

            // Assert
            using (var context = this.CreateContext())
            {
                foreach (var applicationUser in new[] { allocatedUser, interruptedUser })
                {
                    var userEmails = context.EmailQueueItems.Where(e =>
                        e.To == applicationUser.Email &&
                        e.Subject.Contains(nextWorkingDate.ForDisplay()));

                    Assert.Single(userEmails);
                }
            }
        }

        [Fact]
        public async Task Test_Run_ExcludesVisitorAccounts()
        {
            // Arrange
            var nextWorkingDate = 28.December(2018);

            var visitorUser = new ApplicationUser { Email = "x@y.z", IsVisitor = true };

            var visitorRequest = new DataRequest { ApplicationUser = visitorUser, Date = nextWorkingDate };

            this.SeedDatabase(new[] { visitorRequest }, new List<DataAllocation>());

            // Act
            using (var context = this.CreateContext())
            {
                await new DailySummaryBuilder()
                    .WithCurrentInstant(27.December(2018).At(11, 0, 0).Utc())
                    .Build(context)
                    .Run();
            }

            // Assert
            using (var context = this.CreateContext())
            {
                Assert.Empty(context.EmailQueueItems);
            }
        }

        [Theory]
        [InlineData(22, 11, 23, 11)]
        [InlineData(23, 11, 26, 10)]
        public void Test_GetNextRunTime(int currentDay, int currentHour, int expectedDay, int expectedHour)
        {
            // Arrange
            var currentInstant = currentDay.March(2018).At(currentHour, 00, 00).Utc();

            using (var context = this.CreateContext())
            {
                // Act
                var result = new DailySummaryBuilder()
                    .WithCurrentInstant(currentInstant)
                    .Build(context)
                    .GetNextRunTime(currentInstant);

                // Assert
                var expected = expectedDay.March(2018).At(expectedHour, 00, 00).Utc();

                Assert.Equal(expected, result);
            }
        }

        private void SeedDatabase(IReadOnlyList<DataRequest> requests, IReadOnlyList<DataAllocation> allocations)
        {
            using (var context = this.CreateContext())
            {
                context.Requests.AddRange(requests);
                context.Allocations.AddRange(allocations);

                context.SaveChanges();
            }
        }
    }
}