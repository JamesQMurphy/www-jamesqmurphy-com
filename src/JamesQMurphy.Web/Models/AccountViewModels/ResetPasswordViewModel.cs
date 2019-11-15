using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Models.AccountViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [RegularExpression(
            Services.ApplicationPasswordValidator<ApplicationUser>.REGEX_PATTERN,
            ErrorMessage = Services.ApplicationPasswordValidator<ApplicationUser>.REGEX_DESCRIPTION
            )]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
        public string Username { get; set; }
    }
}
