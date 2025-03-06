using System.ComponentModel.DataAnnotations;

namespace Cosmetics.Service.OTP
{
    public class SendOtpModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


        [Required]
        [PhoneNumber]
        public string Phone { get; set; }

        public DateTime OtpExpiration { get; set; }
    }
}
