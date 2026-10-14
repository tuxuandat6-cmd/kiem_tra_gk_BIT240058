using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BIT240058_MID.Controllers
{
    /// <summary>
    /// Controller cha dùng chung: cung cấp bộ dịch L và hàm hiển thị trang "Không tìm thấy" (404).
    /// </summary>
    public abstract class BaseController : Controller
    {
        // Bộ dịch ngôn ngữ (lấy từ DI khi cần)
        protected IStringLocalizer<SharedResource> L =>
            HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();

        // Tên key TempData để hiển thị thông báo thành công / lỗi sau khi redirect
        protected const string SuccessKey = "Success";
        protected const string ErrorKey = "Error";

        /// <summary>
        /// Trả về trang 404 thân thiện (Views/Shared/NotFound.cshtml) thay vì lỗi hệ thống.
        /// messageKey là câu tiếng Anh, sẽ được dịch theo ngôn ngữ đang chọn.
        /// </summary>
        protected IActionResult NotFoundPage(string messageKey, params object[] args)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound", L[messageKey, args].Value);
        }
    }
}
