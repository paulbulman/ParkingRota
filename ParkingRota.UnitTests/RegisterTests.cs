namespace ParkingRota.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using Areas.Identity.Pages.Account;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Moq;
    using ParkingRota.Business;
    using ParkingRota.Business.EmailTemplates;
    using ParkingRota.Business.Model;
    using Xunit;

    public class RegisterTests
    {
        private const string EmailAddress = "a@b.c";

        [Theory]
        [InlineData("A valid registration token", "An unbreached password")]
        [InlineData("Another valid registration token", "Another unbreached password")]
        public async Task Test_Register_Succeeds(string registrationToken, string password)
        {
            const int IpAddressInt = 0x2414188f;
            const string IpAddressString = "143.24.20.36";

            const string ConfirmEmailUrl = "[Confirm email URL]";

            const string RegistersuccessPageName = "/RegisterSuccess";

            // Arrange
            // Set up HTTP context accessor
            var httpContextAccessor = TestHelpers.CreateHttpContextAccessor(IpAddressInt);

            // Set up user manager
            var mockUserManager = TestHelpers.CreateMockUserManager();
            mockUserManager
                .Setup(f => f.CreateAsync(It.IsAny<ApplicationUser>(), password))
                .Returns(Task.FromResult(IdentityResult.Success))
                .Callback<ApplicationUser, string>((user, p) => user.Id = "[New user Id]");
            mockUserManager
                .Setup(u => u.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .Returns(Task.FromResult("[Confirm email token]"));

            // Set up registration token repository
            var mockRegistrationTokenRepository = new Mock<IRegistrationTokenRepository>(MockBehavior.Strict);
            mockRegistrationTokenRepository.Setup(r => r.DeleteRegistrationToken(registrationToken));

            // Set up registration token validator
            var mockRegistrationTokenValidator = new Mock<IRegistrationTokenValidator>(MockBehavior.Strict);
            mockRegistrationTokenValidator.Setup(v => v.TokenIsValid(registrationToken)).Returns(true);

            // Set up password breach checker
            var passwordBreachChecker = TestHelpers.CreatePasswordBreachChecker(password, isBreached: false);

            // Set up email repository
            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);
            mockEmailRepository.Setup(e => e.AddToQueue(It.IsAny<ConfirmEmailAddress>()));

            // Set up model
            var httpContext = new DefaultHttpContext();

            var actionContext = new ActionContext(
                httpContext, new RouteData(), new PageActionDescriptor(), new ModelStateDictionary());

            var mockUrlHelper = new Mock<UrlHelper>(actionContext);
            mockUrlHelper
                .Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(ConfirmEmailUrl);

            var model = new RegisterModel(
                httpContextAccessor,
                mockUserManager.Object,
                mockRegistrationTokenRepository.Object,
                mockRegistrationTokenValidator.Object,
                passwordBreachChecker,
                Mock.Of<ILogger<RegisterModel>>(),
                mockEmailRepository.Object)
            {
                PageContext = { HttpContext = httpContext },
                Input = CreateInputModel(registrationToken, password),
                Url = mockUrlHelper.Object
            };

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal(RegistersuccessPageName, ((RedirectToPageResult)result).PageName);

            mockRegistrationTokenRepository.Verify(
                r => r.DeleteRegistrationToken(registrationToken),
                Times.Once);

            mockEmailRepository.Verify(
                p => p.AddToQueue(
                    It.Is<ConfirmEmailAddress>(e =>
                        e.To == EmailAddress &&
                        e.HtmlBody.Contains(ConfirmEmailUrl, StringComparison.Ordinal) &&
                        e.HtmlBody.Contains(IpAddressString, StringComparison.OrdinalIgnoreCase))),
                Times.Once);
        }

        [Theory]
        [InlineData("A registration token")]
        [InlineData("Another registration token")]
        public async Task Test_Register_InvalidToken(string registrationToken)
        {
            // Arrange
            // Set up user manager
            var mockUserManager = TestHelpers.CreateMockUserManager();

            // Set up registration token validator
            var mockRegistrationTokenValidator = new Mock<IRegistrationTokenValidator>(MockBehavior.Strict);
            mockRegistrationTokenValidator.Setup(v => v.TokenIsValid(registrationToken)).Returns(false);

            // Set up model
            var model = new RegisterModel(
                Mock.Of<IHttpContextAccessor>(),
                mockUserManager.Object,
                Mock.Of<IRegistrationTokenRepository>(),
                mockRegistrationTokenValidator.Object,
                Mock.Of<IPasswordBreachChecker>(),
                Mock.Of<ILogger<RegisterModel>>(),
                Mock.Of<IEmailRepository>())
            {
                Input = CreateInputModel(registrationToken, "password")
            };

            // Act
            var result = await model.OnPostAsync();

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
            var mockUserManager = TestHelpers.CreateMockUserManager();

            // Set up registration token validator
            var mockRegistrationTokenValidator = new Mock<IRegistrationTokenValidator>(MockBehavior.Strict);
            mockRegistrationTokenValidator.Setup(v => v.TokenIsValid(It.IsAny<string>())).Returns(true);

            // Set up password breach checker
            var passwordBreachChecker = TestHelpers.CreatePasswordBreachChecker(password, isBreached: true);

            // Set up model
            var model = new RegisterModel(
                Mock.Of<IHttpContextAccessor>(),
                mockUserManager.Object,
                Mock.Of<IRegistrationTokenRepository>(),
                mockRegistrationTokenValidator.Object,
                passwordBreachChecker,
                Mock.Of<ILogger<RegisterModel>>(),
                Mock.Of<IEmailRepository>())
            {
                Input = CreateInputModel("token", password),
            };

            // Act
            var result = await model.OnPostAsync();

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
