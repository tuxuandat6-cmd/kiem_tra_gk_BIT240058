# Quản lý món ăn — ASP.NET Core MVC + EF Core + SQL Server

MSSV: **BIT240058** — Database: **BIT240058_MID**

## 1. Cách chạy

Yêu cầu: .NET SDK 8 (kiểm tra bằng `dotnet --list-sdks`), SQL Server chạy ở `localhost` (Windows Authentication).

**Cách A: Visual Studio 2022**
Mở file `BIT240058_MID.csproj`, rồi bấm F5 (hoặc nút ▶).

**Cách B: VS Code / terminal**
```
cd BIT240058_MID
dotnet run
```
Sau đó mở http://localhost:5058

Lần chạy đầu, ứng dụng **tự tạo database `BIT240058_MID` và 3 bảng** (`EnsureCreated`), rồi thêm dữ liệu mẫu:
- 3 loại món
- 6 món, trong đó có 2 món ngừng bán và 1 món chưa có ảnh

Bạn không cần chạy lệnh migration.

> Nếu máy chỉ có .NET 9: trong file `.csproj`, đổi `net8.0` thành `net9.0` và đổi `Version="8.0.10"` thành `Version="9.0.0"`.
> Nếu SQL Server là bản Express: trong `appsettings.json`, đổi `Server=localhost` thành `Server=localhost\\SQLEXPRESS`.

## 2. Cấu trúc thư mục

```
BIT240058_MID/
├── Program.cs                 Cấu hình: kết nối DB, đa ngôn ngữ, xử lý lỗi, route
├── appsettings.json           Chuỗi kết nối SQL Server (localhost, Windows Auth)
├── SharedResource.cs          Lớp đánh dấu cho đa ngôn ngữ
├── Resources/
│   ├── SharedResource.resx    Ngôn ngữ mặc định English (dùng thẳng câu tiếng Anh)
│   └── SharedResource.vi.resx Bản dịch Tiếng Việt
├── Models/                    3 Model có MSSV + validation
│   ├── DishCategory_BIT240058.cs  → bảng BIT240058_DishCategories
│   ├── Dish_BIT240058.cs          → bảng BIT240058_Dishes (+ phân loại Quick/Regular)
│   ├── DishImage_BIT240058.cs     → bảng BIT240058_DishImages
│   └── GreaterThanZeroAttribute.cs  validation "> 0" cho Price, PreparationTime
├── Data/
│   ├── AppDbContext.cs        DbContext: quan hệ 1-n, khóa ngoại, index 1 ảnh đại diện
│   └── SeedData.cs            Tạo DB + dữ liệu mẫu
├── ViewModels/                DishFilter (điều kiện lọc), DishIndexViewModel...
├── Controllers/
│   ├── DishesController.cs           Danh sách/lọc, Details, Create, Edit, Delete, ảnh
│   ├── DishCategoriesController.cs   Loại món, chặn xóa loại đang dùng
│   ├── LanguageController.cs         Đổi ngôn ngữ (cookie), giữ URL + bộ lọc
│   ├── HomeController.cs             Trang lỗi 404/500
│   └── BaseController.cs             Hàm dùng chung: trang 404 thân thiện
├── Views/                     Giao diện Razor (Bootstrap 5)
└── database/check-names.sql   Script kiểm tra tên bảng/cột khi demo
```

## 3. Đáp ứng yêu cầu đề bài

| Yêu cầu | Cách làm |
|---|---|
| Tiền tố MSSV | `[Table("BIT240058_Dishes")]`, `[Column("BIT240058_Name")]`… trên từng thuộc tính. Tên Model: `Dish_BIT240058`… |
| Quan hệ | `DishCategory 1-n Dish` (khóa ngoại Restrict), `Dish 1-n DishImage` (Cascade), cấu hình trong `AppDbContext` |
| Validation | `[Required]`, `[GreaterThanZero]`, `[Range]` trong Model. Loại món tồn tại + tên không trùng trong cùng loại: `ValidateBusinessRulesAsync()` + unique index `(Name, DishCategoryId)` |
| Create/Edit chọn loại từ DB | `LoadCategoriesAsync()` đổ `SelectList` từ bảng loại món |
| Tìm kiếm / lọc / sắp xếp | `DishesController.Index`: ghép `IQueryable` → EF sinh 1 câu SQL `WHERE … ORDER BY` (xem câu SQL in trong console) |
| Giữ điều kiện lọc | Form GET, các ô lấy lại giá trị từ `Filter`/query string |
| Quick / Regular | `[NotMapped] IsQuick` (≤ 15 phút), không có cột trong DB |
| Ảnh | Thêm bằng URL, hiển thị nhiều ảnh, đổi ảnh đại diện: `ExecuteUpdate` bỏ ảnh cũ rồi đặt ảnh mới trong 1 transaction + filtered unique index |
| Ràng buộc nghiệp vụ | Không xóa loại còn món; món ngừng bán vẫn hiển thị nhãn "Unavailable / Ngừng bán"; lọc "Available" chỉ ra món đang bán |
| Xử lý lỗi | Id không tồn tại → trang 404 thân thiện; giá min > max, nhập chữ vào ô giá, món chưa có ảnh, không có kết quả, form sai → thông báo; `UseExceptionHandler` không bao giờ hiện stack trace |
| Đa ngôn ngữ | Mặc định English (chỉ đọc cookie, bỏ qua ngôn ngữ trình duyệt). Nút English / Tiếng Việt trên menu, lưu cookie. Khi đổi ngôn ngữ, quay lại đúng URL kèm query string nên giữ bộ lọc. Validation + lỗi nhập sai kiểu cũng được dịch |

## 4. Kịch bản quay video demo

1. Mở SSMS, chạy `database/check-names.sql`: thấy database `BIT240058_MID`, 3 bảng, mọi cột có tiền tố `BIT240058_`. Câu 3 của script trả về rỗng. Mở thư mục `Models` để thấy tên `Dish_BIT240058`…
2. Mở web (mặc định **English**). Vào danh sách món và loại món. Thử lần lượt:
   - Tìm `ph`.
   - Lọc loại "Món chính".
   - Lọc trạng thái Available.
   - Lọc giá 30000 – 50000.
   - Sắp xếp giá giảm dần.
   Sau mỗi lần lọc, các ô vẫn giữ giá trị.
3. Xem cột Speed: Quick / Regular. Thử **Create** với dữ liệu sai: tên trống, giá 0, giá `abc`, tên trùng "Phở bò" trong "Món chính". Sau đó nhập dữ liệu hợp lệ và lưu.
4. Vào **Details** "Cơm tấm sườn" (3 ảnh). Thêm ảnh `https://picsum.photos/seed/demo/640/420`, rồi bấm "Set as thumbnail" ở ảnh khác. Mở "Sinh tố bơ" để thấy thông báo món chưa có ảnh.
5. Vào Dish categories, xóa "Món chính": bị chặn. Gõ URL `/Dishes/Details/9999`: hiện trang 404. Thử thêm min price 50000 > max price 10000: báo lỗi.
6. Bấm **Tiếng Việt** ở trang danh sách đang có bộ lọc: giao diện đổi sang tiếng Việt, bộ lọc vẫn giữ. Chuyển qua Chi tiết, Sửa, Thêm: vẫn là tiếng Việt. Thử submit form sai để thấy câu báo lỗi tiếng Việt. Bấm **English** để đổi lại.
