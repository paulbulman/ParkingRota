namespace ParkingRota.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Areas.Identity.Pages.Account;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Moq;
    using ParkingRota.Business.Emails;
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
            var mockHttpContextAccessor = TestHelpers.CreateMockHttpContextAccessor(IpAddressInt);

            // Set up user manager
            var user = new ApplicationUser { Email = EmailAddress };

            var mockUserManager = TestHelpers.CreateMockUserManager();
            mockUserManager
                .Setup(u => u.FindByEmailAsync(EmailAddress))
                .Returns(Task.FromResult(user));
            mockUserManager
                .Setup(u => u.IsEmailConfirmedAsync(user))
                .Returns(Task.FromResult(true));
            mockUserManager
                .Setup(u => u.GeneratePasswordResetTokenAsync(user))
                .Returns(Task.FromResult("[Reset code]"));

            // Set up email repository
            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);
            mockEmailRepository.Setup(e => e.AddToQueue(It.IsAny<ResetPassword>()));
            mockEmailRepository
                .Setup(e => e.GetRecent())
                .Returns(new List<EmailQueueItem>());

            // Set up model
            var httpContext = new DefaultHttpContext();

            var actionContext = new ActionContext(
                httpContext, new RouteData(), new PageActionDescriptor(), new ModelStateDictionary());

            var mockUrlHelper = new Mock<UrlHelper>(actionContext);
            mockUrlHelper
                .Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(ConfirmEmailUrl);

            var model = new ForgotPasswordModel(
                mockHttpContextAccessor.Object,
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
            var user = new ApplicationUser { Email = EmailAddress };

            var mockUserManager = TestHelpers.CreateMockUserManager();
            mockUserManager
                .Setup(u => u.FindByEmailAsync(EmailAddress))
                .Returns(Task.FromResult(user));
            mockUserManager
                .Setup(u => u.IsEmailConfirmedAsync(user))
                .Returns(Task.FromResult(false));

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
            var mockHttpContextAccessor = TestHelpers.CreateMockHttpContextAccessor(IpAddressInt);

            // Set up user manager
            var user = new ApplicationUser { Email = EmailAddress };

            var mockUserManager = TestHelpers.CreateMockUserManager();
            mockUserManager
                .Setup(u => u.FindByEmailAsync(EmailAddress))
                .Returns(Task.FromResult(user));
            mockUserManager
                .Setup(u => u.IsEmailConfirmedAsync(user))
                .Returns(Task.FromResult(true));
            mockUserManager
                .Setup(u => u.GeneratePasswordResetTokenAsync(user))
                .Returns(Task.FromResult("[Reset code]"));

            // Set up email repository
            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);
            mockEmailRepository
                .Setup(e => e.GetRecent())
                .Returns(new[] { new EmailQueueItem { To = EmailAddress, Subject = "Reset password" } });

            // Set up model
            var httpContext = new DefaultHttpContext();

            var actionContext = new ActionContext(
                httpContext, new RouteData(), new PageActionDescriptor(), new ModelStateDictionary());

            var mockUrlHelper = new Mock<UrlHelper>(actionContext);
            mockUrlHelper
                .Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns("https://some.url");

            var model = new ForgotPasswordModel(
                mockHttpContextAccessor.Object,
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
    }
}
