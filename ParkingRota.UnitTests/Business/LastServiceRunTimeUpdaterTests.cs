namespace ParkingRota.UnitTests.Business
{
    using Moq;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using Xunit;

    public static class LastServiceRunTimeUpdaterTests
    {
        [Fact]
        public static void Test_Update()
        {
            // Arrange
            var previousInstant = 28.June(2019).At(7, 40, 58).Utc();

            var mockSystemParameterListRepository = new Mock<ISystemParameterListRepository>(MockBehavior.Strict);

            mockSystemParameterListRepository.Setup(r => r.GetSystemParameterList())
                .Returns(new SystemParameterList {LastServiceRunTime = previousInstant});
            mockSystemParameterListRepository.Setup(r => r.UpdateSystemParameterList(It.IsAny<SystemParameterList>()));

            var currentInstant = 28.June(2019).At(7, 41, 19).Utc();
            var fakeClock = new FakeClock(currentInstant);
            
            // Act
            new LastServiceRunTimeUpdater(fakeClock, mockSystemParameterListRepository.Object)
                .Update();

            // Assert
            mockSystemParameterListRepository.Verify(
                r => r.UpdateSystemParameterList(
                    It.Is<SystemParameterList>(p => p.LastServiceRunTime == currentInstant)),
                Times.Once);
        }
    }
}