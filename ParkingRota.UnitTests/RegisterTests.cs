namespace ParkingRota.UnitTests
{
    using System;
    using System.Net;
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
    using ParkingRota.Business.Model;
    using Xunit;

    public class RegisterTests
    {
        private const string EmailAddress = "a@b.c";

        [Theory]
        [InlineData("A return URL", "A valid registration token", "An unbreached password")]
        [InlineData("Another return URL", "Another valid registration token", "Another unbreached password")]
        public async Task Test_Register_Succeeds(string returnUrl, string registrationToken, string password)
        {
            const int IpAddressInt = 0x2414188f;
            const string IpAddressString = "143.24.20.36";

            const string ConfirmEmailUrl = "[Confirm email URL]";

            const string ExpectedSubject = "[Parking Rota] Confirm your email";

            // Arrange
            // Set up HTTP context accessor
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
            mockHttpContextAccessor
                .SetupGet(a => a.HttpContext.Request.Headers)
                .Returns(new HeaderDictionary());
            mockHttpContextAccessor
                .SetupGet(a => a.HttpContext.Connection.RemoteIpAddress)
                .Returns(new IPAddress(IpAddressInt));

            // Set up user manager
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            mockUserManager
                .Setup(f => f.CreateAsync(It.IsAny<ApplicationUser>(), password))
                .Returns(Task.FromResult(IdentityResult.Success))
                .Callback<ApplicationUser, string>((user, p) => user.Id = "[New user Id]");
            mockUserManager
                .Setup(u => u.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .Returns(Task.FromResult("[Confirm email token]"));

            // Set up registration token validator
            var mockRegistrationTokenValidator = new Mock<IRegistrationTokenValidator>(MockBehavior.Strict);
            mockRegistrationTokenValidator.Setup(v => v.TokenIsValid(registrationToken)).Returns(true);

            // Set up password breach checker
            var mockPasswordBreachChecker = new Mock<IPasswordBreachChecker>(MockBehavior.Strict);
            mockPasswordBreachChecker.Setup(c => c.PasswordIsBreached(password)).Returns(Task.FromResult(false));

            // Set up sign in manager
            var mockSigninManager = TestHelpers.CreateMockSigninManager(mockUserManager.Object);

            // Set up email sender
            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            mockEmailSender
                .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(default(object)));

            // Set up model
            var httpContext = new DefaultHttpContext();

            var actionContext = new ActionContext(
                httpContext, new RouteData(), new PageActionDescriptor(), new ModelStateDictionary());

            var mockUrlHelper = new Mock<UrlHelper>(actionContext);
            mockUrlHelper
                .Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(ConfirmEmailUrl);

            var model = new RegisterModel(
                mockHttpContextAccessor.Object,
                mockUserManager.Object,
                mockRegistrationTokenValidator.Object,
                mockPasswordBreachChecker.Object,
                mockSigninManager.Object,
                Mock.Of<ILogger<RegisterModel>>(),
                mockEmailSender.Object)
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

            mockEmailSender.Verify(e => e.SendEmailAsync(
                    EmailAddress,
                    ExpectedSubject,
                    It.Is<string>(s =>
                        s.Contains(ConfirmEmailUrl, StringComparison.Ordinal) &&
                        s.Contains(IpAddressString, StringComparison.OrdinalIgnoreCase))),
                Times.Once);
        }

        [Theory]
        [InlineData("A registration token")]
        [InlineData("Another registration token")]
        public async Task Test_Register_InvalidToken(string registrationToken)
        {
            // Arrange
            // Set up user manager
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            // Set up registration token validator
            var mockRegistrationTokenValidator = new Mock<IRegistrationTokenValidator>(MockBehavior.Strict);
            mockRegistrationTokenValidator.Setup(v => v.TokenIsValid(registrationToken)).Returns(false);

            // Set up sign in manager
            var mockSigninManager = TestHelpers.CreateMockSigninManager(mockUserManager.Object);

            // Set up model
            var model = new RegisterModel(
                Mock.Of<IHttpContextAccessor>(),
                mockUserManager.Object,
                mockRegistrationTokenValidator.Object,
                Mock.Of<IPasswordBreachChecker>(),
                mockSigninManager.Object,
                Mock.Of<ILogger<RegisterModel>>(),
                Mock.Of<IEmailSender>())
            {
                Input = CreateInputModel(registrationToken, "password")
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
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            // Set up registration token validator
            var mockRegistrationTokenValidator = new Mock<IRegistrationTokenValidator>(MockBehavior.Strict);
            mockRegistrationTokenValidator.Setup(v => v.TokenIsValid(It.IsAny<string>())).Returns(true);

            // Set up password breach checker
            var mockPasswordBreachChecker = new Mock<IPasswordBreachChecker>(MockBehavior.Strict);
            mockPasswordBreachChecker.Setup(c => c.PasswordIsBreached(password)).Returns(Task.FromResult(true));

            // Set up sign in manager
            var mockSigninManager = TestHelpers.CreateMockSigninManager(mockUserManager.Object);

            // Set up model
            var model = new RegisterModel(
                Mock.Of<IHttpContextAccessor>(),
                mockUserManager.Object,
                mockRegistrationTokenValidator.Object,
                mockPasswordBreachChecker.Object,
                mockSigninManager.Object,
                Mock.Of<ILogger<RegisterModel>>(),
                Mock.Of<IEmailSender>())
            {
                Input = CreateInputModel("token", password),
            };

            // Act
            var result = await model.OnPostAsync("Return URL");

            // Assert
            Assert.IsType<PageResult>(result);
        }

        private static RegisterModel.InputModel CreateInputModel(string registrationToken, string password) =>
            new RegisterModel.InputModel
            {
                Email = EmailAddress,
                Password = password,
                RegistrationToken = registrationToken
            };
    }
}
