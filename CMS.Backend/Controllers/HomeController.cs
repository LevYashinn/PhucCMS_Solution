using CMS.Backend.Models;
using CMS.Data;
using CMS.Data.Entities; // Bắt buộc thêm dòng này để truy cập các Entity
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

        public IActionResult Index(int? categoryId)
        {
            // 1. Luôn lấy tin tức mới nhất (đảm bảo không bao giờ bị mất)
            var latestPosts = _context.Posts
                                     .Include(p => p.Category) // Đảm bảo đã Include Category để hiển thị tên danh mục bài viết
                                     .OrderByDescending(p => p.CreatedDate)
                                     .Take(3)
                                     .ToList();

            // 2. Lấy danh mục để hiển thị trên menu lọc sản phẩm
            ViewBag.Categories = _context.CategoryProducts.ToList();

            // 3. Xử lý logic lọc sản phẩm
            var productsQuery = _context.Products.Include(p => p.CategoryProduct).AsQueryable();

            if (categoryId.HasValue)
            {
                // Lọc sản phẩm theo danh mục nếu có categoryId
                productsQuery = productsQuery.Where(p => p.CategoryProductId == categoryId.Value);
            }

            ViewBag.Products = productsQuery.OrderByDescending(p => p.Id).Take(6).ToList();
            ViewBag.SelectedCategoryId = categoryId; // Để giữ trạng thái nút đang chọn

            // 4. TRẢ VỀ MODEL LÀ DANH SÁCH BÀI VIẾT (Tin tức)
            // Dòng này rất quan trọng để @model ở file Index.cshtml nhận được dữ liệu
            return View(latestPosts);
        }

        public IActionResult Dashboard()
        {
            // 1. Thống kê số lượng
            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalPosts = _context.Posts.Count();
            ViewBag.TotalCategories = _context.CategoryProducts.Count();
            ViewBag.TotalUsers = _context.Users.Count();

            // 2. Lấy 5 đơn hàng gần đây
            ViewBag.RecentOrders = _context.Orders
                                           .Include(o => o.Customer)
                                           .OrderByDescending(o => o.OrderDate)
                                           .Take(5)
                                           .ToList();

            // 3. Lấy sản phẩm tồn kho thấp (<= 5)
            ViewBag.LowStockProducts = _context.Products
                                               .Where(p => p.StockQuantity <= 5)
                                               .OrderBy(p => p.StockQuantity)
                                               .Take(5)
                                               .ToList();

            return View();
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
    }
}