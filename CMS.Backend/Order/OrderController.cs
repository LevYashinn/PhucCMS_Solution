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

        // Action hiển thị danh sách đơn hàng
        public IActionResult Index()
        {
            // ĐÃ SỬA: Thêm .Include(o => o.Customer) để lấy kèm thông tin tên Khách hàng
            var orders = _context.Orders
                                 .Include(o => o.Customer)
                                 .ToList();

            return View(orders);
        }
    }
}