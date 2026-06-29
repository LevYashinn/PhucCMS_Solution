using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore; // BẮT BUỘC THÊM DÒNG NÀY ĐỂ DÙNG INCLUDE

namespace CMS.Backend.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========================================================
        // 1. Action hiển thị danh sách tất cả đơn hàng (Trang chủ Admin Order)
        // ========================================================
        public IActionResult Index()
        {
            // Lấy danh sách kèm tên Khách hàng + Sắp xếp đơn mới nhất lên đầu
            var orders = _context.Orders
                                 .Include(o => o.Customer)
                                 .OrderByDescending(o => o.Id) // 🌟 Hiển thị đơn mới nhất lên trên cùng
                                 .ToList();

            return View(orders);
        }

        // ========================================================
        // 2. Action xem chi tiết đơn hàng (Xem khách mua giày gì)
        // ========================================================
        public IActionResult Details(int id)
        {
            // Tìm đơn hàng theo ID, kèm theo Khách hàng và Chi tiết giày đã mua
            var order = _context.Orders
                                .Include(o => o.Customer)
                                .Include(o => o.OrderDetails)         // Lấy danh sách chi tiết đơn
                                    .ThenInclude(od => od.Product)    // 🌟 Móc nối lấy luôn hình ảnh, tên Giày từ bảng Product
                                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound(); // Không tìm thấy đơn hàng thì báo lỗi 404
            }

            return View(order);
        }
    }
}