namespace ParkingRota.UnitTests.Pages
{
    using Microsoft.AspNetCore.Http;
    using ParkingRota.Pages;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Xunit;

    public static class IndexModelTests
    {
        [Fact]
        public static void Test_Index_RedirectsAuthenticatedUser()
        {
            var result = LoadPage(isAuthenticated: true);

            Assert.IsType<RedirectToPageResult>(result);

            Assert.Equal("/Summary", ((RedirectToPageResult)result).PageName);
        }

        [Fact]
        public static void Test_Index_UnauthenticatedUser()
        {
            var result = LoadPage(isAuthenticated: false);

            Assert.IsType<PageResult>(result);
        }

        private static IActionResult LoadPage(bool isAuthenticated)
        {
            var userManager = TestHelpers.CreateMockUserManager().Object;

            var principal = new ClaimsPrincipal();

            var mockSigninManager = TestHelpers.CreateMockSigninManager(userManager);
            mockSigninManager
                .Setup(s => s.IsSignedIn(principal))
                .Returns(isAuthenticated);

            var model = new IndexModel(mockSigninManager.Object)
            {
                PageContext = { HttpContext = new DefaultHttpContext { User = principal } }
            };

            return model.OnGet();
        }
    }
}