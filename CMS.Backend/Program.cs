using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using CMS.Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CẤU HÌNH CORS (BẮT BUỘC PHẢI THÊM)
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin()  // Cho phép React (cổng 3000 hoặc bất kỳ cổng nào) gọi API
              .AllowAnyHeader()  // Cho phép mọi loại dữ liệu gửi lên
              .AllowAnyMethod(); // Cho phép GET, POST, PUT, DELETE...
    });
});

// ==========================================
// 🌟 ĐOẠN FIX LỖI TÀNG HÌNH VÀ VÒNG LẶP CHO REACT
// ==========================================
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // 1. Chống sập Backend do lỗi vòng lặp vô hạn
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

        // 2. Ép C# viết thường chữ cái đầu (Thành id, name, price, imageUrl) để React đọc được
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// --- THÊM 2 DÒNG NÀY ĐỂ HỖ TRỢ SWAGGER ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// ------------------------------------------

// Đăng ký DbContext vào hệ thống
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình dịch vụ xác thực bằng Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // --- THÊM 2 DÒNG NÀY ĐỂ HIỂN THỊ GIAO DIỆN SWAGGER ---
    app.UseSwagger();
    app.UseSwaggerUI();
    // ----------------------------------------------------
}

// ==========================================
// 🌟 TẮT ÉP BUỘC HTTPS ĐỂ CỔNG HTTP (5226) CỦA REACT KHÔNG BỊ CHẶN LỖI NETWORK
// ==========================================
// app.UseHttpsRedirection(); 

app.UseStaticFiles();

app.UseRouting();

// ==========================================
// 2. KÍCH HOẠT CORS (BẮT BUỘC ĐẶT Ở VỊ TRÍ NÀY)
// Nằm dưới UseRouting và trên UseAuthentication
// ==========================================
app.UseCors("AllowReactApp");

// Bật Middleware Xác thực 
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();