using Microsoft.EntityFrameworkCore;
using VKITActivityManager.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
var builder = WebApplication.CreateBuilder(args);
// 1. Mở rộng giới hạn dung lượng upload lên 500MB cho Kestrel Server
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 524288000; // 500MB
});

// 2. Mở rộng giới hạn bộ đọc Form dữ liệu lên 500MB
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 524288000; // 500MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
options.MultipartBodyLengthLimit = 1073741824; // 200 MB tính bằng bytes
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Nếu chưa đăng nhập, tự động đá về trang này
        options.Cookie.Name = "VKITAdminCookie";
        options.ExpireTimeSpan = TimeSpan.FromHours(2); // Đăng nhập tự động thoát sau 2 tiếng
    });
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// --- THÊM ĐOẠN NÀY ĐỂ MÁY CHỦ KESTREL NHẬN FILE LÊN ĐẾN 200MB ---
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 524288000; // 200 MB
});
// Đăng ký EmailService
builder.Services.AddTransient<EmailService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


// --- THÊM DÒNG NÀY (Bắt buộc phải nằm trước UseAuthorization) ---
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
