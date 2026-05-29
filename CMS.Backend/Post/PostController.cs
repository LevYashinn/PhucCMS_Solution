using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CMS.Backend.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Inject DbContext
        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách bài viết từ SQL Server
        public IActionResult Index()
        {
            // Lấy dữ liệu thật từ bảng Posts
            var posts = _context.Posts
                                .Include(p => p.Category)
                                .ToList();

            return View(posts);
        }

        // Chi tiết bài viết
        public IActionResult Details(int id)
        {
            // Tìm bài viết theo Id
            var post = _context.Posts
                               .Include(p => p.Category)
                               .FirstOrDefault(p => p.Id == id);

            // Nếu không tồn tại
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }
    }
}