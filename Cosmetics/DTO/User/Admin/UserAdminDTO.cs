namespace Cosmetics.DTO.User.Admin
{
    public class UserAdminDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string RoleType { get; set; }
        public string UserStatus { get; set; }
    }
}
