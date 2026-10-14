using BIT240058_MID.Models;

namespace BIT240058_MID.Data
{
    /// <summary>
    /// Tạo database + dữ liệu mẫu khi chạy lần đầu:
    /// 3 loại món, 6 món (có món ngừng bán và món chưa có ảnh để demo), nhiều ảnh.
    /// </summary>
    public static class SeedData
    {
        public static void Initialize(AppDbContext db)
        {
            // Tạo database BIT240058_MID và 3 bảng nếu chưa có
            db.Database.EnsureCreated();

            // Đã có dữ liệu thì không thêm nữa
            if (db.DishCategories.Any()) return;

            // ----- 3 loại món ăn -----
            var main = new DishCategory_BIT240058 { Name = "Món chính", Description = "Các món ăn no, dùng cho bữa chính" };
            var starter = new DishCategory_BIT240058 { Name = "Món khai vị", Description = "Món ăn nhẹ trước bữa chính" };
            var drink = new DishCategory_BIT240058 { Name = "Đồ uống", Description = "Nước giải khát, sinh tố, trà" };
            db.DishCategories.AddRange(main, starter, drink);

            // ----- 6 món ăn, mỗi món thuộc đúng một loại -----
            db.Dishes.AddRange(
                Dish("Phở bò", 55000, 20, true, "Phở bò tái chín, nước dùng hầm xương 8 tiếng.", main,
                    "https://picsum.photos/seed/phobo/640/420", "https://picsum.photos/seed/phobo2/640/420"),
                Dish("Cơm tấm sườn", 45000, 15, true, "Cơm tấm sườn nướng, bì, chả trứng.", main,
                    "https://picsum.photos/seed/comtam/640/420", "https://picsum.photos/seed/comtam2/640/420",
                    "https://picsum.photos/seed/comtam3/640/420"),
                Dish("Gỏi cuốn", 35000, 10, true, "Gỏi cuốn tôm thịt chấm tương đậu.", starter,
                    "https://picsum.photos/seed/goicuon/640/420"),
                Dish("Chả giò", 40000, 25, false, "Chả giò chiên giòn nhân thịt và khoai môn.", starter,
                    "https://picsum.photos/seed/chagio/640/420"),
                Dish("Trà đào", 30000, 5, true, "Trà đào cam sả.", drink,
                    "https://picsum.photos/seed/tradao/640/420", "https://picsum.photos/seed/tradao2/640/420"),
                // Món chưa có ảnh -> dùng để demo trường hợp "món chưa có ảnh"
                Dish("Sinh tố bơ", 35000, 7, false, "Sinh tố bơ sữa đặc.", drink)
            );

            db.SaveChanges();
        }

        // Hàm tiện ích tạo món ăn; ảnh đầu tiên được đặt làm ảnh đại diện
        private static Dish_BIT240058 Dish(string name, decimal price, int minutes, bool available,
                                          string desc, DishCategory_BIT240058 category, params string[] imageUrls)
        {
            var dish = new Dish_BIT240058
            {
                Name = name,
                Price = price,
                PreparationTime = minutes,
                IsAvailable = available,
                Description = desc,
                DishCategory = category
            };
            for (int i = 0; i < imageUrls.Length; i++)
            {
                dish.Images.Add(new DishImage_BIT240058 { ImageUrl = imageUrls[i], IsThumbnail = i == 0 });
            }
            return dish;
        }
    }
}
