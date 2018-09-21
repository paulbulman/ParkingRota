namespace ParkingRota.UnitTests
{
    using System.Threading.Tasks;
    using Areas.Identity.Pages.Account;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Moq;
    using ParkingRota.Business;
    using Xunit;

    public class ResetPasswordTests
    {
        [Theory]
        [InlineData("An unbreached password")]
        [InlineData("Another unbreached password")]
        public async Task Test_ResetPassword_Succeeds(string password)
        {
            // Arrange
            var model = CreateModel(password, passwordIsBreached: false);

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./ResetPasswordConfirmation", ((RedirectToPageResult)result).PageName);
        }

        [Theory]
        [InlineData("A password")]
        [InlineData("Another password")]
        public async Task Test_ResetPassword_BreachedPassword(string password)
        {
            // Arrange
            var model = CreateModel(password, passwordIsBreached: true);

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<PageResult>(result);
        }

        private static ResetPasswordModel CreateModel(string password, bool passwordIsBreached)
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

            var mockPasswordBreachChecker = new Mock<IPasswordBreachChecker>(MockBehavior.Strict);
            mockPasswordBreachChecker
                .Setup(c => c.PasswordIsBreached(password))
                .Returns(Task.FromResult(passwordIsBreached));

            return new ResetPasswordModel(mockUserManager.Object, mockPasswordBreachChecker.Object)
            {
                Input = new ResetPasswordModel.InputModel { Email = "a@b.c", Password = password }
            };
        }
    }
}