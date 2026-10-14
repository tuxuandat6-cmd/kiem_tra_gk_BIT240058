using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace BIT240058_MID.Controllers
{
    /// <summary>
    /// Chuyển đổi ngôn ngữ English ↔ Tiếng Việt.
    /// Ngôn ngữ được lưu trong COOKIE nên khi chuyển sang trang khác vẫn giữ ngôn ngữ đã chọn.
    /// </summary>
    public class LanguageController : Controller
    {
        // GET /Language/Set?culture=vi&returnUrl=/Dishes?keyword=pho
        public IActionResult Set(string culture, string? returnUrl)
        {
            // Chỉ chấp nhận "en" hoặc "vi"
            if (culture != "en" && culture != "vi") culture = "en";

            // Ghi cookie ngôn ngữ (hạn 1 năm)
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, Path = "/" });

            // Quay lại đúng trang cũ (kèm query string) -> giữ nguyên điều kiện tìm kiếm/lọc.
            // Url.IsLocalUrl: chỉ cho quay về trang trong website (chống chuyển hướng ra ngoài)
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                returnUrl = "/";

            return LocalRedirect(returnUrl);
        }
    }
}
