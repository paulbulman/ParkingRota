namespace ParkingRota.UnitTests.Pages
{
    using Microsoft.AspNetCore.Http;
    using ParkingRota.Pages;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Areas.Identity.Pages.Account;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;
    using Moq;
    using ParkingRota.Business.Model;
    using Xunit;

    public static class IndexModelTests
    {
        [Fact]
        public static async Task Test_Index_RedirectsAuthenticatedUser()
        {
            var result = await LoadPage(isAuthenticated: true);

            Assert.IsType<RedirectToPageResult>(result);

            Assert.Equal("/Summary", ((RedirectToPageResult)result).PageName);
        }

        [Fact]
        public static async Task Test_Index_UnauthenticatedUser()
        {
            var result = await LoadPage(isAuthenticated: false);

            Assert.IsType<PageResult>(result);
        }

        private static async Task<IActionResult> LoadPage(bool isAuthenticated)
        {
            var principal = new ClaimsPrincipal();

            var userManager = TestHelpers.CreateMockUserManager(principal, new ApplicationUser()).Object;

            var mockSigninManager = TestHelpers.CreateMockSigninManager(userManager);
            mockSigninManager
                .Setup(s => s.IsSignedIn(principal))
                .Returns(isAuthenticated);

            var model = new IndexModel(
                TestHelpers.CreateHttpContextAccessor(0x2414188f),
                Mock.Of<ILogger<LoginModel>>(),
                mockSigninManager.Object,
                userManager)
            {
                PageContext = { HttpContext = new DefaultHttpContext { User = principal } }
            };

            return await model.OnGetAsync();
        }
    }
}