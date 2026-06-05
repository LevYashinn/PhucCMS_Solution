using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using Microsoft.EntityFrameworkCore; // BẮT BUỘC THÊM: Để sử dụng được lệnh .Include()
using System.Linq;

namespace CMS.Backend.Controllers
{
    public class OrderDetailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action hiển thị danh sách chi tiết đơn hàng
        public IActionResult Index()
        {
            // ĐÃ SỬA: Dùng .Include để liên kết (JOIN) sang bảng Product và Order trong SQL Server
            var orderDetails = _context.OrderDetails
                                       .Include(od => od.Product) // Nạp kèm dữ liệu sản phẩm để lấy tên sản phẩm
                                       .OrderByDescending(od => od.Id) // Sắp xếp chi tiết mới lên đầu (tùy chọn)
                                       .ToList();

            return View(orderDetails);
        }
    }
}