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