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
    using Moq;
    using ParkingRota.Business.EmailTemplates;
    using ParkingRota.Business.Model;
    using Xunit;

    public class ForgotPasswordTests
    {
        private const string EmailAddress = "a@b.c";

        [Fact]
        public async Task Test_ForgotPassword_Succeeds()
        {
            // Arrange
            const int IpAddressInt = 0x2414188f;
            const string IpAddressString = "143.24.20.36";

            const string ConfirmEmailUrl = "https://some.url";

            // Set up HTTP context accessor
            var httpContextAccessor = TestHelpers.CreateHttpContextAccessor(IpAddressInt);

            // Set up user manager
            var mockUserManager = CreateMockUserManager(userIsConfirmed: true);

            // Set up email repository
            var otherUserEmail = new EmailQueueItem { To = "Other email address", Subject = "Reset password" };
            var otherSubjectEmail = new EmailQueueItem { To = EmailAddress, Subject = "Other subject" };

            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);
            mockEmailRepository.Setup(e => e.AddToQueue(It.IsAny<ResetPassword>()));
            mockEmailRepository
                .Setup(e => e.GetRecent())
                .Returns(new[] { otherUserEmail, otherSubjectEmail });

            // Set up model
            var httpContext = new DefaultHttpContext();

            var mockUrlHelper = CreateMockUrlHelper(httpContext, ConfirmEmailUrl);

            var model = new ForgotPasswordModel(
                httpContextAccessor,
                mockUserManager.Object,
                mockEmailRepository.Object)
            {
                PageContext = { HttpContext = httpContext },
                Input = new ForgotPasswordModel.InputModel { Email = EmailAddress },
                Url = mockUrlHelper.Object
            };

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./ForgotPasswordConfirmation", ((RedirectToPageResult)result).PageName);

            mockEmailRepository.Verify(p => p.AddToQueue(
                    It.Is<ResetPassword>(e =>
                        e.To == EmailAddress &&
                        e.HtmlBody.Contains(ConfirmEmailUrl, StringComparison.Ordinal) &&
                        e.HtmlBody.Contains(IpAddressString, StringComparison.OrdinalIgnoreCase))),
                Times.Once);
        }

        [Fact]
        public async Task Test_ForgotPassword_InvalidEmailAddress()
        {
            // Arrange
            // Set up user manager
            var mockUserManager = TestHelpers.CreateMockUserManager();
            mockUserManager
                .Setup(u => u.FindByEmailAsync(EmailAddress))
                .Returns(Task.FromResult<ApplicationUser>(null));

            // Set up model
            var model = new ForgotPasswordModel(
                Mock.Of<IHttpContextAccessor>(),
                mockUserManager.Object,
                new Mock<IEmailRepository>(MockBehavior.Strict).Object)
            {
                Input = new ForgotPasswordModel.InputModel { Email = EmailAddress }
            };

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./ForgotPasswordConfirmation", ((RedirectToPageResult)result).PageName);
        }

        [Fact]
        public async Task Test_ForgotPassword_UnconfirmedEmailAddress()
        {
            // Arrange
            // Set up user manager
            var mockUserManager = CreateMockUserManager(userIsConfirmed: false);

            // Set up model
            var model = new ForgotPasswordModel(
                Mock.Of<IHttpContextAccessor>(),
                mockUserManager.Object,
                new Mock<IEmailRepository>(MockBehavior.Strict).Object)
            {
                Input = new ForgotPasswordModel.InputModel { Email = EmailAddress }
            };

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./ForgotPasswordConfirmation", ((RedirectToPageResult)result).PageName);
        }

        [Fact]
        public async Task Test_ForgotPassword_EmailAlreadySent()
        {
            // Arrange
            const int IpAddressInt = 0x2414188f;

            // Set up HTTP context accessor
            var httpContextAccessor = TestHelpers.CreateHttpContextAccessor(IpAddressInt);

            // Set up user manager
            var mockUserManager = CreateMockUserManager(userIsConfirmed: true);

            // Set up email repository
            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);
            mockEmailRepository
                .Setup(e => e.GetRecent())
                .Returns(new[] { new EmailQueueItem { To = EmailAddress, Subject = "Reset password" } });

            // Set up model
            var httpContext = new DefaultHttpContext();

            var mockUrlHelper = CreateMockUrlHelper(httpContext, "https://some.url");

            var model = new ForgotPasswordModel(
                httpContextAccessor,
                mockUserManager.Object,
                mockEmailRepository.Object)
            {
                PageContext = { HttpContext = httpContext },
                Input = new ForgotPasswordModel.InputModel { Email = EmailAddress },
                Url = mockUrlHelper.Object
            };

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./ForgotPasswordConfirmation", ((RedirectToPageResult)result).PageName);
        }

        private static Mock<UserManager<ApplicationUser>> CreateMockUserManager(bool userIsConfirmed)
        {
            var user = new ApplicationUser { Email = EmailAddress };

            var mockUserManager = TestHelpers.CreateMockUserManager();
            mockUserManager
                .Setup(u => u.FindByEmailAsync(EmailAddress))
                .Returns(Task.FromResult(user));
            mockUserManager
                .Setup(u => u.IsEmailConfirmedAsync(user))
                .Returns(Task.FromResult(userIsConfirmed));

            if (userIsConfirmed)
            {
                mockUserManager
                    .Setup(u => u.GeneratePasswordResetTokenAsync(user))
                    .Returns(Task.FromResult("[Reset code]"));
            }

            return mockUserManager;
        }

        private static Mock<UrlHelper> CreateMockUrlHelper(HttpContext httpContext, string confirmEmailUrl)
        {
            var actionContext = new ActionContext(
                httpContext, new RouteData(), new PageActionDescriptor(), new ModelStateDictionary());

            var mockUrlHelper = new Mock<UrlHelper>(actionContext);
            mockUrlHelper
                .Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(confirmEmailUrl);

            return mockUrlHelper;
        }
    }
}
