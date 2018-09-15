namespace ParkingRota.UnitTests
{
    using System.Threading.Tasks;
    using Areas.Identity.Pages.Account;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class RegisterTests
    {
        [Theory]
        [InlineData("A return URL")]
        [InlineData("Another return URL")]
        public async Task Test_Register_Succeeds(string returnUrl)
        {
            // Arrange
            // Set up user manager
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            mockUserManager
                .Setup(f => f.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(IdentityResult.Success))
                .Callback<IdentityUser, string>((user, password) => user.Id = "[New user Id]");
            mockUserManager
                .Setup(u => u.GenerateEmailConfirmationTokenAsync(It.IsAny<IdentityUser>()))
                .Returns(Task.FromResult("[Confirm email token]"));

            // Set up sign in manager
            var httpContextAccessor = Mock.Of<IHttpContextAccessor>();
            var userClaimsPrincipalFactory = Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>();

            var mockSigninManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object, httpContextAccessor, userClaimsPrincipalFactory, null, null, null);

            // Set up model
            var httpContext = new DefaultHttpContext();

            var actionContext = new ActionContext(
                httpContext, new RouteData(), new PageActionDescriptor(), new ModelStateDictionary());

            var mockUrlHelper = new Mock<UrlHelper>(actionContext);
            mockUrlHelper
                .Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns("[Confirm email URL]");

            var model = new RegisterModel(
                mockUserManager.Object,
                mockSigninManager.Object,
                Mock.Of<ILogger<RegisterModel>>(),
                Mock.Of<IEmailSender>())
            {
                PageContext = { HttpContext = httpContext },
                Input = new RegisterModel.InputModel { Email = "a@b.c", Password = "abc" },
                Url = mockUrlHelper.Object
            };

            // Act
            var result = await model.OnPostAsync(returnUrl);

            // Assert
            Assert.IsType<LocalRedirectResult>(result);
            Assert.Equal(returnUrl, ((LocalRedirectResult)result).Url);
        }
    }
}
