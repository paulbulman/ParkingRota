namespace ParkingRota.Business.Model
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Identity;

    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Car registration number")]
        public string CarRegistrationNumber { get; set; }

        public decimal CommuteDistance { get; set; }
    }
}