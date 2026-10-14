using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace BIT240058_MID.Models
{
    /// <summary>
    /// Model món ăn - bảng BIT240058_Dishes.
    /// Ràng buộc: tên món không được trùng trong cùng một loại (unique index Name + DishCategoryId).
    /// </summary>
    [Table("BIT240058_Dishes")]
    [Index(nameof(Name), nameof(DishCategoryId), IsUnique = true, Name = "UX_BIT240058_Dishes_Name_Category")]
    public class Dish_BIT240058
    {
        /// <summary>Ngưỡng phân loại: ≤ 15 phút là "Nhanh"</summary>
        public const int QuickMaxMinutes = 15;

        // ----- Khóa chính -----
        [Key]
        [Column("BIT240058_Id")]
        public int Id { get; set; }

        // ----- Tên món: không được để trống -----
        [Required(ErrorMessage = "Dish name is required.")]
        [StringLength(150, ErrorMessage = "Dish name must not exceed 150 characters.")]
        [Display(Name = "Dish name")]
        [Column("BIT240058_Name")]
        public string Name { get; set; } = string.Empty;

        // ----- Giá: phải > 0 (kiểm tra bằng attribute tự viết GreaterThanZero) -----
        [GreaterThanZero(ErrorMessage = "Price must be greater than 0.")]
        [Display(Name = "Price")]
        [Column("BIT240058_Price", TypeName = "decimal(18,0)")]
        public decimal Price { get; set; }

        // ----- Thời gian chế biến (phút): phải > 0 -----
        [GreaterThanZero(ErrorMessage = "Preparation time must be greater than 0.")]
        [Display(Name = "Preparation time (minutes)")]
        [Column("BIT240058_PreparationTime")]
        public int PreparationTime { get; set; }

        // ----- Trạng thái đang bán -----
        [Display(Name = "Available")]
        [Column("BIT240058_IsAvailable")]
        public bool IsAvailable { get; set; } = true;

        // ----- Mô tả -----
        [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        [Display(Name = "Description")]
        [Column("BIT240058_Description")]
        public string? Description { get; set; }

        // ----- Khóa ngoại tới loại món (bắt buộc chọn) -----
        // Ô chọn dùng giá trị 0 cho "-- Chọn loại --", nên Range(1, ...) bắt người dùng phải chọn.
        // Việc loại món có tồn tại trong DB hay không được kiểm tra thêm ở Controller.
        [Range(1, int.MaxValue, ErrorMessage = "Please select a dish category.")]
        [Display(Name = "Dish category")]
        [Column("BIT240058_DishCategoryId")]
        public int DishCategoryId { get; set; }

        // ----- Quan hệ: nhiều Dish thuộc một DishCategory -----
        [ValidateNever]
        public DishCategory_BIT240058? DishCategory { get; set; }

        // ----- Quan hệ: một Dish có nhiều DishImage -----
        [ValidateNever]
        public ICollection<DishImage_BIT240058> Images { get; set; } = new List<DishImage_BIT240058>();

        // ----- Phân loại thời gian chế biến -----
        // Chỉ TÍNH KHI HIỂN THỊ, [NotMapped] => không tạo cột trong database.
        [NotMapped]
        public bool IsQuick => PreparationTime <= QuickMaxMinutes;

        /// <summary>Trả về khóa "Quick"/"Regular", View sẽ dịch thành "Nhanh"/"Thông thường"</summary>
        [NotMapped]
        public string PreparationType => IsQuick ? "Quick" : "Regular";
    }
}
