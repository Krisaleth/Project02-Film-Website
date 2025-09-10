

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Project02.ViewModels.User
{
    public class UserRowVm
    {
        public long Users_ID { get; set; }
        public string Users_FullName { get; set; } = default!;
        public string Users_Email { get; set; } = default!;
        public string Users_Phone { get; set; } = default!;
        public byte[] RowsVersion { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Account_Status { get; set; } = default!;  
    }

    public class UserIndexVm
    {
        public List<UserRowVm> Items { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItem { get; set; }
        public string? search { get; set; }
        public string? sortOrder { get; set; }
        public IEnumerable<SelectListItem>? SortOptions { get; set; }
        public int TotalPage => (int)Math.Ceiling((double)TotalItem / PageSize);
        public int Start => TotalItem == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int End => Math.Min(Page * PageSize, TotalItem);
    }
}
