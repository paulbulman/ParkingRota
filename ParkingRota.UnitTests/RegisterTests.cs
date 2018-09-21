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
    using ParkingRota.Business;
    using Xunit;

    public class RegisterTests
    {
        [Theory]
        [InlineData("A return URL", "A valid registration token", "An unbreached password")]
        [InlineData("Another return URL", "Another valid registration token", "Another unbreached password")]
        public async Task Test_Register_Succeeds(string returnUrl, string registrationToken, string password)
        {
            // Arrange
            // Set up user manager
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            mockUserManager
                .Setup(f => f.CreateAsync(It.IsAny<IdentityUser>(), password))
                .Returns(Task.FromResult(IdentityResult.Success))
                .Callback<IdentityUser, string>((user, p) => user.Id = "[New user Id]");
            mockUserManager
                .Setup(u => u.GenerateEmailConfirmationTokenAsync(It.IsAny<IdentityUser>()))
                .Returns(Task.FromResult("[Confirm email token]"));

            // Set up registration token validator
            var mockRegistrationTokenValidator = new Mock<IRegistrationTokenValidator>(MockBehavior.Strict);
            mockRegistrationTokenValidator.Setup(v => v.TokenIsValid(registrationToken)).Returns(true);

            // Set up password breach checker
            var mockPasswordBreachChecker = new Mock<IPasswordBreachChecker>(MockBehavior.Strict);
            mockPasswordBreachChecker.Setup(c => c.PasswordIsBreached(password)).Returns(Task.FromResult(false));

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
                mockRegistrationTokenValidator.Object,
                mockPasswordBreachChecker.Object,
                mockSigninManager.Object,
                Mock.Of<ILogger<RegisterModel>>(),
                Mock.Of<IEmailSender>())
            {
                PageContext = { HttpContext = httpContext },
                Input = CreateInputModel(registrationToken, password),
                Url = mockUrlHelper.Object
            };

            // Act
            var result = await model.OnPostAsync(returnUrl);

            // Assert
            Assert.IsType<LocalRedirectResult>(result);
            Assert.Equal(returnUrl, ((LocalRedirectResult)result).Url);
        }

        private static RegisterModel.InputModel CreateInputModel(string registrationToken, string password) =>
            new RegisterModel.InputModel
            {
                Email = "a@b.c",
                Password = password,
                RegistrationToken = registrationToken
            };

        [Theory]
        [InlineData("A registration token")]
        [InlineData("Another registration token")]
        public async Task Test_Register_InvalidToken(string registrationToken)
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

            // Set up registration token validator
            var mockRegistrationTokenValidator = new Mock<IRegistrationTokenValidator>(MockBehavior.Strict);
            mockRegistrationTokenValidator.Setup(v => v.TokenIsValid(registrationToken)).Returns(false);

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
                mockRegistrationTokenValidator.Object,
                Mock.Of<IPasswordBreachChecker>(),
                mockSigninManager.Object,
                Mock.Of<ILogger<RegisterModel>>(),
                Mock.Of<IEmailSender>())
            {
                PageContext = { HttpContext = httpContext },
                Input = CreateInputModel(registrationToken, "password"),
                Url = mockUrlHelper.Object
            };

            // Act
            var result = await model.OnPostAsync("Return URL");

            // Assert
            Assert.IsType<PageResult>(result);
        }

        [Theory]
        [InlineData("A password")]
        [InlineData("Another password")]
        public async Task Test_Register_BreachedPassword(string password)
        {
            // Arrange
            // Set up user manager
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            mockUserManager
                .Setup(f => f.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(IdentityResult.Success))
                .Callback<IdentityUser, string>((user, p) => user.Id = "[New user Id]");
            mockUserManager
                .Setup(u => u.GenerateEmailConfirmationTokenAsync(It.IsAny<IdentityUser>()))
                .Returns(Task.FromResult("[Confirm email token]"));

            // Set up registration token validator
            var mockRegistrationTokenValidator = new Mock<IRegistrationTokenValidator>(MockBehavior.Strict);
            mockRegistrationTokenValidator.Setup(v => v.TokenIsValid(It.IsAny<string>())).Returns(true);

            // Set up password breach checker
            var mockPasswordBreachChecker = new Mock<IPasswordBreachChecker>(MockBehavior.Strict);
            mockPasswordBreachChecker.Setup(c => c.PasswordIsBreached(password)).Returns(Task.FromResult(true));

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
                mockRegistrationTokenValidator.Object,
                mockPasswordBreachChecker.Object,
                mockSigninManager.Object,
                Mock.Of<ILogger<RegisterModel>>(),
                Mock.Of<IEmailSender>())
            {
                PageContext = { HttpContext = httpContext },
                Input = CreateInputModel("token", password),
                Url = mockUrlHelper.Object
            };

            // Act
            var result = await model.OnPostAsync("Return URL");

            // Assert
            Assert.IsType<PageResult>(result);
        }
    }
}
