using JamesQMurphy.Auth;
using System.ComponentModel.DataAnnotations;

namespace JamesQMurphy.Web.Models.AccountViewModels
{
    public class RegisterUsernameViewModel
    {
        [Required]
        [StringLength(25, ErrorMessage = "{0} must be at least {2} and at most {1} characters long.", MinimumLength = 3)]
        [RegularExpression(@"^\w+$", ErrorMessage = "{0} can only contain letters, numbers, and underscores (_).")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        public bool IsTrue => true;

        [Required]
        [Compare(nameof(IsTrue), ErrorMessage = "Please acknowledge the Privacy Policy")]
        public bool AcceptPrivacy { get; set; }
    }
}
