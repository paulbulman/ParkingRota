namespace ParkingRota.UnitTests.Business
{
    using System.Linq;
    using Data;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Data;
    using Xunit;
    using DataSystemParameterList = ParkingRota.Data.SystemParameterList;

    public class LastServiceRunTimeUpdaterTests : DatabaseTests
    {
        [Fact]
        public void Test_Update()
        {
            // Arrange
            var previousInstant = 28.June(2019).At(7, 40, 58).Utc();
            var currentInstant = 28.June(2019).At(7, 41, 19).Utc();

            this.SetClock(currentInstant);

            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                context.SystemParameterLists.Add(new DataSystemParameterList { LastServiceRunTime = previousInstant });
                context.SaveChanges();
            }

            // Act
            using (var scope = this.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<LastServiceRunTimeUpdater>().Update();
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var result = context.SystemParameterLists.Single();
                
                Assert.Equal(currentInstant, result.LastServiceRunTime);
            }
        }
    }
}