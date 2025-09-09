namespace Project02.ViewModels.User
{
    public class UserCreateVm
    {
        public string User_Name { get; set; } = default!;
        public string User_Email { get; set; } = default!;
        public string User_Phone { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
        public string Account_Status {  get; set; } = "Active";

    }
}
