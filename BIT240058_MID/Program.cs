using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.Extensions.WebEncoders;
using BIT240058_MID;
using BIT240058_MID.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

// =====================================================================
// 1. KẾT NỐI DATABASE (Entity Framework Core + SQL Server)
//    Chuỗi kết nối nằm trong appsettings.json (Server=localhost, Windows Authentication)
// =====================================================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =====================================================================
// 2. ĐA NGÔN NGỮ (English / Tiếng Việt)
//    - Bản dịch tiếng Việt nằm trong Resources/SharedResource.vi.resx
//    - Dịch cả giao diện (View), câu báo lỗi validation (DataAnnotations)
// =====================================================================
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        // Mọi ErrorMessage / Display(Name) trong Model đều dịch qua SharedResource
        options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(SharedResource));
    });

// Dịch các câu lỗi khi người dùng nhập SAI KIỂU (vd: nhập "abc" vào ô Giá)
builder.Services.AddOptions<MvcOptions>().Configure<IStringLocalizerFactory>((mvc, factory) =>
{
    var L = factory.Create(typeof(SharedResource));
    var p = mvc.ModelBindingMessageProvider;
    p.SetAttemptedValueIsInvalidAccessor((value, field) => L["The value '{0}' is not valid for {1}.", value, field].Value);
    p.SetValueIsInvalidAccessor(value => L["The value '{0}' is invalid.", value].Value);
    p.SetValueMustBeANumberAccessor(field => L["The field {0} must be a number.", field].Value);
    p.SetValueMustNotBeNullAccessor(value => L["This field is required."].Value);
    p.SetMissingBindRequiredValueAccessor(field => L["This field is required."].Value);
    p.SetMissingKeyOrValueAccessor(() => L["This field is required."].Value);
    p.SetUnknownValueIsInvalidAccessor(field => L["The value is not valid for {0}.", field].Value);
    p.SetNonPropertyAttemptedValueIsInvalidAccessor(value => L["The value '{0}' is invalid.", value].Value);
    p.SetNonPropertyUnknownValueIsInvalidAccessor(() => L["The value is invalid."].Value);
    p.SetNonPropertyValueMustBeANumberAccessor(() => L["The field must be a number."].Value);
});

// Cho phép Razor xuất thẳng chữ tiếng Việt có dấu (không mã hóa thành &#x...;)
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

var app = builder.Build();

// =====================================================================
// 3. TẠO DATABASE + DỮ LIỆU MẪU KHI KHỞI ĐỘNG
// =====================================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData.Initialize(db);
}

// =====================================================================
// 4. XỬ LÝ LỖI: không bao giờ hiển thị lỗi hệ thống (stack trace) ra màn hình
//    - Lỗi không mong muốn (500)  -> /Home/Error
//    - Đường dẫn không tồn tại (404) -> /Home/StatusCodePage?code=404
// =====================================================================
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/StatusCodePage", "?code={0}");

app.UseStaticFiles();

// =====================================================================
// 5. CẤU HÌNH NGÔN NGỮ CHO MỖI REQUEST
//    - Mặc định: English ("en")
//    - Chỉ đọc ngôn ngữ từ COOKIE (do người dùng chọn), KHÔNG đọc theo ngôn ngữ trình duyệt
//      => lần đầu truy cập luôn là tiếng Anh như đề yêu cầu
// =====================================================================
var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("vi") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider()
    }
});

app.UseRouting();

// Route mặc định: /{controller}/{action}/{id?} -> trang chủ là danh sách món ăn
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dishes}/{action=Index}/{id?}");

app.Run();
