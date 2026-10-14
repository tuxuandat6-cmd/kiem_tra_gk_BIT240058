using BIT240058_MID.Models;

namespace BIT240058_MID.ViewModels
{
    /// <summary>Một dòng trên trang danh sách món (lấy bằng 1 câu SELECT có JOIN)</summary>
    public class DishListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int PreparationTime { get; set; }
        public bool IsAvailable { get; set; }
        public string? ThumbnailUrl { get; set; }

        // Phân loại thời gian chế biến - tính khi hiển thị, không lưu DB
        public bool IsQuick => PreparationTime <= Dish_BIT240058.QuickMaxMinutes;
    }

    /// <summary>Dữ liệu truyền cho View danh sách món ăn</summary>
    public class DishIndexViewModel
    {
        public DishFilter Filter { get; set; } = new();
        public List<DishListItem> Dishes { get; set; } = new();
        public List<DishCategory_BIT240058> Categories { get; set; } = new();
        public List<string> Errors { get; set; } = new();   // lỗi bộ lọc (đã dịch)
    }

    /// <summary>Một dòng trên trang danh sách loại món (kèm số món)</summary>
    public class CategoryListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DishCount { get; set; }
    }
}
