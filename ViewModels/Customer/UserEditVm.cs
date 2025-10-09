
using System.ComponentModel.DataAnnotations;
namespace Project02.ViewModels.Customer
{ 
        public class UserEditVm
        {
            [Required]
            public long User_ID { get; set; }

            [Required(ErrorMessage = "Họ tên không được để trống")]
            [StringLength(255)]
            [Display(Name = "Họ tên")]
            public string User_Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email không được để trống")]
            [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
            [StringLength(255)]
            [Display(Name = "Email")]
            public string User_Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Số điện thoại không được để trống")]
            [StringLength(15)]
            [Display(Name = "Số điện thoại")]
            public string User_Phone { get; set; } = string.Empty;

            // Nếu có RowVersion, thêm field byte[] này để dùng concurrency check
            public byte[]? RowsVersion { get; set; }
        }
    }
