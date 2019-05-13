namespace ParkingRota.UnitTests.Pages.Users
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Moq;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Emails;
    using ParkingRota.Business.Model;
    using ParkingRota.Pages.Users;
    using Xunit;

    public static class CreateModelTests
    {
        [Fact]
        public static void Test_AddNewUser_Succeeds()
        {
            const int IpAddressInt = 0x2414188f;
            const string IpAddressString = "143.24.20.36";

            const string SignupUrl = "[Signup URL]";

            var currentInstant = 9.May(2019).At(10, 14, 10).Utc();
            var expectedExpiryInstant = 10.May(2019).At(10, 14, 10).Utc();

            // Arrange
            // Set up clock
            var mockClock = new FakeClock(currentInstant);

            // Set up email repository
            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);
            mockEmailRepository.Setup(r => r.AddToQueue(It.IsAny<IEmail>()));

            // Set up HTTP context accessor
            var httpContextAccessor = TestHelpers.CreateHttpContextAccessor(IpAddressInt);

            // Set up registration token repository
            var mockRegistrationTokenRepository = new Mock<IRegistrationTokenRepository>(MockBehavior.Strict);
            mockRegistrationTokenRepository.Setup(r => r.AddRegistrationToken(It.IsAny<RegistrationToken>()));

            // Set up model
            var httpContext = new DefaultHttpContext();

            var actionContext = new ActionContext(
                httpContext, new RouteData(), new PageActionDescriptor(), new ModelStateDictionary());

            var mockUrlHelper = new Mock<UrlHelper>(actionContext);
            mockUrlHelper
                .Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(SignupUrl);

            // Act
            const string EmailAddress = "a@b.c";

            var model = new CreateModel(
                mockClock,
                mockEmailRepository.Object,
                httpContextAccessor,
                mockRegistrationTokenRepository.Object)
            {
                Input = new CreateModel.InputModel { Email = EmailAddress, ConfirmEmail = EmailAddress },
                Url = mockUrlHelper.Object
            };

            model.OnPost();

            // Assert
            mockEmailRepository.Verify(
                r => r.AddToQueue(It.Is<Signup>(e =>
                    e.To == EmailAddress &&
                    e.HtmlBody.Contains(SignupUrl) && e.HtmlBody.Contains(IpAddressString) &&
                    e.PlainTextBody.Contains(SignupUrl) && e.PlainTextBody.Contains(IpAddressString))),
                Times.Once);

            mockRegistrationTokenRepository.Verify(
                r => r.AddRegistrationToken(It.Is<RegistrationToken>(t =>
                    !string.IsNullOrEmpty(t.Token) &&
                    t.ExpiryTime == expectedExpiryInstant)),
                Times.Once);

            Assert.Equal("Email will be sent.", model.StatusMessage);
        }
    }
}