using System.ComponentModel.DataAnnotations;

namespace Cosmetics.DTO.User
{
    public class EditSelfModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Required]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        public string Phone { get; set; }
    }
}