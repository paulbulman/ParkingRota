namespace ParkingRota.UnitTests.Data
{
    using System;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using ParkingRota.Data;
    using Xunit;
    using DataSystemParameterList = ParkingRota.Data.SystemParameterList;
    using ModelSystemParameterList = ParkingRota.Business.Model.SystemParameterList;

    public class SystemParameterListRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> contextOptions;

        public SystemParameterListRepositoryTests() =>
            this.contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        [Fact]
        public void Test_GetSystemParameterList()
        {
            var systemParameterList = new DataSystemParameterList { Id = 1, ReservableSpaces = 4, NearbyDistance = 3.99m };

            // Arrange
            using (var context = this.CreateContext())
            {
                context.SystemParameterLists.Add(systemParameterList);
                context.SaveChanges();
            }

            var mapperConfiguration = new MapperConfiguration(c =>
            {
                c.CreateMap<DataSystemParameterList, ModelSystemParameterList>();
            });

            using (var context = this.CreateContext())
            {
                // Act
                var repository = new SystemParameterListRepository(
                    context,
                    new Mapper(mapperConfiguration));

                var result = repository.GetSystemParameterList();

                // Assert
                Assert.NotNull(result);

                Assert.Equal(systemParameterList.ReservableSpaces, result.ReservableSpaces);
                Assert.Equal(systemParameterList.NearbyDistance, result.NearbyDistance);
            }
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(this.contextOptions);
    }
}