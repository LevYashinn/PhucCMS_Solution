using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using CMS.Data.Entities;
using System.Linq;

namespace CMS.Backend.Account // Đã đổi namespace cho khớp với thư mục của bạn
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] Customer customer)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var isEmailExist = _context.Customers.Any(c => c.Email == customer.Email);
            if (isEmailExist) return BadRequest(new { message = "Email này đã được sử dụng!" });

            _context.Customers.Add(customer);
            _context.SaveChanges();

            return Ok(new { message = "Đăng ký thành công!", customerId = customer.Id });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var customer = _context.Customers
                .FirstOrDefault(c => c.Email == request.Email && c.Password == request.Password);

            if (customer == null) return Unauthorized(new { message = "Email hoặc mật khẩu sai!" });

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                user = new { customer.Id, customer.FullName, customer.Email, customer.Phone, customer.Address }
            });
        }
    }

    // Class phụ phải nằm TRONG cùng namespace nhưng NGOÀI class AccountsController
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}