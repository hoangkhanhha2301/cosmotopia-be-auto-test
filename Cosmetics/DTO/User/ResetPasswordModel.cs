namespace Cosmetics.DTO.User
{
    public class ResetPasswordModel
    {

        public string Email { get; set; }
    }

    public class SetNewPasswordModel
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}