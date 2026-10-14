using Microsoft.AspNetCore.Mvc;

namespace BIT240058_MID.Controllers
{
    /// <summary>Các trang báo lỗi chung (thay cho trang lỗi hệ thống)</summary>
    public class HomeController : BaseController
    {
        public IActionResult Index() => RedirectToAction("Index", "Dishes");

        // Lỗi không mong muốn (exception) -> trang "Đã có lỗi xảy ra"
        public IActionResult Error()
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return View();
        }

        // Đường dẫn không tồn tại (404) hoặc lỗi HTTP khác
        public IActionResult StatusCodePage(int code)
        {
            if (code == StatusCodes.Status404NotFound)
                return NotFoundPage("The page you requested does not exist.");

            Response.StatusCode = code;
            return View("Error");
        }
    }
}
