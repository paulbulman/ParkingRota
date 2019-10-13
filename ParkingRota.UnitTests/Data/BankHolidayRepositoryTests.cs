namespace ParkingRota.UnitTests.Data
{
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Model;
    using ParkingRota.Data;
    using Xunit;
    using ModelBankHoliday = ParkingRota.Business.Model.BankHoliday;

    public class BankHolidayRepositoryTests : DatabaseTests
    {
        public static BankHolidayRepository CreateRepository(IApplicationDbContext context) =>
            new BankHolidayRepository(context, MapperBuilder.Build());

        [Fact]
        public void Test_GetBankHolidays()
        {
            // Arrange
            var existingBankHolidays = new[]
            {
                this.Seed.BankHoliday(25.December(2019)),
                this.Seed.BankHoliday(26.December(2019))
            };

            using (var scope = this.CreateScope())
            {
                // Act
                var result = scope.ServiceProvider
                    .GetRequiredService<IBankHolidayRepository>()
                    .GetBankHolidays();

                // Assert
                Assert.Equal(existingBankHolidays.Length, result.Count);

                foreach (var existingBankHoliday in existingBankHolidays)
                {
                    Assert.Single(result.Where(b => b.Date == existingBankHoliday.Date));
                }
            }
        }

        [Fact]
        public void Test_AddBankHolidays()
        {
            // Arrange
            var existingBankHolidays = new[]
            {
                this.Seed.BankHoliday(25.December(2018)),
                this.Seed.BankHoliday(26.December(2018))
            };

            var newBankHolidays = new[]
            {
                new ModelBankHoliday { Date = 25.December(2019) },
                new ModelBankHoliday { Date = 26.December(2019) }
            };

            using (var scope = this.CreateScope())
            {
                // Act
                scope.ServiceProvider
                    .GetRequiredService<IBankHolidayRepository>()
                    .AddBankHolidays(newBankHolidays);
            }

            // Assert
            var expectedDates = existingBankHolidays.Select(b => b.Date)
                .Concat(newBankHolidays.Select(b => b.Date))
                .ToArray();

            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var result = context.BankHolidays.ToArray();

                Assert.Equal(expectedDates.Length, result.Length);

                foreach (var expectedDate in expectedDates)
                {
                    Assert.Single(result.Where(b => b.Date == expectedDate));
                }
            }
        }
    }
}