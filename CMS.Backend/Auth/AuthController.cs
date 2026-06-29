using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace CMS.Backend.Controllers
{
    [AllowAnonymous]
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            // Bỏ qua lỗi nhỏ để tránh 400 Bad Request
            ModelState.Remove("Address");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var isEmailExist = _context.Customers.Any(c => c.Email == model.Email);
            if (isEmailExist)
            {
                return BadRequest(new { message = "Email này đã được đăng ký! Vui lòng dùng email khác." });
            }

            var newCustomer = new Customer
            {
                FullName = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Password = model.Password,
                Address = model.Address ?? ""
            };

            _context.Customers.Add(newCustomer);
            _context.SaveChanges();

            return Ok(new { message = "Đăng ký thành công!" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new { message = "Vui lòng nhập đầy đủ Email và Mật khẩu!" });
            }

            var customer = _context.Customers
                .FirstOrDefault(c => c.Email == model.Email && c.Password == model.Password);

            if (customer == null)
            {
                return BadRequest(new { message = "Email hoặc Mật khẩu không chính xác!" });
            }

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                user = new
                {
                    id = customer.Id,
                    name = customer.FullName,
                    email = customer.Email,
                    phone = customer.Phone
                }
            });
        }
        // ==========================================
        // 🚀 API CẬP NHẬT THÔNG TIN HỒ SƠ (PROFILE)
        // ==========================================
        [HttpPut("update-profile/{id}")]
        public IActionResult UpdateProfile(int id, [FromBody] UpdateProfileModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Tìm khách hàng trong DB theo ID
            var customer = _context.Customers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin tài khoản!" });
            }

            // 2. Cập nhật các trường thông tin mới
            customer.FullName = model.Name;
            customer.Phone = model.Phone;
            customer.Address = model.Address ?? "";

            // Nếu người dùng có nhập mật khẩu mới thì mới cập nhật mật khẩu
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                customer.Password = model.NewPassword;
            }

            // 3. Lưu thay đổi vào Database
            _context.SaveChanges();

            // 4. Trả về thông tin mới để React cập nhật lại localStorage
            return Ok(new
            {
                message = "Cập nhật hồ sơ thành công!",
                user = new
                {
                    id = customer.Id,
                    name = customer.FullName,
                    email = customer.Email,
                    phone = customer.Phone,
                    address = customer.Address
                }
            });

        }
        // ========================================================
        // 🚀 API QUÊN MẬT KHẨU (XÁC THỰC EMAIL + SĐT ĐỂ ĐẶT LẠI)
        // ========================================================
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Tìm xem có Customer nào khớp cả Email và Số điện thoại không để xác minh chính chủ
            var customer = _context.Customers
                .FirstOrDefault(c => c.Email == model.Email && c.Phone == model.Phone);

            if (customer == null)
            {
                return BadRequest(new { message = "Email hoặc Số điện thoại không chính xác, vui lòng kiểm tra lại!" });
            }

            // 2. Nếu khớp thông tin -> Tiến hành đè mật khẩu mới
            customer.Password = model.NewPassword;
            _context.SaveChanges(); // Lưu vào SQL Server

            return Ok(new { message = "Đặt lại mật khẩu thành công! Bạn đang được chuyển đến trang đăng nhập..." });
        }
        // --- CLASS NHẬN DỮ LIỆU CẬP NHẬT TỪ TRÌNH DUYỆT ---
        public class UpdateProfileModel
        {
            public string Name { get; set; }
            public string Phone { get; set; }
            public string? Address { get; set; }
            public string? NewPassword { get; set; } // Có thể đổi mật khẩu hoặc không
        }
    }
    // --- CLASS ĐẠI DIỆN DỮ LIỆU ĐỂ HỨNG TỪ REACT ---
    public class ForgotPasswordModel
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string NewPassword { get; set; }
    }
    public class RegisterModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string? Address { get; set; }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}