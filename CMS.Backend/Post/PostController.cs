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
        // Tham số 'id' được truyền vào từ URL (ví dụ: /Post/Index/5)
        public IActionResult Index(int? id)
        {
            // Tạo truy vấn ban đầu lấy kèm bảng Category
            var query = _context.Posts.Include(p => p.Category).AsQueryable();

            // 1. Nếu người dùng CÓ truyền id -> lọc theo Danh mục
            if (id != null)
            {
                query = query.Where(p => p.CategoryId == id);
            }
            // Ngược lại nếu id == null -> Bỏ qua không lọc, hệ thống tự lấy hết bài viết

            // 2. Sắp xếp theo ngày mới nhất và chuyển thành List
            var posts = query.OrderByDescending(p => p.CreatedDate).ToList();

            // 3. Truyền dữ liệu ra View
            return View(posts);
        }


        // GET: Post/Details/5
        public IActionResult Details(int id)
        {
            // 1. Truy vấn bài viết theo ID
            // Sử dụng .Include(p => p.Category) để lấy kèm thông tin Danh mục (Join bảng)
            var post = _context.Posts
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            // 2. Kiểm tra nếu không tìm thấy bài viết (tránh lỗi màn hình trắng)
            if (post == null)
            {
                return NotFound(); // Trả về trang lỗi 404
            }

            // 3. Truyền dữ liệu sang View
            return View(post);
        }


    }
}