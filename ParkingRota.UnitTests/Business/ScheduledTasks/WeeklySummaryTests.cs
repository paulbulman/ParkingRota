namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using Xunit;
    using DataAllocation = ParkingRota.Data.Allocation;
    using DataRequest = ParkingRota.Data.Request;

    public class WeeklySummaryTests : DatabaseTests
    {
        [Fact]
        public void Test_ScheduledTaskType()
        {
            using (var context = this.CreateContext())
            {
                var result = new WeeklySummaryBuilder()
                    .Build(context)
                    .ScheduledTaskType;

                Assert.Equal(ScheduledTaskType.WeeklySummary, result);
            }
        }

        [Fact]
        public async void Test_Run()
        {
            // Arrange
            var firstDate = 24.December(2018);
            var lastDate = 28.December(2018);

            var allocatedUser = new ApplicationUser { Email = "a@b.c" };
            var interruptedUser = new ApplicationUser { Email = "x@y.z" };

            var allocations = new[]
            {
                new DataAllocation { ApplicationUser = allocatedUser, Date = firstDate },
                new DataAllocation { ApplicationUser = allocatedUser, Date = lastDate }
            };

            var requests = new[]
            {
                new DataRequest { ApplicationUser = allocatedUser, Date = firstDate },
                new DataRequest { ApplicationUser = allocatedUser, Date = lastDate },
                new DataRequest { ApplicationUser = interruptedUser, Date = firstDate },
                new DataRequest { ApplicationUser = interruptedUser, Date = lastDate }
            };

            this.SeedDatabase(requests, allocations);

            // Act
            using (var context = this.CreateContext())
            {
                await new WeeklySummaryBuilder()
                    .WithCurrentInstant(13.December(2018).AtMidnight().Utc())
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
                        e.Subject == $"Weekly provisional allocations summary for {firstDate.ForDisplay()} - {lastDate.ForDisplay()}");

                    Assert.Single(userEmails);
                }
            }
        }

        [Fact]
        public async void Test_Run_ExcludesVisitorAccounts()
        {
            // Arrange
            var firstDate = 24.December(2018);
            var lastDate = 28.December(2018);

            var visitorUser = new ApplicationUser { Email = "x@y.z", IsVisitor = true };

            var requests = new[]
            {
                new DataRequest { ApplicationUser = visitorUser, Date = firstDate },
                new DataRequest { ApplicationUser = visitorUser, Date = lastDate }
            };

            this.SeedDatabase(requests, new List<DataAllocation>());

            // Act
            using (var context = this.CreateContext())
            {
                await new WeeklySummaryBuilder()
                    .WithCurrentInstant(13.December(2018).AtMidnight().Utc())
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
        [InlineData(15, 0, 22, 0)]
        [InlineData(16, 0, 22, 0)]
        [InlineData(22, 0, 28, 23)]
        public void Test_GetNextRunTime(int currentDay, int currentHour, int expectedDay, int expectedHour)
        {
            // Arrange
            var currentInstant = currentDay.March(2018).At(currentHour, 00, 00).Utc();

            using (var context = this.CreateContext())
            {
                // Act
                var result = new WeeklySummaryBuilder()
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