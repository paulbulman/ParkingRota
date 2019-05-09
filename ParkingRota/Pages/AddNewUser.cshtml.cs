namespace ParkingRota.Pages
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    public class AddNewUserModel : PageModel
    {
        [TempData]
        public string StatusMessage { get; set; }

        public void OnPost()
        {
            this.Input.Email = string.Empty;
            this.Input.ConfirmEmail = string.Empty;

            this.StatusMessage = "Token will be sent.";
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [EmailAddress]
            [Display(Name = "Confirm email")]
            [Compare("Email", ErrorMessage = "The email and confirmation email do not match.")]
            public string ConfirmEmail { get; set; }
        }
    }
}