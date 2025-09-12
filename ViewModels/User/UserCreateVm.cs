using System.ComponentModel.DataAnnotations;

namespace Project02.ViewModels.User
{
    public class UserCreateVm
    {
        [Required]
        public string User_Name { get; set; } = default!;
        [Required]
        public string User_Email { get; set; } = default!;
        [Required]
        public string User_Phone { get; set; } = default!;
        [Required]
        public string UserName { get; set; } = default!;
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = default!;
        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp!")]
        public string ConfirmPassword { get; set; } = default!;
        [Required]
        public string Role { get; set; } = default!;
        [Required]
        public string Account_Status {  get; set; } = "Active";

    }
}
