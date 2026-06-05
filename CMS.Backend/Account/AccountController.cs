using CMS.Data; // Thay bằng Namespace tương ứng của project chứa ApplicationDbContext
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CMS.Controllers // Thay bằng namespace của bạn
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. HIỂN THỊ GIAO DIỆN ĐĂNG NHẬP ---
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // --- 2. XỬ LÝ LOGIC ĐĂNG NHẬP ---
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Kiểm tra tài khoản trong Database
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

            if (user != null)
            {
                // Thiết lập danh tính (Claims)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role), // Lưu vai trò: Admin/Editor
                    new Claim("FullName", user.FullName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Đăng nhập và lưu Cookie vào trình duyệt
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View();
        }

        // --- 3. XỬ LÝ ĐĂNG XUẤT ---
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // --- 4. TRANG TỪ CHỐI TRUY CẬP (ACCESS DENIED) ---
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}