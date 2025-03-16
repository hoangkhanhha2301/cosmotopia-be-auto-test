namespace Cosmetics.DTO.User.Admin
{
    public class UserAdminDTO
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int RoleType { get; set; }
        public string? RoleName { get; set; }
        public string UserStatus { get; set; }
    }
}
