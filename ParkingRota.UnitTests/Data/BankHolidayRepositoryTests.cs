namespace ParkingRota.UnitTests.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Data;
    using Xunit;
    using DataBankHoliday = ParkingRota.Data.BankHoliday;
    using ModelBankHoliday = ParkingRota.Business.Model.BankHoliday;

    public class BankHolidayRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> contextOptions;

        public BankHolidayRepositoryTests() =>
            this.contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        [Fact]
        public void Test_GetBankHolidays()
        {
            // Arrange
            var existingBankHolidays = new[]
            {
                new DataBankHoliday { Date = 25.December(2019) },
                new DataBankHoliday { Date = 26.December(2019) }
            };

            this.SeedDatabase(existingBankHolidays);

            var mapperConfiguration = new MapperConfiguration(c =>
            {
                c.CreateMap<DataBankHoliday, ModelBankHoliday>();
            });

            var mapper = new Mapper(mapperConfiguration);

            using (var context = this.CreateContext())
            {
                // Act
                var result = new BankHolidayRepository(context, mapper).GetBankHolidays();

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
                new DataBankHoliday { Date = 25.December(2018) },
                new DataBankHoliday { Date = 26.December(2018) }
            };

            this.SeedDatabase(existingBankHolidays);

            var newBankHolidays = new[]
            {
                new ModelBankHoliday { Date = 25.December(2019) },
                new ModelBankHoliday { Date = 26.December(2019) }
            };

            // Act
            using (var context = this.CreateContext())
            {
                var repository = new BankHolidayRepository(context, Mock.Of<IMapper>());

                repository.AddBankHolidays(newBankHolidays);
            }

            // Assert
            var expectedDates = existingBankHolidays.Select(b => b.Date)
                .Concat(newBankHolidays.Select(b => b.Date))
                .ToArray();

            using (var context = this.CreateContext())
            {
                var result = context.BankHolidays.ToArray();

                Assert.Equal(expectedDates.Length, result.Length);

                foreach (var expectedDate in expectedDates)
                {
                    Assert.Single(result.Where(b => b.Date == expectedDate));
                }
            }
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(this.contextOptions);

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