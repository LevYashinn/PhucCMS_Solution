using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore; // 1. BẮT BUỘC THÊM DÒNG NÀY

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
            // 2. THÊM .Include(o => o.Customer) VÀO TRƯỚC .ToList()
            var orders = _context.Orders
                                 .Include(o => o.Customer)
                                 .ToList();

            return View(orders);
        }
    }
}