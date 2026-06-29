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

        // --- CLASS NHẬN DỮ LIỆU CẬP NHẬT TỪ TRÌNH DUYỆT ---
        public class UpdateProfileModel
        {
            public string Name { get; set; }
            public string Phone { get; set; }
            public string? Address { get; set; }
            public string? NewPassword { get; set; } // Có thể đổi mật khẩu hoặc không
        }
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