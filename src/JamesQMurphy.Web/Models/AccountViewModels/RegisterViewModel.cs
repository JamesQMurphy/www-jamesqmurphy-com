using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(25, ErrorMessage = "{0} must be at least {2} and at most {1} characters long.", MinimumLength = 3)]
        [RegularExpression(@"^\w+$", ErrorMessage = "{0} can only contain letters, numbers, and underscores (_).")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [RegularExpression(
            Services.ApplicationPasswordValidator<ApplicationUser>.REGEX_PATTERN,
            ErrorMessage = Services.ApplicationPasswordValidator<ApplicationUser>.REGEX_DESCRIPTION
            )]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public bool IsTrue => true;

        [Required]
        [Compare(nameof(IsTrue), ErrorMessage = "Please acknowledge the Privacy Policy")]
        public bool AcceptPrivacy { get; set; }
    }
}
