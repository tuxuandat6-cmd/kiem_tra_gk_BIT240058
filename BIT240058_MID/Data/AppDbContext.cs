using BIT240058_MID.Models;
using Microsoft.EntityFrameworkCore;

namespace BIT240058_MID.Data
{
    /// <summary>
    /// DbContext của Entity Framework Core (ORM) - ánh xạ 3 Model vào 3 bảng SQL Server.
    /// Tên bảng/cột có tiền tố MSSV đã khai báo bằng [Table], [Column] trong từng Model.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ----- 3 bảng trong database -----
        public DbSet<DishCategory_BIT240058> DishCategories => Set<DishCategory_BIT240058>();
        public DbSet<Dish_BIT240058> Dishes => Set<Dish_BIT240058>();
        public DbSet<DishImage_BIT240058> DishImages => Set<DishImage_BIT240058>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----- Quan hệ 1-n: DishCategory -> Dish -----
            // Restrict: database KHÔNG cho xóa loại món khi vẫn còn món thuộc loại đó
            modelBuilder.Entity<Dish_BIT240058>()
                .HasOne(d => d.DishCategory)
                .WithMany(c => c.Dishes)
                .HasForeignKey(d => d.DishCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ----- Quan hệ 1-n: Dish -> DishImage -----
            // Cascade: xóa món thì xóa luôn các ảnh của món đó
            modelBuilder.Entity<DishImage_BIT240058>()
                .HasOne(i => i.Dish)
                .WithMany(d => d.Images)
                .HasForeignKey(i => i.DishId)
                .OnDelete(DeleteBehavior.Cascade);

            // ----- Mỗi món tối đa MỘT ảnh đại diện -----
            // Unique index có điều kiện (filtered index): chỉ áp dụng cho các dòng IsThumbnail = 1
            modelBuilder.Entity<DishImage_BIT240058>()
                .HasIndex(i => i.DishId)
                .IsUnique()
                .HasFilter("[BIT240058_IsThumbnail] = 1")
                .HasDatabaseName("UX_BIT240058_DishImages_OneThumbnail");
        }
    }
}
