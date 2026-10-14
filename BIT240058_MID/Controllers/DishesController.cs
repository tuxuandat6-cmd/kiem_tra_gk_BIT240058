using BIT240058_MID.Data;
using BIT240058_MID.Models;
using BIT240058_MID.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BIT240058_MID.Controllers
{
    /// <summary>
    /// Quản lý món ăn: danh sách + tra cứu, chi tiết, thêm, sửa, xóa, quản lý ảnh.
    /// </summary>
    public class DishesController : BaseController
    {
        private readonly AppDbContext _db;

        public DishesController(AppDbContext db)
        {
            _db = db;
        }

        // =================================================================
        // DANH SÁCH + TÌM KIẾM / LỌC / SẮP XẾP
        // GET /Dishes?keyword=..&categoryId=..&status=..&minPrice=..&maxPrice=..&sort=..
        // Tất cả điều kiện được ghép vào IQueryable -> EF Core sinh 1 câu SQL có WHERE + ORDER BY.
        // KHÔNG tải toàn bộ dữ liệu rồi lọc bằng vòng lặp.
        // =================================================================
        public async Task<IActionResult> Index(DishFilter filter)
        {
            var vm = new DishIndexViewModel
            {
                Filter = filter,
                Categories = await _db.DishCategories.OrderBy(c => c.Name).ToListAsync()
            };

            // ----- Kiểm tra dữ liệu bộ lọc -----
            // Lỗi sai kiểu dữ liệu, vd: nhập chữ "abc" vào ô giá
            if (HasError(nameof(DishFilter.MinPrice)) || HasError(nameof(DishFilter.MaxPrice)))
                vm.Errors.Add(L["Price filter must be a valid number."]);
            if (HasError(nameof(DishFilter.CategoryId)))
                vm.Errors.Add(L["Dish category filter is not valid."]);
            if (filter.MinPrice < 0 || filter.MaxPrice < 0)
                vm.Errors.Add(L["Price filter must not be negative."]);
            if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue && filter.MinPrice > filter.MaxPrice)
                vm.Errors.Add(L["Minimum price must not be greater than maximum price."]);
            if (filter.CategoryId.HasValue && !vm.Categories.Any(c => c.Id == filter.CategoryId))
                vm.Errors.Add(L["Dish category with Id = {0} does not exist.", filter.CategoryId.Value]);

            // Bộ lọc sai -> không truy vấn, hiển thị thông báo lỗi
            if (vm.Errors.Count > 0)
                return View(vm);

            // ----- Ghép điều kiện lọc (chạy ở tầng database) -----
            IQueryable<Dish_BIT240058> query = _db.Dishes;

            // Tìm theo một phần tên món -> SQL: WHERE Name LIKE '%keyword%'
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim();
                query = query.Where(d => d.Name.Contains(keyword));
            }

            // Lọc theo loại món
            if (filter.CategoryId.HasValue)
                query = query.Where(d => d.DishCategoryId == filter.CategoryId.Value);

            // Lọc theo trạng thái: "Đang bán" thì KHÔNG hiển thị món ngừng bán
            if (filter.Status == DishFilter.StatusAvailable)
                query = query.Where(d => d.IsAvailable);
            else if (filter.Status == DishFilter.StatusUnavailable)
                query = query.Where(d => !d.IsAvailable);

            // Lọc theo khoảng giá
            if (filter.MinPrice.HasValue)
                query = query.Where(d => d.Price >= filter.MinPrice.Value);
            if (filter.MaxPrice.HasValue)
                query = query.Where(d => d.Price <= filter.MaxPrice.Value);

            // ----- Sắp xếp -> SQL: ORDER BY -----
            query = filter.Sort switch
            {
                DishFilter.SortPriceAsc => query.OrderBy(d => d.Price).ThenBy(d => d.Id),
                DishFilter.SortPriceDesc => query.OrderByDescending(d => d.Price).ThenBy(d => d.Id),
                DishFilter.SortTimeAsc => query.OrderBy(d => d.PreparationTime).ThenBy(d => d.Id),
                _ => query.OrderBy(d => d.Id)
            };

            // ----- Chỉ lấy các cột cần hiển thị (1 câu SELECT có JOIN loại món + ảnh đại diện) -----
            vm.Dishes = await query.Select(d => new DishListItem
            {
                Id = d.Id,
                Name = d.Name,
                CategoryName = d.DishCategory!.Name,
                Price = d.Price,
                PreparationTime = d.PreparationTime,
                IsAvailable = d.IsAvailable,
                ThumbnailUrl = d.Images.Where(i => i.IsThumbnail).Select(i => i.ImageUrl).FirstOrDefault()
            }).ToListAsync();

            return View(vm);
        }

        // =================================================================
        // CHI TIẾT MÓN ĂN: GET /Dishes/Details/5
        // =================================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)   // thiếu Id hoặc Id không phải số (vd: /Dishes/Details/abc)
                return NotFoundPage("Invalid dish Id.");

            // Lấy món + loại món + danh sách ảnh (ảnh đại diện đứng đầu)
            var dish = await _db.Dishes
                .Include(d => d.DishCategory)
                .Include(d => d.Images.OrderByDescending(i => i.IsThumbnail).ThenBy(i => i.Id))
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dish == null)
                return NotFoundPage("Dish with Id = {0} was not found.", id);

            return View(dish);
        }

        // =================================================================
        // THÊM MÓN ĂN
        // GET  /Dishes/Create  -> hiển thị form trống
        // POST /Dishes/Create  -> kiểm tra hợp lệ rồi lưu
        // =================================================================
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync(null);
            return View(new Dish_BIT240058());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Price,PreparationTime,IsAvailable,Description,DishCategoryId")] Dish_BIT240058 dish)
        {
            // Kiểm tra nghiệp vụ cần truy vấn DB (loại món tồn tại, tên không trùng)
            await ValidateBusinessRulesAsync(dish);

            if (!ModelState.IsValid)
            {
                // Dữ liệu không hợp lệ -> hiển thị lại form, giữ dữ liệu đã nhập + báo lỗi
                await LoadCategoriesAsync(dish.DishCategoryId);
                return View(dish);
            }

            try
            {
                _db.Dishes.Add(dish);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Phòng trường hợp unique index trong DB chặn (2 người thêm cùng lúc)
                ModelState.AddModelError(string.Empty, L["Could not save. The dish name may already exist in this category."]);
                await LoadCategoriesAsync(dish.DishCategoryId);
                return View(dish);
            }

            TempData[SuccessKey] = L["Dish \"{0}\" was created.", dish.Name].Value;
            return RedirectToAction(nameof(Details), new { id = dish.Id });
        }

        // =================================================================
        // SỬA MÓN ĂN
        // GET  /Dishes/Edit/5  -> form có sẵn dữ liệu cũ
        // POST /Dishes/Edit/5  -> kiểm tra hợp lệ rồi cập nhật
        // =================================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFoundPage("Invalid dish Id.");

            var dish = await _db.Dishes.FindAsync(id);
            if (dish == null)
                return NotFoundPage("Dish with Id = {0} was not found.", id);

            await LoadCategoriesAsync(dish.DishCategoryId);
            return View(dish);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Name,Price,PreparationTime,IsAvailable,Description,DishCategoryId")] Dish_BIT240058 input)
        {
            // Món cần sửa phải còn tồn tại
            var dish = await _db.Dishes.FindAsync(id);
            if (dish == null)
                return NotFoundPage("Dish with Id = {0} was not found.", id);

            input.Id = id;
            await ValidateBusinessRulesAsync(input);

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync(input.DishCategoryId);
                return View(input);
            }

            // Chép dữ liệu mới vào entity đang được EF theo dõi -> SaveChanges sinh câu UPDATE
            dish.Name = input.Name.Trim();
            dish.Price = input.Price;
            dish.PreparationTime = input.PreparationTime;
            dish.IsAvailable = input.IsAvailable;
            dish.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
            dish.DishCategoryId = input.DishCategoryId;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, L["Could not save. The dish name may already exist in this category."]);
                await LoadCategoriesAsync(input.DishCategoryId);
                return View(input);
            }

            TempData[SuccessKey] = L["Dish was updated."].Value;
            return RedirectToAction(nameof(Details), new { id });
        }

        // =================================================================
        // XÓA MÓN ĂN: POST /Dishes/Delete/5 (ảnh của món bị xóa theo - cascade)
        // =================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var dish = await _db.Dishes.FindAsync(id);
            if (dish == null)
                return NotFoundPage("Dish with Id = {0} was not found.", id);

            _db.Dishes.Remove(dish);
            await _db.SaveChangesAsync();

            TempData[SuccessKey] = L["Dish \"{0}\" was deleted.", dish.Name].Value;
            return RedirectToAction(nameof(Index));
        }

        // =================================================================
        // QUẢN LÝ ẢNH
        // =================================================================

        // ----- Thêm ảnh bằng URL: POST /Dishes/AddImage -----
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(int dishId, string? imageUrl, bool isThumbnail)
        {
            if (!await _db.Dishes.AnyAsync(d => d.Id == dishId))
                return NotFoundPage("Dish with Id = {0} was not found.", dishId);

            // Kiểm tra URL: không trống, phải là http/https, tối đa 1000 ký tự
            var url = imageUrl?.Trim() ?? string.Empty;
            if (url.Length == 0)
            {
                TempData[ErrorKey] = L["Image URL is required."].Value;
                return RedirectToAction(nameof(Details), new { id = dishId });
            }
            if (url.Length > 1000
                || !Uri.TryCreate(url, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                TempData[ErrorKey] = L["Image URL is not valid (must start with http:// or https://)."].Value;
                return RedirectToAction(nameof(Details), new { id = dishId });
            }

            // Món chưa có ảnh đại diện thì ảnh đầu tiên tự thành ảnh đại diện
            bool hasThumbnail = await _db.DishImages.AnyAsync(i => i.DishId == dishId && i.IsThumbnail);
            bool makeThumbnail = isThumbnail || !hasThumbnail;

            // Dùng transaction: bỏ ảnh đại diện cũ + thêm ảnh mới phải cùng thành công
            await using var tx = await _db.Database.BeginTransactionAsync();
            if (makeThumbnail && hasThumbnail)
            {
                // Ảnh đại diện cũ -> ảnh thường (UPDATE trực tiếp trong DB)
                await _db.DishImages
                    .Where(i => i.DishId == dishId && i.IsThumbnail)
                    .ExecuteUpdateAsync(s => s.SetProperty(i => i.IsThumbnail, false));
            }
            _db.DishImages.Add(new DishImage_BIT240058 { DishId = dishId, ImageUrl = url, IsThumbnail = makeThumbnail });
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData[SuccessKey] = L["Image was added."].Value;
            return RedirectToAction(nameof(Details), new { id = dishId });
        }

        // ----- Đổi ảnh đại diện: POST /Dishes/SetThumbnail -----
        // Mỗi món tối đa 1 ảnh đại diện: ảnh cũ tự chuyển thành ảnh thường
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetThumbnail(int dishId, int imageId)
        {
            if (!await _db.Dishes.AnyAsync(d => d.Id == dishId))
                return NotFoundPage("Dish with Id = {0} was not found.", dishId);
            if (!await _db.DishImages.AnyAsync(i => i.Id == imageId && i.DishId == dishId))
                return NotFoundPage("Image with Id = {0} was not found.", imageId);

            await using var tx = await _db.Database.BeginTransactionAsync();

            // Bước 1: bỏ đánh dấu tất cả ảnh đại diện hiện tại của món
            await _db.DishImages
                .Where(i => i.DishId == dishId && i.IsThumbnail)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.IsThumbnail, false));

            // Bước 2: đặt ảnh được chọn làm ảnh đại diện
            await _db.DishImages
                .Where(i => i.Id == imageId)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.IsThumbnail, true));

            await tx.CommitAsync();

            TempData[SuccessKey] = L["Thumbnail was changed."].Value;
            return RedirectToAction(nameof(Details), new { id = dishId });
        }

        // ----- Xóa ảnh: POST /Dishes/DeleteImage -----
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int dishId, int imageId)
        {
            var image = await _db.DishImages.FirstOrDefaultAsync(i => i.Id == imageId && i.DishId == dishId);
            if (image == null)
                return NotFoundPage("Image with Id = {0} was not found.", imageId);

            bool wasThumbnail = image.IsThumbnail;
            _db.DishImages.Remove(image);
            await _db.SaveChangesAsync();

            // Nếu xóa ảnh đại diện -> chọn ảnh còn lại đầu tiên làm ảnh đại diện
            if (wasThumbnail)
            {
                var next = await _db.DishImages.Where(i => i.DishId == dishId).OrderBy(i => i.Id).FirstOrDefaultAsync();
                if (next != null)
                {
                    next.IsThumbnail = true;
                    await _db.SaveChangesAsync();
                }
            }

            TempData[SuccessKey] = L["Image was deleted."].Value;
            return RedirectToAction(nameof(Details), new { id = dishId });
        }

        // =================================================================
        // HÀM HỖ TRỢ
        // =================================================================

        /// <summary>Kiểm tra một tham số trên URL có bị lỗi chuyển kiểu không</summary>
        private bool HasError(string key) =>
            ModelState.TryGetValue(key, out var entry) && entry.Errors.Count > 0;

        /// <summary>Đổ danh sách loại món từ DATABASE vào ô chọn (dropdown) của form Create/Edit</summary>
        private async Task LoadCategoriesAsync(int? selectedId)
        {
            var categories = await _db.DishCategories.OrderBy(c => c.Name).ToListAsync();
            ViewData["Categories"] = new SelectList(categories, "Id", "Name", selectedId);
        }

        /// <summary>
        /// Validation nghiệp vụ (cần truy vấn DB):
        ///  - DishCategoryId phải tồn tại
        ///  - Tên món không được trùng trong cùng một loại (khi sửa thì bỏ qua chính món đó)
        /// </summary>
        private async Task ValidateBusinessRulesAsync(Dish_BIT240058 dish)
        {
            if (dish.DishCategoryId > 0 && !await _db.DishCategories.AnyAsync(c => c.Id == dish.DishCategoryId))
            {
                ModelState.AddModelError(nameof(dish.DishCategoryId), L["Dish category does not exist."]);
                return;
            }

            if (string.IsNullOrWhiteSpace(dish.Name) || dish.DishCategoryId <= 0) return;

            var name = dish.Name.Trim();
            bool duplicated = await _db.Dishes.AnyAsync(d =>
                d.DishCategoryId == dish.DishCategoryId &&
                d.Name == name &&
                d.Id != dish.Id);

            if (duplicated)
                ModelState.AddModelError(nameof(dish.Name), L["Dish name already exists in this category."]);
            else
                dish.Name = name;
        }
    }
}
