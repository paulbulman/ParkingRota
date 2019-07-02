namespace ParkingRota.UnitTests.Business
{
    using System.Linq;
    using Data;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using Xunit;
    using DataSystemParameterList = ParkingRota.Data.SystemParameterList;

    public class LastServiceRunTimeUpdaterTests : DatabaseTests
    {
        [Fact]
        public void Test_Update()
        {
            // Arrange
            var previousInstant = 28.June(2019).At(7, 40, 58).Utc();
            using (var context = this.CreateContext())
            {
                context.SystemParameterLists.Add(new DataSystemParameterList { LastServiceRunTime = previousInstant });
                context.SaveChanges();
            }

            var currentInstant = 28.June(2019).At(7, 41, 19).Utc();

            // Act
            using (var context = this.CreateContext())
            {
                new LastServiceRunTimeUpdater(
                        new FakeClock(currentInstant),
                        SystemParameterListRepositoryTests.CreateRepository(context))
                    .Update();
            }

            // Assert
            using (var context = this.CreateContext())
            {
                var result = context.SystemParameterLists.Single();
                
                Assert.Equal(currentInstant, result.LastServiceRunTime);
            }
        }
    }
}