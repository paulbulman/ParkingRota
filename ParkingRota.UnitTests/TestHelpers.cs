namespace ParkingRota.UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Moq;
    using NodaTime;
    using ParkingRota.Business;
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

        public static IPasswordBreachChecker CreatePasswordBreachChecker(string password, bool isBreached)
        {
            var mockPasswordBreachChecker = new Mock<IPasswordBreachChecker>(MockBehavior.Strict);
            mockPasswordBreachChecker
                .Setup(c => c.PasswordIsBreached(password))
                .Returns(Task.FromResult(isBreached));

            return mockPasswordBreachChecker.Object;
        }

        public static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(int ipAddress)
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);

            mockHttpContextAccessor
                .SetupGet(a => a.HttpContext.Request.Headers)
                .Returns(new HeaderDictionary());
            mockHttpContextAccessor
                .SetupGet(a => a.HttpContext.Connection.RemoteIpAddress)
                .Returns(new IPAddress(ipAddress));

            return mockHttpContextAccessor;
        }

        public static Mock<SignInManager<ApplicationUser>> CreateMockSigninManager(UserManager<ApplicationUser> userManager)
        {
            var httpContextAccessor = Mock.Of<IHttpContextAccessor>();
            var userClaimsPrincipalFactory = Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>();

            return new Mock<SignInManager<ApplicationUser>>(
                userManager, httpContextAccessor, userClaimsPrincipalFactory, null, null, null);
        }

        public static Mock<UserManager<ApplicationUser>> CreateMockUserManager() =>
            new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

        public static Mock<UserManager<ApplicationUser>> CreateMockUserManager(
            ClaimsPrincipal principal, ApplicationUser loggedInUser)
        {
            var mockUserManager = CreateMockUserManager();

            mockUserManager
                .Setup(u => u.GetUserAsync(principal))
                .Returns(Task.FromResult(loggedInUser));

            return mockUserManager;
        }
    }
}