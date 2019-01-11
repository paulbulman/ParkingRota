namespace ParkingRota.UnitTests
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Areas.Identity.Pages.Account.Manage;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;
    using Moq;
    using ParkingRota.Business.Model;
    using Xunit;

    public class ChangePasswordTests
    {
        [Theory]
        [InlineData("An old password", "An unbreached new password")]
        [InlineData("Another old password", "Another unbreached new password")]
        public async Task Test_ChangePassword_Succeeds(string oldPassword, string newPassword)
        {
            // Arrange
            var principal = new ClaimsPrincipal();
            var loggedInUser = new ApplicationUser();

            // Set up user manager
            var mockUserManager = TestHelpers.CreateMockUserManager(principal, loggedInUser);

            mockUserManager
                .Setup(u => u.ChangePasswordAsync(loggedInUser, oldPassword, newPassword))
                .Returns(Task.FromResult(IdentityResult.Success));

            // Set up sign in manager
            var mockSigninManager = TestHelpers.CreateMockSigninManager(mockUserManager.Object);

            // Set up password breach checker
            var passwordBreachChecker = TestHelpers.CreatePasswordBreachChecker(newPassword, isBreached: false);

            // Set up model
            var model = new ChangePasswordModel(
                mockUserManager.Object,
                mockSigninManager.Object,
                passwordBreachChecker,
                Mock.Of<ILogger<ChangePasswordModel>>())
            {
                PageContext = { HttpContext = new DefaultHttpContext { User = principal } },
                Input = new ChangePasswordModel.InputModel { OldPassword = oldPassword, NewPassword = newPassword }
            };

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("Your password has been changed.", model.StatusMessage);
        }

        [Theory]
        [InlineData("A password")]
        [InlineData("Another password")]
        public async Task Test_ChangePassword_BreachedPassword(string password)
        {
            // Arrange
            var principal = new ClaimsPrincipal();
            var loggedInUser = new ApplicationUser();

            // Set up user manager
            var mockUserManager = TestHelpers.CreateMockUserManager(principal, loggedInUser);

            // Set up sign in manager
            var mockSigninManager = TestHelpers.CreateMockSigninManager(mockUserManager.Object);

            // Set up password breach checker
            var passwordBreachChecker = TestHelpers.CreatePasswordBreachChecker(password, isBreached: true);

            // Set up model
            var model = new ChangePasswordModel(
                mockUserManager.Object,
                mockSigninManager.Object,
                passwordBreachChecker,
                Mock.Of<ILogger<ChangePasswordModel>>())
            {
                PageContext = { HttpContext = new DefaultHttpContext { User = principal } },
                Input = new ChangePasswordModel.InputModel { NewPassword = password }
            };

            // Act
            var result = await model.OnPostAsync();

            // Assert
            Assert.IsType<PageResult>(result);
        }
    }
}