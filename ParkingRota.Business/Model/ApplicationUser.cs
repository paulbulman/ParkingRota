namespace ParkingRota.Business.Model
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Identity;

    public class ApplicationUser : IdentityUser
    {
        [ProtectedPersonalData]
        [Required]
        [StringLength(50)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [ProtectedPersonalData]
        [Required]
        [StringLength(50)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [ProtectedPersonalData]
        [Required]
        [StringLength(10)]
        [Display(Name = "Car registration number")]
        public string CarRegistrationNumber { get; set; }

        [ProtectedPersonalData]
        [StringLength(10)]
        [Display(Name = "Alternative car registration number")]
        public string AlternativeCarRegistrationNumber { get; set; }

        [PersonalData]
        public decimal CommuteDistance { get; set; }

        public string FullName => $"{this.FirstName} {this.LastName}";
    }
}