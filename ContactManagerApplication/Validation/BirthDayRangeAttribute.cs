using System.ComponentModel.DataAnnotations;

namespace ContactManagerApplication.Validation
{
    /// <summary>
    /// Validates that a <see cref="DateOnly"/> date of birth falls within an acceptable
    /// employment age range: at least <see cref="MinAgeYears"/> years old and no older
    /// than <see cref="MaxAgeYears"/> years.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class BirthDayRangeAttribute : ValidationAttribute
    {
        public const int MinAgeYears = 14;
        public const int MaxAgeYears = 100;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Field is optional — skip when empty
            if (value is null)
                return ValidationResult.Success;

            if (value is not DateOnly birthDay)
                return new ValidationResult("Invalid date format.");

            var today    = DateOnly.FromDateTime(DateTime.Today);
            var earliest = today.AddYears(-MaxAgeYears);   // oldest acceptable DOB
            var latest   = today.AddYears(-MinAgeYears);   // youngest acceptable DOB

            if (birthDay > today)
                return new ValidationResult("Date of birth cannot be in the future.");

            if (birthDay > latest)
                return new ValidationResult(
                    $"Employee must be at least {MinAgeYears} years old " +
                    $"(born on or before {latest:dd/MM/yyyy}).");

            if (birthDay < earliest)
                return new ValidationResult(
                    $"Date of birth cannot be earlier than {earliest:dd/MM/yyyy} " +
                    $"({MaxAgeYears} years ago).");

            return ValidationResult.Success;
        }
    }
}
