using CMS.Backend.Models;
using CMS.Data;
using CMS.Data.Entities; // Bổ sung để Controller hiểu các class Product, Order...
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

namespace CMS.Backend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var latestPosts = _context.Posts
                                      .Include(p => p.Category)
                                      .OrderByDescending(p => p.CreatedDate)
                                      .Take(3)
                                      .ToList();
            return View(latestPosts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // HÀM DASHBOARD ĐÃ ĐƯỢC NÂNG CẤP LẤY DỮ LIỆU THẬT 100%
        public IActionResult Dashboard()
        {
            // 1. Số liệu cho 4 thẻ thống kê trên cùng
            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalPosts = _context.Posts.Count();
            ViewBag.TotalCategories = _context.CategoryProducts.Count();
            ViewBag.TotalUsers = _context.Users.Count();

            // 2. Lấy danh sách 5 đơn hàng mới nhất (Kèm theo thông tin Khách hàng)
            var recentOrders = _context.Orders
                                       .Include(o => o.Customer)
                                       .OrderByDescending(o => o.OrderDate)
                                       .Take(5)
                                       .ToList();
            ViewBag.RecentOrders = recentOrders;

            // 3. Lấy danh sách các sản phẩm sắp hết hàng (Tồn kho từ 5 đôi trở xuống)
            var lowStockProducts = _context.Products
                                           .Where(p => p.StockQuantity <= 5)
                                           .OrderBy(p => p.StockQuantity)
                                           .Take(5)
                                           .ToList();
            ViewBag.LowStockProducts = lowStockProducts;

            return View();
        }
    }
}