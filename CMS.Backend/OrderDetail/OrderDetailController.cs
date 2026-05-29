using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore; // BẮT BUỘC THÊM DÒNG NÀY ĐỂ DÙNG INCLUDE

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
            // ĐÃ SỬA: Thêm .Include(od => od.Product) để lấy được Tên Sản Phẩm
            var orderDetails = _context.OrderDetails
                                       .Include(od => od.Product)
                                       .ToList();

            return View(orderDetails);
        }
    }
}