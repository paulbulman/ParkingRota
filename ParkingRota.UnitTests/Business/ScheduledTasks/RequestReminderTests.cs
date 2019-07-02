namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using Xunit;
    using DataRequest = ParkingRota.Data.Request;

    public class RequestReminderTests : DatabaseTests
    {
        [Fact]
        public void Test_ScheduledTaskType()
        {
            using (var context = this.CreateContext())
            {
                var result = new RequestReminderBuilder()
                    .Build(context)
                    .ScheduledTaskType;

                Assert.Equal(ScheduledTaskType.RequestReminder, result);
            }
        }

        [Fact]
        public async Task Test_Run_RequestsNotEntered()
        {
            // Arrange
            var date = 12.December(2018);

            var userWithRequests = new ApplicationUser();
            var userWithoutRequests = new ApplicationUser { Email = "a@b.c" };
            var otherUserWithoutRequests = new ApplicationUser { Email = "x@y.z" };

            var existingRequests = new[]
            {
                new DataRequest { ApplicationUser = userWithRequests, Date = 28.December(2018) },
                new DataRequest { ApplicationUser = userWithoutRequests, Date = 21.December(2018) },
                new DataRequest { ApplicationUser = otherUserWithoutRequests, Date = 21.December(2018) }
            };

            this.SeedDatabase(existingRequests);

            // Act
            using (var context = this.CreateContext())
            {
                await new RequestReminderBuilder()
                    .WithCurrentInstant(date.AtMidnight().Utc())
                    .WithUsers(userWithRequests, userWithoutRequests, otherUserWithoutRequests)
                    .Build(context)
                    .Run();
            }

            // Assert
            using (var context = this.CreateContext())
            {
                foreach (var expectedApplicationUser in new[] { userWithoutRequests, otherUserWithoutRequests })
                {
                    var userEmails = context.EmailQueueItems.Where(e =>
                        e.To == expectedApplicationUser.Email &&
                        e.Subject == $"No requests entered for {24.December(2018).ForDisplay()} - {28.December(2018).ForDisplay()}");

                    Assert.Single(userEmails);
                }
            }
        }

        [Fact]
        public async Task Test_Run_RequestsAlreadyEntered()
        {
            // Arrange
            var date = 12.December(2018);

            var user = new ApplicationUser();

            this.SeedDatabase(new DataRequest { ApplicationUser = user, Date = 28.December(2018) });

            // Act
            using (var context = this.CreateContext())
            {
                await new RequestReminderBuilder()
                    .WithCurrentInstant(date.AtMidnight().Utc())
                    .WithUsers(user)
                    .Build(context)
                    .Run();
            }

            // Assert
            using (var context = this.CreateContext())
            {
                Assert.Empty(context.EmailQueueItems);
            }
        }

        [Fact]
        public async Task Test_Run_InactiveUser()
        {
            // Arrange
            var date = 12.December(2018);

            var user = new ApplicationUser();

            this.SeedDatabase(new DataRequest { ApplicationUser = user, Date = date.PlusDays(-31) });

            // Act
            using (var context = this.CreateContext())
            {
                await new RequestReminderBuilder()
                    .WithCurrentInstant(date.AtMidnight().Utc())
                    .WithUsers(user)
                    .Build(context)
                    .Run();
            }

            // Assert
            using (var context = this.CreateContext())
            {
                Assert.Empty(context.EmailQueueItems);
            }
        }

        [Fact]
        public async Task Test_Run_VisitorUser()
        {
            // Arrange
            var date = 12.December(2018);

            var user = new ApplicationUser { IsVisitor = true };

            this.SeedDatabase(new DataRequest { ApplicationUser = user, Date = 21.December(2018) });

            // Act
            using (var context = this.CreateContext())
            {
                await new RequestReminderBuilder()
                    .WithCurrentInstant(date.AtMidnight().Utc())
                    .WithUsers(user)
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
        [InlineData(14, 0, 21, 0)]
        [InlineData(15, 0, 21, 0)]
        [InlineData(21, 0, 27, 23)]
        public void Test_GetNextRunTime(int currentDay, int currentHour, int expectedDay, int expectedHour)
        {
            // Arrange
            var currentInstant = currentDay.March(2018).At(currentHour, 00, 00).Utc();

            using (var context = this.CreateContext())
            {
                // Act
                var result = new RequestReminderBuilder()
                    .WithCurrentInstant(currentInstant)
                    .Build(context)
                    .GetNextRunTime(currentInstant);

                // Assert
                var expected = expectedDay.March(2018).At(expectedHour, 00, 00).Utc();

                Assert.Equal(expected, result);
            }
        }

        private void SeedDatabase(params DataRequest[] existingRequests)
        {
            using (var context = this.CreateContext())
            {
                context.Requests.AddRange(existingRequests);
                context.SaveChanges();
            }
        }
    }
}