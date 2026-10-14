using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BIT240058_MID.Models
{
    /// <summary>
    /// Model ảnh món ăn - bảng BIT240058_DishImages.
    /// Mỗi món tối đa MỘT ảnh có IsThumbnail = true
    /// (được đảm bảo bởi code ở Controller + unique index có điều kiện trong AppDbContext).
    /// </summary>
    [Table("BIT240058_DishImages")]
    public class DishImage_BIT240058
    {
        [Key]
        [Column("BIT240058_Id")]
        public int Id { get; set; }

        // ----- Đường dẫn ảnh (URL) -----
        [Required(ErrorMessage = "Image URL is required.")]
        [StringLength(1000)]
        [Column("BIT240058_ImageUrl")]
        public string ImageUrl { get; set; } = string.Empty;

        // ----- Có phải ảnh đại diện không -----
        [Column("BIT240058_IsThumbnail")]
        public bool IsThumbnail { get; set; }

        // ----- Khóa ngoại tới món ăn -----
        [Column("BIT240058_DishId")]
        public int DishId { get; set; }

        [ValidateNever]
        public Dish_BIT240058? Dish { get; set; }
    }
}
