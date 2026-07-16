using System.ComponentModel.DataAnnotations;
using ContactManagerApplication.Validation;

namespace ContactManagerApplication.Models.ViewModel
{
    public class CreateEmployeeVM
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Name must be between 2 and 100 characters.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Display(Name = "Date of Birth")]
        [BirthDayRange]
        public DateOnly? BirthDay { get; set; }

        [Display(Name = "Married")]
        public bool? IsMarried { get; set; }

        [Display(Name = "Phone Number")]
        [StringLength(15, MinimumLength = 7,
            ErrorMessage = "Phone number must be between 7 and 15 characters.")]
        [RegularExpression(@"^\+?[\d\s\(\)\-]+$",
            ErrorMessage = "Phone can only contain digits, spaces, +, hyphens, and parentheses.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Salary is required.")]
        [Range(0, 9_999_999.99,
            ErrorMessage = "Salary must be between 0 and 9,999,999.99.")]
        [Display(Name = "Salary")]
        public decimal Salary { get; set; }
    }
}
