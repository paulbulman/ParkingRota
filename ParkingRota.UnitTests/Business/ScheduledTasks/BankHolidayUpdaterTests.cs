namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Model;
    using ParkingRota.Business.ScheduledTasks;
    using ParkingRota.Data;
    using Xunit;

    public class BankHolidayUpdaterTests : DatabaseTests
    {
        [Fact]
        public void Test_ScheduledTaskType()
        {
            using (var scope = this.CreateScope())
            {
                Assert.Equal(
                    ScheduledTaskType.BankHolidayUpdater,
                    CreateBankHolidayUpdater(scope).ScheduledTaskType);
            }
        }

        [Fact]
        public async Task Test_Run()
        {
            // Arrange
            var newBankHolidayDates = new[] { 1.January(2020), 2.January(2020) };
            this.SetBankHolidayDates(newBankHolidayDates);

            var existingBankHolidays = new[]
            {
                this.Seed.BankHoliday(25.December(2019)),
                this.Seed.BankHoliday(26.December(2019))
            };

            var allBankHolidayDates = existingBankHolidays.Select(b => b.Date)
                .Concat(newBankHolidayDates)
                .ToArray();

            // Act
            using (var scope = this.CreateScope())
            {
                await CreateBankHolidayUpdater(scope).Run();
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var result = context.BankHolidays.ToArray();
                Assert.Equal(allBankHolidayDates.Length, result.Length);

                foreach (var expectedDate in allBankHolidayDates)
                {
                    Assert.Single(result.Where(b => b.Date == expectedDate));
                }
            }
        }

        [Theory]
        [InlineData(18, 0, 25, 0)]
        [InlineData(19, 0, 25, 0)]
        [InlineData(25, 0, 31, 23)]
        public void Test_GetNextRunTime(int currentDay, int currentHour, int expectedDay, int expectedHour)
        {
            // Arrange
            var currentInstant = currentDay.March(2019).At(currentHour, 00, 00).Utc();
            this.SetClock(currentInstant);

            using (var scope = this.CreateScope())
            {
                // Act
                var result = CreateBankHolidayUpdater(scope).GetNextRunTime(currentInstant);
                
                // Assert
                var expected = expectedDay.March(2019).At(expectedHour, 00, 00).Utc();

                Assert.Equal(expected, result);
            }
        }

        private static BankHolidayUpdater CreateBankHolidayUpdater(IServiceScope scope) =>
            scope.ServiceProvider
                .GetRequiredService<IEnumerable<IScheduledTask>>()
                .OfType<BankHolidayUpdater>()
                .Single();
    }
}