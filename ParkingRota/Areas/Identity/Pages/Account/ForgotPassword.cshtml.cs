namespace ParkingRota.Areas.Identity.Pages.Account
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Business.Model;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;

        public ForgotPasswordModel(
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (this.ModelState.IsValid)
            {
                // Deliberately slow this method down slightly to avoid leaking information via time taken
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(200)));

                var user = await this.userManager.FindByEmailAsync(this.Input.Email);
                if (user == null || !(await this.userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return this.RedirectToPage("./ForgotPasswordConfirmation");
                }

                var code = await this.userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = this.Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { code },
                    protocol: this.Request.Scheme);

                var encodedCallbackUrl = HtmlEncoder.Default.Encode(callbackUrl);

                var emailBody =
                    "<p>Someone - hopefully you - requested to reset the password associated with this email address on the Parking Rota website.<p>" +
                    $"<p>If this was you, you can do so by <a href='{encodedCallbackUrl}'>clicking here</a>. If not, you can disregard this email.</p>" +
                    $"<p>The request originated from IP address {this.httpContextAccessor.GetOriginatingIpAddress()}</p>";

                await this.emailSender.SendEmailAsync(this.Input.Email, "[Parking Rota] Reset Password", emailBody);

                return this.RedirectToPage("./ForgotPasswordConfirmation");
            }

            return this.Page();
        }
    }
}
