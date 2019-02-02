namespace ParkingRota.UnitTests.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using Xunit;

    public static class BankHolidayUpdaterTests
    {
        [Fact]
        public static async Task Test_Update()
        {
            // Arrange
            var existingBankHolidays = new[]
            {
                new BankHoliday { Date = 25.December(2018) },
                new BankHoliday { Date = 26.December(2018) }
            };

            var mockBankHolidayRepository = new Mock<IBankHolidayRepository>(MockBehavior.Strict);
            mockBankHolidayRepository.Setup(r => r.GetBankHolidays()).Returns(existingBankHolidays);
            mockBankHolidayRepository.Setup(r => r.AddBankHolidays(It.IsAny<IReadOnlyList<BankHoliday>>()));

            var newBankHolidayDates = new[] { 25.December(2019), 26.December(2019) };

            var bankHolidayDates = existingBankHolidays.Select(b => b.Date)
                .Concat(newBankHolidayDates)
                .ToArray();

            var mockBankHolidayFetcher = new Mock<IBankHolidayFetcher>(MockBehavior.Strict);
            mockBankHolidayFetcher
                .Setup(f => f.Fetch())
                .Returns(Task.FromResult((IReadOnlyList<LocalDate>)bankHolidayDates));

            // Act
            await new BankHolidayUpdater(mockBankHolidayFetcher.Object, mockBankHolidayRepository.Object).Update();

            // Assert
            mockBankHolidayRepository.Verify(
                r => r.AddBankHolidays(It.Is<IReadOnlyList<BankHoliday>>(actual =>
                    actual.Select(b => b.Date).SequenceEqual(newBankHolidayDates))),
                Times.Once);
        }
    }
}