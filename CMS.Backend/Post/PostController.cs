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

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Hiển thị danh sách bài viết từ SQL Server
        public IActionResult Index(int? id)
        {
            var query = _context.Posts.Include(p => p.Category).AsQueryable();

            if (id != null)
            {
                query = query.Where(p => p.CategoryId == id);
            }

            var posts = query.OrderByDescending(p => p.CreatedDate).ToList();
            return View(posts);
        }

        // 2. Chi tiết bài viết
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var post = _context.Posts
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            if (post == null) return NotFound();

            return View(post);
        }

        // 3. GET: Giao diện Thêm mới
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // 4. POST: Xử lý Thêm mới (ĐÃ ĐỒNG BỘ LOGIC NHƯ PRODUCT)
        [HttpPost]
        public IActionResult Create(Post model, IFormFile uploadImage)
        {
            ModelState.Remove("Category");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("uploadImage");

            if (model.CategoryId <= 0) ModelState.AddModelError("CategoryId", "Vui lòng chọn danh mục.");

            // Tự động gán ngày tạo là ngày hôm nay
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

        // 5. GET: Giao diện Cập nhật
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null) return NotFound();

            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", post.CategoryId);
            return View(post);
        }

        // 6. POST: Xử lý Cập nhật (ĐÃ ĐỒNG BỘ LOGIC TÌM VÀ MAP DATA NHƯ PRODUCT)
        [HttpPost]
        public IActionResult Edit(Post model, IFormFile? uploadImage)
        {
            ModelState.Remove("Category");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("uploadImage");

            if (model.CategoryId <= 0)
            {
                ModelState.AddModelError("CategoryId", "Vui lòng chọn danh mục bài viết.");
            }

            if (ModelState.IsValid)
            {
                // 🌟 TÌM BÀI VIẾT TRONG DB ĐỂ CẬP NHẬT (GIÚP KHÔNG BỊ MẤT DỮ LIỆU CŨ)
                var postInDb = _context.Posts.FirstOrDefault(p => p.Id == model.Id);
                if (postInDb == null) return NotFound();

                postInDb.Title = model.Title;
                postInDb.CategoryId = model.CategoryId;
                postInDb.Content = model.Content;
                // KHÔNG cập nhật CreatedDate để giữ nguyên ngày đăng bài ban đầu

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

                    // 🌟 Xóa ảnh cũ đi cho nhẹ Server giống hệt bên Product
                    if (!string.IsNullOrEmpty(postInDb.ImageUrl))
                    {
                        string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", postInDb.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                    }

                    postInDb.ImageUrl = "/uploads/" + fileName;
                }

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // 7. POST/GET: Thực thi Xóa
        public IActionResult Delete(int id)
        {
            var post = _context.Posts.Find(id);
            if (post != null)
            {
                // Xóa luôn hình ảnh vật lý trong thư mục uploads
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

        // =========================================================================
        // 🌟 API HỖ TRỢ UPLOAD ẢNH TỪ BÊN TRONG KHUNG SOẠN THẢO CKEDITOR
        // =========================================================================
        [HttpPost]
        public IActionResult UploadImageEditor(IFormFile upload)
        {
            if (upload != null && upload.Length > 0)
            {
                // Lưu ảnh vào thư mục wwwroot/uploads
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(upload.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    upload.CopyTo(stream);
                }

                string url = $"/uploads/{fileName}";

                // Trả về JSON theo đúng chuẩn mà thư viện CKEditor yêu cầu
                return Json(new { uploaded = 1, fileName = fileName, url = url });
            }

            return Json(new { uploaded = 0, error = new { message = "Lỗi: Không thể tải ảnh lên máy chủ!" } });
        }
    }
}