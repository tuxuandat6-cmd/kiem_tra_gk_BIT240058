using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BIT240058_MID.Models
{
    /// <summary>
    /// Model loại món ăn.
    /// Tên bảng và tên TẤT CẢ cột trong database đều có tiền tố MSSV "BIT240058_".
    /// </summary>
    [Table("BIT240058_DishCategories")]
    public class DishCategory_BIT240058
    {
        // ----- Khóa chính -----
        [Key]
        [Column("BIT240058_Id")]
        public int Id { get; set; }

        // ----- Tên loại món: bắt buộc, tối đa 100 ký tự -----
        // ErrorMessage / Display.Name là "khóa" tiếng Anh, sẽ được dịch sang tiếng Việt
        // qua file Resources/SharedResource.vi.resx
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, ErrorMessage = "Category name must not exceed 100 characters.")]
        [Display(Name = "Category name")]
        [Column("BIT240058_Name")]
        public string Name { get; set; } = string.Empty;

        // ----- Mô tả (không bắt buộc) -----
        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        [Display(Name = "Description")]
        [Column("BIT240058_Description")]
        public string? Description { get; set; }

        // ----- Quan hệ: Một DishCategory có nhiều Dish -----
        [ValidateNever]
        public ICollection<Dish_BIT240058> Dishes { get; set; } = new List<Dish_BIT240058>();
    }
}
