using System.ComponentModel.DataAnnotations;

namespace Cosmetics.DTO.User
{
    public class ChangePasswordModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}
