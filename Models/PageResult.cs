namespace Project02.Models
{
    public class PagedResult<T>
    {
        // Danh sách dữ liệu ở trang hiện tại
        public List<T> Items { get; set; } = new List<T>();

        // Số trang hiện tại (bắt đầu từ 1)
        public int PageIndex { get; set; }

        // Số item mỗi trang
        public int PageSize { get; set; }

        // Tổng số item
        public int TotalItems { get; set; }

        // Tính số trang tối đa
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalItems / PageSize);

        // Có trang trước không?
        public bool HasPrevious => PageIndex > 1;

        // Có trang sau không?
        public bool HasNext => PageIndex < TotalPages;
    }
}
