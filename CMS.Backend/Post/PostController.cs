using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;

namespace CMS.Backend.Controllers
{
    [Authorize] // Bắt buộc phải đăng nhập mới được vào các hàm bên dưới
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Inject DbContext
        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách bài viết từ SQL Server
        public IActionResult Index(int? id)
        {
            var query = _context.Posts.Include(p => p.Category).AsQueryable();

            // Nếu người dùng CÓ truyền id -> lọc theo Danh mục
            if (id != null)
            {
                query = query.Where(p => p.CategoryId == id);
            }

            // Sắp xếp theo ngày mới nhất và chuyển thành List
            var posts = query.OrderByDescending(p => p.CreatedDate).ToList();

            return View(posts);
        }

        // Chi tiết bài viết
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var post = _context.Posts
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Giao diện Thêm mới
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // ===============================================
        // POST: Xử lý Thêm mới (ĐÃ FIX LỖI KHÔNG LƯU)
        // ===============================================
        [HttpPost]
        public IActionResult Create(Post model, IFormFile uploadImage)
        {
            // 🌟 CÁC DÒNG QUAN TRỌNG ĐỂ VƯỢ QUA KIỂM DUYỆT ẢNH CỦA C#
            ModelState.Remove("Category");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("uploadImage");

            // 🌟 Tự động gán ngày tạo là ngày hôm nay
            model.CreatedDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.Length > 0)
                {
                    string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadImage.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        uploadImage.CopyTo(stream);
                    }

                    model.ImageUrl = "/uploads/" + fileName;
                }

                _context.Posts.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // GET: Giao diện Cập nhật
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null) return NotFound();

            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", post.CategoryId);
            return View(post);
        }

        // ===============================================
        // POST: Xử lý Cập nhật (ĐÃ FIX LỖI KHÔNG LƯU)
        // ===============================================
        [HttpPost]
        public IActionResult Edit(Post model, IFormFile? uploadImage)
        {
            // 🌟 CÁC DÒNG QUAN TRỌNG ĐỂ VƯỢ QUA KIỂM DUYỆT
            ModelState.Remove("Category");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("uploadImage");

            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.Length > 0)
                {
                    string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadImage.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        uploadImage.CopyTo(stream);
                    }

                    // Nếu có ảnh mới thì lấy link ảnh mới
                    model.ImageUrl = "/uploads/" + fileName;
                }
                else
                {
                    // Nếu không upload ảnh mới, giữ lại đường dẫn ảnh cũ
                    var oldPost = _context.Posts.AsNoTracking().FirstOrDefault(p => p.Id == model.Id);
                    if (oldPost != null && string.IsNullOrEmpty(model.ImageUrl))
                    {
                        model.ImageUrl = oldPost.ImageUrl;
                    }
                }

                _context.Posts.Update(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // POST/GET: Thực thi Xóa
        public IActionResult Delete(int id)
        {
            var post = _context.Posts.Find(id);
            if (post != null)
            {
                // Xóa luôn hình ảnh vật lý trong thư mục uploads để đỡ nặng máy chủ
                if (!string.IsNullOrEmpty(post.ImageUrl))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                }

                _context.Posts.Remove(post);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // =========================================================================
        // 🚀 CỬA PHỤ CHO REACT: LẤY BÀI VIẾT (Vượt mặt [Authorize] ở trên cùng)
        // =========================================================================
        [AllowAnonymous]
        [HttpGet("api/get-posts")]
        public IActionResult GetPostsForReact()
        {
            var posts = _context.Posts.OrderByDescending(p => p.CreatedDate).ToList();
            return Json(posts);
        }
    }
}