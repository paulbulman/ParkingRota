﻿namespace ParkingRota.UnitTests.Data
{
    using System.Linq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Data;
    using Xunit;
    using DataSystemParameterList = ParkingRota.Data.SystemParameterList;
    using ModelSystemParameterList = ParkingRota.Business.Model.SystemParameterList;

    public class SystemParameterListRepositoryTests : DatabaseTests
    {
        public static SystemParameterListRepository CreateRepository(ApplicationDbContext context) =>
            new SystemParameterListRepository(context, MapperBuilder.Build());

        [Fact]
        public void Test_GetSystemParameterList()
        {
            // Arrange
            var systemParameterList = this.SeedDatabase();

            using (var context = this.CreateContext())
            {
                // Act
                var result = CreateRepository(context).GetSystemParameterList();

                // Assert
                Assert.NotNull(result);

                Assert.Equal(systemParameterList.TotalSpaces, result.TotalSpaces);
                Assert.Equal(systemParameterList.ReservableSpaces, result.ReservableSpaces);
                Assert.Equal(systemParameterList.NearbyDistance, result.NearbyDistance);
                Assert.Equal(systemParameterList.FromEmailAddress, result.FromEmailAddress);
                Assert.Equal(systemParameterList.LastServiceRunTime, result.LastServiceRunTime);
            }
        }

        [Fact]
        public void Test_UpdateSystemParameterList()
        {
            // Arrange
            var systemParameterList = this.SeedDatabase();

            var updatedSystemParameterList = new ModelSystemParameterList
            {
                TotalSpaces = systemParameterList.TotalSpaces + 1,
                ReservableSpaces = systemParameterList.ReservableSpaces + 2,
                NearbyDistance = systemParameterList.NearbyDistance + 3,
                FromEmailAddress = systemParameterList.FromEmailAddress + "_updated",
                LastServiceRunTime = systemParameterList.LastServiceRunTime.Plus(4.Minutes())
            };

            // Act
            using (var context = this.CreateContext())
            {
                CreateRepository(context).UpdateSystemParameterList(updatedSystemParameterList);
            }

            // Assert
            using (var context = this.CreateContext())
            {
                var result = context.SystemParameterLists.ToArray();

                Assert.NotNull(result);
                Assert.Single(result);

                var actual = result.Single();

                Assert.Equal(updatedSystemParameterList.TotalSpaces, actual.TotalSpaces);
                Assert.Equal(updatedSystemParameterList.ReservableSpaces, actual.ReservableSpaces);
                Assert.Equal(updatedSystemParameterList.NearbyDistance, actual.NearbyDistance);
                Assert.Equal(updatedSystemParameterList.FromEmailAddress, actual.FromEmailAddress);
                Assert.Equal(updatedSystemParameterList.LastServiceRunTime, actual.LastServiceRunTime);
            }
        }

        private SystemParameterList SeedDatabase()
        {
            var systemParameterList = new DataSystemParameterList
            {
                Id = 1,
                TotalSpaces = 19,
                ReservableSpaces = 4,
                NearbyDistance = 3.99m,
                FromEmailAddress = "noreply@parkingrota",
                LastServiceRunTime = 27.June(2019).At(16, 54, 20).Utc()
            };

            using (var context = this.CreateContext())
            {
                context.SystemParameterLists.Add(systemParameterList);
                context.SaveChanges();
            }

            return systemParameterList;
        }
    }
}