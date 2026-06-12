using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using System.Linq;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/orders
        // 1. Lấy toàn bộ danh sách đơn hàng (Dành cho Admin)
        [HttpGet]
        public IActionResult GetAll()
        {
            var orders = _context.Orders
                .OrderByDescending(o => o.Id) // Đơn hàng mới nhất xếp lên đầu
                .Select(o => new {
                    o.Id,
                    o.OrderDate,
                    o.Status, // 0: Chờ duyệt, 1: Đang giao, 2: Đã xong
                    o.Notes,
                    CustomerName = o.Customer.FullName // Kéo tên khách hàng từ bảng Customers
                })
                .ToList();

            return Ok(orders);
        }

        // GET: api/orders/customer/5
        // 2. Lấy lịch sử đơn hàng của một khách hàng cụ thể
        [HttpGet("customer/{customerId}")]
        public IActionResult GetByCustomer(int customerId)
        {
            var orders = _context.Orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.Id)
                .Select(o => new {
                    o.Id,
                    o.OrderDate,
                    o.Status,
                    o.Notes
                })
                .ToList();

            return Ok(orders);
        }

        // GET: api/orders/5
        // 3. Lấy chi tiết một đơn hàng (Bao gồm thông tin người mua và danh sách giày đã mua)
        [HttpGet("{id}")]
        public IActionResult GetDetail(int id)
        {
            var order = _context.Orders
                .Where(o => o.Id == id)
                .Select(o => new {
                    o.Id,
                    o.OrderDate,
                    o.Status,
                    o.Notes,
                    // Thông tin người nhận
                    CustomerInfo = new
                    {
                        o.Customer.FullName,
                        o.Customer.Phone,
                        o.Customer.Address
                    },
                    // Danh sách sản phẩm trong đơn hàng này
                    OrderItems = o.OrderDetails.Select(od => new {
                        od.ProductId,
                        ProductName = od.Product.Name,
                        od.Quantity,
                        od.UnitPrice,
                        TotalPrice = od.Quantity * od.UnitPrice // Tính tổng tiền từng dòng
                    }).ToList()
                })
                .FirstOrDefault();

            if (order == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng này trong hệ thống" });
            }

            return Ok(order);
        }
    }
}