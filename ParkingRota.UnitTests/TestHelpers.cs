namespace ParkingRota.UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Moq;
    using NodaTime;
    using ParkingRota.Business.Model;
    using ParkingRota.Calendar;

    public static class TestHelpers
    {
        public static LocalDateTime At(this LocalDate localDate, int hour, int minute, int second) =>
            localDate.At(new LocalTime(hour, minute, second));

        public static Instant Utc(this LocalDateTime localDateTime) => localDateTime.InUtc().ToInstant();

        public static IReadOnlyList<LocalDate> ActiveDates<T>(this Calendar<T> calendar) =>
            calendar.Weeks
                .SelectMany(w => w.Days)
                .Where(d => d.IsActive)
                .Select(d => d.Date)
                .ToArray();

        public static T Data<T>(this Calendar<T> calendar, LocalDate date) =>
            calendar.Weeks
                .SelectMany(w => w.Days)
                .Single(d => d.Date == date)
                .Data;

        public static Mock<UserManager<ApplicationUser>> CreateMockUserManager(
            ClaimsPrincipal principal, ApplicationUser loggedInUser)
        {
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            mockUserManager
                .Setup(u => u.GetUserAsync(principal))
                .Returns(Task.FromResult(loggedInUser));

            return mockUserManager;
        }

        public static Mock<SignInManager<ApplicationUser>> CreateMockSigninManager(UserManager<ApplicationUser> userManager)
        {
            var httpContextAccessor = Mock.Of<IHttpContextAccessor>();
            var userClaimsPrincipalFactory = Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>();

            return new Mock<SignInManager<ApplicationUser>>(
                userManager, httpContextAccessor, userClaimsPrincipalFactory, null, null, null);
        }
    }
}