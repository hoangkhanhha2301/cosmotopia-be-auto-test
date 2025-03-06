using System.ComponentModel.DataAnnotations;

namespace Cosmetics.Service.OTP
{
    public class PhoneNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var phoneNumber = value.ToString();
                if (phoneNumber.Length == 10 && phoneNumber.All(char.IsDigit))
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult("Phone number must be exactly 10 digits.");
        }
    }
}

