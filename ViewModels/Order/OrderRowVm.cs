using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace Project02.ViewModels.Order
{
    public class OrderRowVm
    {
        public long Order_ID { get; set; }
        public long User_ID { get; set; }
        public string UserName { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public string OrderDateString => OrderDate.ToString("dd/MM/yyyy h:mm tt", new CultureInfo("vi-VN"));
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
    }

    public class OrderIndexVm
    {
        public List<OrderRowVm> Orders { get; set; } = new List<OrderRowVm>();
        public int Page { get; set; } = 1;
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? SearchString { get; set; }
        public string? SortOrder { get; set; }
        public IEnumerable<SelectListItem>? SortOptions { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int EndPage => (CurrentPage - 1) * PageSize + Orders.Count;
        public int StartPage => (CurrentPage - 1) * PageSize + 1;
    }
}
