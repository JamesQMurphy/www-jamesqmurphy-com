using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Models.ContactViewModels
{
    public class IndexViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter at least 30 characters of text.")]
        [MinLength(30, ErrorMessage = "Please enter at least 30 characters of text.")]
        [Display(Name = "Comments")]
        public string Comments { get; set; }
    }
}
