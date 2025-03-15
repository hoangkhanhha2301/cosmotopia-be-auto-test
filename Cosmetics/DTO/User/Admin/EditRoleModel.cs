using System.ComponentModel.DataAnnotations;

namespace Cosmetics.DTO.User.Admin
{
    public class EditRoleModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [Range(0, 4, ErrorMessage = "RoleType must be between 0 and 4")]
        public int RoleType { get; set; }
    }

}
