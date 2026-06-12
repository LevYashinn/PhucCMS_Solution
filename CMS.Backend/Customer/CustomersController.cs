using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using System.Linq;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/customers
        // 1. Lấy danh sách toàn bộ khách hàng
        [HttpGet]
        public IActionResult GetAll()
        {
            var customers = _context.Customers
                .OrderByDescending(c => c.Id) // Khách hàng mới đăng ký lên đầu
                .Select(c => new {
                    c.Id,
                    c.FullName,
                    c.Email,
                    c.Phone,
                    c.Address
                    // LƯU Ý: Tuyệt đối không Select trường Password ra đây để bảo mật
                })
                .ToList();

            return Ok(customers);
        }

        // GET: api/customers/search/nguyen
        // 2. Tìm kiếm khách hàng theo tên hoặc số điện thoại (Chức năng mở rộng rất hay dùng)
        [HttpGet("search/{keyword}")]
        public IActionResult Search(string keyword)
        {
            var customers = _context.Customers
                .Where(c => c.FullName.Contains(keyword) || c.Phone.Contains(keyword))
                .Select(c => new {
                    c.Id,
                    c.FullName,
                    c.Email,
                    c.Phone,
                    c.Address
                })
                .ToList();

            return Ok(customers);
        }

        // GET: api/customers/5
        // 3. Lấy chi tiết thông tin 1 khách hàng kèm lịch sử mua hàng của họ
        [HttpGet("{id}")]
        public IActionResult GetDetail(int id)
        {
            var customer = _context.Customers
                .Where(c => c.Id == id)
                .Select(c => new {
                    // Thông tin cá nhân
                    c.Id,
                    c.FullName,
                    c.Email,
                    c.Phone,
                    c.Address,

                    // Kéo theo danh sách các đơn hàng người này đã từng đặt
                    OrderHistory = c.Orders.Select(o => new {
                        o.Id,
                        o.OrderDate,
                        o.Status,
                        o.Notes
                    }).ToList()
                })
                .FirstOrDefault();

            if (customer == null)
            {
                return NotFound(new { message = "Không tìm thấy khách hàng này trong hệ thống" });
            }

            return Ok(customer);
        }
    }
}