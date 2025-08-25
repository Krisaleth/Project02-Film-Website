using System.ComponentModel.DataAnnotations;

namespace Project02.ViewModels;
public class AdminLoginVm
{
    [Required(ErrorMessage = "Nhập tên đăng nhập")]
    public string UserName { get; set; } = default!;

    [Required(ErrorMessage = "Nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;

    public string? ReturnUrl { get; set; }
    public bool RememberMe { get; set; }
}
