using System.ComponentModel.DataAnnotations;

namespace Project02.ViewModels.User
{
    public class UserEditVm
    {
        [Required]
        public long User_ID { get; set; }
        [Required]
        public string User_FullName { get; set; } = default!;
        [Required]
        [EmailAddress]
        public string User_Email { get; set; } = default!;
        [Required]
        public string User_Phone { get; set; } = default!;
        [Required]
        public string User_Name { get; set; } = default!;
        [Required]
        public string Account_Status { get; set; } = default!;
        [Required]
        public byte[] RowsVersion { get; set; } = default!;
    }
}
