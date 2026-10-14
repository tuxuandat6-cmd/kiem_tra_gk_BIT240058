namespace BIT240058_MID.ViewModels
{
    /// <summary>
    /// Các điều kiện tìm kiếm / lọc / sắp xếp trên trang danh sách món ăn.
    /// Được bind tự động từ query string, ví dụ: /Dishes?keyword=pho&amp;categoryId=1&amp;sort=price_asc
    /// </summary>
    public class DishFilter
    {
        public const string StatusAvailable = "available";
        public const string StatusUnavailable = "unavailable";

        public const string SortPriceAsc = "price_asc";
        public const string SortPriceDesc = "price_desc";
        public const string SortTimeAsc = "time_asc";

        public string? Keyword { get; set; }      // tìm theo một phần tên món
        public int? CategoryId { get; set; }      // lọc theo loại món
        public string? Status { get; set; }       // available / unavailable
        public decimal? MinPrice { get; set; }    // giá thấp nhất
        public decimal? MaxPrice { get; set; }    // giá cao nhất
        public string? Sort { get; set; }         // kiểu sắp xếp
    }
}
