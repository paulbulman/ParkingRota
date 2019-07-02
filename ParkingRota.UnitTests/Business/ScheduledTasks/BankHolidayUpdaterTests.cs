namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Model;
    using Xunit;
    using DataBankHoliday = ParkingRota.Data.BankHoliday;

    public class BankHolidayUpdaterTests : DatabaseTests
    {
        [Fact]
        public void Test_ScheduledTaskType()
        {
            using (var context = this.CreateContext())
            {
                var result = new BankHolidayUpdaterBuilder()
                    .Build(context)
                    .ScheduledTaskType;

                Assert.Equal(ScheduledTaskType.BankHolidayUpdater, result);
            }
        }

        [Fact]
        public async Task Test_Run()
        {
            // Arrange
            var existingBankHolidays = new[]
            {
                new DataBankHoliday { Date = 25.December(2019) },
                new DataBankHoliday { Date = 26.December(2019) }
            };

            this.SeedDatabase(existingBankHolidays);

            var newBankHolidayDates = new[] { 1.January(2020), 2.January(2020) };

            var allBankHolidayDates = existingBankHolidays.Select(b => b.Date)
                .Concat(newBankHolidayDates)
                .ToArray();

            // Act
            using (var context = this.CreateContext())
            {
                await new BankHolidayUpdaterBuilder()
                    .WithReturnedBankHolidayDates(allBankHolidayDates)
                    .Build(context)
                    .Run();
            }

            // Assert
            using (var context = this.CreateContext())
            {
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

            // Act
            Instant result;
            using (var context = this.CreateContext())
            {
                result = new BankHolidayUpdaterBuilder()
                    .WithCurrentInstant(currentInstant)
                    .Build(context)
                    .GetNextRunTime(currentInstant);
            }

            // Assert
            var expected = expectedDay.March(2019).At(expectedHour, 00, 00).Utc();

            Assert.Equal(expected, result);
        }

        private void SeedDatabase(IReadOnlyList<DataBankHoliday> bankHolidays)
        {
            using (var context = this.CreateContext())
            {
                context.BankHolidays.AddRange(bankHolidays);
                context.SaveChanges();
            }
        }
    }
}