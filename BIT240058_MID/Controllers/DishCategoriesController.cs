using BIT240058_MID.Data;
using BIT240058_MID.Models;
using BIT240058_MID.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BIT240058_MID.Controllers
{
    /// <summary>
    /// Quản lý loại món ăn: danh sách, thêm, sửa, xóa.
    /// Ràng buộc: KHÔNG cho xóa loại món đang có món sử dụng.
    /// </summary>
    public class DishCategoriesController : BaseController
    {
        private readonly AppDbContext _db;

        public DishCategoriesController(AppDbContext db)
        {
            _db = db;
        }

        // ----- Danh sách loại món kèm số món (đếm bằng SQL COUNT) -----
        public async Task<IActionResult> Index()
        {
            var items = await _db.DishCategories
                .OrderBy(c => c.Id)
                .Select(c => new CategoryListItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DishCount = c.Dishes.Count()
                })
                .ToListAsync();
            return View(items);
        }

        // ----- Thêm loại món -----
        public IActionResult Create() => View(new DishCategory_BIT240058());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] DishCategory_BIT240058 category)
        {
            if (!ModelState.IsValid) return View(category);

            category.Name = category.Name.Trim();
            _db.DishCategories.Add(category);
            await _db.SaveChangesAsync();

            TempData[SuccessKey] = L["Category \"{0}\" was created.", category.Name].Value;
            return RedirectToAction(nameof(Index));
        }

        // ----- Sửa loại món -----
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFoundPage("Invalid category Id.");

            var category = await _db.DishCategories.FindAsync(id);
            if (category == null)
                return NotFoundPage("Dish category with Id = {0} was not found.", id);

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] DishCategory_BIT240058 input)
        {
            var category = await _db.DishCategories.FindAsync(id);
            if (category == null)
                return NotFoundPage("Dish category with Id = {0} was not found.", id);

            input.Id = id;
            if (!ModelState.IsValid) return View(input);

            category.Name = input.Name.Trim();
            category.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
            await _db.SaveChangesAsync();

            TempData[SuccessKey] = L["Category was updated."].Value;
            return RedirectToAction(nameof(Index));
        }

        // ----- Xóa loại món: chặn nếu loại vẫn còn món -----
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.DishCategories.FindAsync(id);
            if (category == null)
                return NotFoundPage("Dish category with Id = {0} was not found.", id);

            // Ràng buộc nghiệp vụ: đếm số món thuộc loại này
            int dishCount = await _db.Dishes.CountAsync(d => d.DishCategoryId == id);
            if (dishCount > 0)
            {
                TempData[ErrorKey] = L["Cannot delete category \"{0}\" because it still has {1} dish(es).",
                    category.Name, dishCount].Value;
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _db.DishCategories.Remove(category);
                await _db.SaveChangesAsync();
                TempData[SuccessKey] = L["Category was deleted."].Value;
            }
            catch (DbUpdateException)
            {
                // Khóa ngoại Restrict trong DB chặn thêm một lớp nữa
                TempData[ErrorKey] = L["Cannot delete this category because it is in use."].Value;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
