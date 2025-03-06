using System.ComponentModel.DataAnnotations;

namespace Cosmetics.Service.OTP
{
    public class VerifyOtpModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}
