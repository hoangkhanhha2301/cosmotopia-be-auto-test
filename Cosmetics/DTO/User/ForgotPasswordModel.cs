using System.ComponentModel.DataAnnotations;

namespace Cosmetics.DTO.User
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
