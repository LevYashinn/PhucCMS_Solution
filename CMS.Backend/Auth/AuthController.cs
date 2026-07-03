using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory; // 🌟 Thư viện bộ nhớ đệm cho OTP
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CMS.Backend.Controllers
{
    [AllowAnonymous]
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache; // 🌟 Khai báo bộ nhớ đệm

        // 🌟 Nhúng IMemoryCache vào Constructor
        public AuthController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
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

        [HttpPut("update-profile/{id}")]
        public IActionResult UpdateProfile(int id, [FromBody] UpdateProfileModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var customer = _context.Customers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin tài khoản!" });
            }

            customer.FullName = model.Name;
            customer.Phone = model.Phone;
            customer.Address = model.Address ?? "";

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                customer.Password = model.NewPassword;
            }

            _context.SaveChanges();

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
        // 🚀 1. API GỬI MÃ OTP VỀ EMAIL
        // ========================================================
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email)) return BadRequest(new { message = "Vui lòng nhập Email!" });

            var user = _context.Customers.FirstOrDefault(c => c.Email == request.Email);
            if (user == null) return NotFound(new { message = "Email này chưa được đăng ký trong hệ thống!" });

            // Tạo mã OTP ngẫu nhiên 6 số
            Random rand = new Random();
            string otp = rand.Next(100000, 999999).ToString();

            // Lưu OTP vào Cache, có giá trị trong 5 phút
            _cache.Set($"OTP_{request.Email}", otp, TimeSpan.FromMinutes(5));

            try
            {
                string fromEmail = "phuc512dz@gmail.com";
                string appPassword = "zbkwoezmagdmxmcg";

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, appPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "Trạm Giày Sneaker - Bảo Mật"),
                    Subject = "[Trạm Giày Sneaker] Mã xác thực lấy lại mật khẩu",
                    Body = $@"
                        <div style='font-family: Arial; padding: 20px; border: 1px solid #ddd; border-radius: 10px; max-width: 500px; margin: auto;'>
                            <h2 style='color: #dc3545; text-align: center;'>MÃ XÁC THỰC OTP</h2>
                            <p>Chào bạn,</p>
                            <p>Bạn vừa yêu cầu đặt lại mật khẩu tại hệ thống Trạm Giày Sneaker. Dưới đây là mã xác thực (OTP) của bạn:</p>
                            <div style='text-align: center; margin: 20px 0;'>
                                <span style='font-size: 30px; font-weight: bold; background: #f8f9fa; padding: 10px 20px; border-radius: 8px; letter-spacing: 5px; color: #212529;'>{otp}</span>
                            </div>
                            <p style='color: #666; font-size: 13px;'>* Mã này chỉ có hiệu lực trong vòng 5 phút. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                        </div>
                    ",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(request.Email);

                await smtpClient.SendMailAsync(mailMessage);
                return Ok(new { message = "Mã OTP đã được gửi đến Email của bạn!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi gửi OTP: " + ex.Message);
                return StatusCode(500, new { message = "Lỗi hệ thống khi gửi Email!" });
            }
        }

        // ========================================================
        // 🚀 2. API XÁC NHẬN OTP & ĐỔI MẬT KHẨU
        // ========================================================
        [HttpPost("reset-password-with-otp")]
        public IActionResult ResetPasswordWithOtp([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp) || string.IsNullOrEmpty(request.NewPassword))
                return BadRequest(new { message = "Vui lòng điền đầy đủ thông tin!" });

            // Kiểm tra OTP trong Cache
            if (!_cache.TryGetValue($"OTP_{request.Email}", out string savedOtp))
            {
                return BadRequest(new { message = "Mã OTP đã hết hạn hoặc không tồn tại. Vui lòng yêu cầu gửi lại!" });
            }

            if (savedOtp != request.Otp)
            {
                return BadRequest(new { message = "Mã OTP không chính xác!" });
            }

            // Đổi mật khẩu
            var user = _context.Customers.FirstOrDefault(c => c.Email == request.Email);
            if (user == null) return NotFound(new { message = "Không tìm thấy tài khoản!" });

            user.Password = request.NewPassword;
            _context.SaveChanges();

            // Xóa OTP khỏi Cache sau khi dùng xong
            _cache.Remove($"OTP_{request.Email}");

            return Ok(new { message = "Đặt lại mật khẩu thành công!" });
        }

        // --- CÁC CLASS NHẬN DỮ LIỆU ---
        public class UpdateProfileModel
        {
            public string Name { get; set; }
            public string Phone { get; set; }
            public string? Address { get; set; }
            public string? NewPassword { get; set; }
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
        public class ForgotPasswordRequest
        {
            public string Email { get; set; }
        }
        public class ResetPasswordRequest
        {
            public string Email { get; set; }
            public string Otp { get; set; }
            public string NewPassword { get; set; }
        }
    }
}