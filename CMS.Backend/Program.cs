using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using CMS.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container (ĐÃ THÊM THUỐC GIẢI VÒNG LẶP VÔ HẠN JSON)
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Dòng này cực kỳ quan trọng: Báo cho C# biết nếu thấy vòng lặp Product -> Category -> Product thì bỏ qua, không dịch nữa.
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Authentication (Cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// 2. Cấu hình CORS - Cấp phép cho React App
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Đảm bảo đúng cổng 3000 của React
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Rất quan trọng nếu bạn dùng Cookie/Session
    });
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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// 3. Thứ tự Middleware là cực kỳ quan trọng
app.UseRouting();

// Sử dụng CORS ở đây
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();