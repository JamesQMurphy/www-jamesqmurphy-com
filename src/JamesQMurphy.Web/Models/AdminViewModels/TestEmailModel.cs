using System;
using System.ComponentModel.DataAnnotations;


namespace JamesQMurphy.Web.Models.AdminViewModels
{
    public class TestEmailModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Message { get; set; }
    }
}
