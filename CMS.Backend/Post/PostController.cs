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
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CMS.Backend.Controllers
{
    [Authorize] // Bắt buộc phải đăng nhập mới được vào các hàm quản trị bên dưới
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 🌟 1. HIỂN THỊ DANH SÁCH BÀI VIẾT (ĐÃ THÊM PHÂN TRANG)
        // ==========================================
        public IActionResult Index(int? id, int page = 1)
        {
            int pageSize = 6; // Hiện 6 bài/trang (chia làm 2 hàng ngang rất đẹp)

            var query = _context.Posts.Include(p => p.Category).AsQueryable();

            // Nếu có lọc theo danh mục
            if (id != null)
            {
                query = query.Where(p => p.CategoryId == id);
                ViewBag.CurrentCategoryId = id; // Lưu lại ID để gắn vào link phân trang
            }

            // Tính toán tổng số lượng và số trang
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Cắt lấy dữ liệu trang hiện tại
            var posts = query.OrderByDescending(p => p.CreatedDate)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();

            // Đẩy dữ liệu phân trang xuống View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(posts);
        }

        // ==========================================
        // CÁC HÀM BÊN DƯỚI GIỮ NGUYÊN HOÀN TOÀN CỦA BẠN
        // ==========================================

        // 2. Chi tiết bài viết (Admin/Khách)
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var post = _context.Posts
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            if (post == null) return NotFound();

            return View(post);
        }

        // 3. GET: Giao diện Thêm mới (Admin)
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // 4. POST: Xử lý Thêm mới bài viết
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

        // 5. GET: Giao diện Cập nhật (Admin)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null) return NotFound();

            ViewBag.CategoryList = new SelectList(_context.Categories, "Id", "Name", post.CategoryId);
            return View(post);
        }

        // 6. POST: Xử lý Cập nhật bài viết
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
                var postInDb = _context.Posts.FirstOrDefault(p => p.Id == model.Id);
                if (postInDb == null) return NotFound();

                postInDb.Title = model.Title;
                postInDb.CategoryId = model.CategoryId;
                postInDb.Content = model.Content;

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

                    // Xóa ảnh cũ cho nhẹ Server
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

        // 7. POST/GET: Thực thi Xóa bài viết
        public IActionResult Delete(int id)
        {
            var post = _context.Posts.Find(id);
            if (post != null)
            {
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
        // 🚀 CỬA PHỤ CHO REACT: LẤY BÀI VIẾT (ĐÃ FIX LỖI 500 SERVER)
        // =========================================================================
        [AllowAnonymous]
        [HttpGet("api/get-posts")]
        public IActionResult GetPostsForReact()
        {
            var rawPosts = _context.Posts
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            var posts = rawPosts.Select(p => new
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                ImageUrl = p.ImageUrl,
                CreatedDate = p.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "Không phân loại"
            });

            return Json(posts);
        }

        [AllowAnonymous]
        [HttpGet("api/get-post-categories")]
        public IActionResult GetPostCategoriesForReact()
        {
            var categories = _context.Categories
                .OrderByDescending(c => c.Id)
                .ToList()
                .Select(c => new
                {
                    Id = c.Id,
                    Name = c.Name
                });

            return Json(categories);
        }

        [HttpPost]
        public IActionResult UploadImageEditor(IFormFile upload)
        {
            if (upload != null && upload.Length > 0)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(upload.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    upload.CopyTo(stream);
                }

                string url = $"/uploads/{fileName}";

                return Json(new { uploaded = 1, fileName = fileName, url = url });
            }

            return Json(new { uploaded = 0, error = new { message = "Lỗi: Không thể tải ảnh lên máy chủ!" } });
        }

        [AllowAnonymous]
        [HttpPost("api/contact/send-message")]
        public async Task<IActionResult> ReceiveContactMessage([FromBody] ContactRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "Vui lòng điền đầy đủ các thông tin bắt buộc!" });
            }

            try
            {
                string fromEmail = "phuc512dz@gmail.com";
                string appPassword = "zbkwoezmagdmxmcg";

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, appPassword),
                    EnableSsl = true,
                };

                string body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.05);'>
                        <div style='background-color: #212529; padding: 20px; text-align: center; color: white;'>
                            <h2 style='margin: 0; text-transform: uppercase; letter-spacing: 1px;'>Khách Hàng Liên Hệ Mới</h2>
                        </div>
                        <div style='padding: 25px; bg-color: #ffffff;'>
                            <p style='font-size: 15px; color: #333;'>Hệ thống <strong>Trạm Giày Sneaker</strong> vừa ghi nhận một lời nhắn mới từ khách hàng qua biểu mẫu liên hệ trực tuyến:</p>
                            
                            <table style='width: 100%; border-collapse: collapse; margin-top: 20px; margin-bottom: 20px;'>
                                <tr style='background-color: #f8f9fa;'>
                                    <td style='padding: 12px; border: 1px solid #eee; font-weight: bold; width: 30%; color: #555;'>Họ và tên:</td>
                                    <td style='padding: 12px; border: 1px solid #eee; color: #212529;'>{request.Name}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 12px; border: 1px solid #eee; font-weight: bold; color: #555;'>Số điện thoại:</td>
                                    <td style='padding: 12px; border: 1px solid #eee; color: #212529;'>{request.Phone}</td>
                                </tr>
                                <tr style='background-color: #f8f9fa;'>
                                    <td style='padding: 12px; border: 1px solid #eee; font-weight: bold; color: #555;'>Địa chỉ Email:</td>
                                    <td style='padding: 12px; border: 1px solid #eee; color: #0d6efd;'><strong>{request.Email}</strong></td>
                                </tr>
                                <tr>
                                    <td style='padding: 12px; border: 1px solid #eee; font-weight: bold; color: #555; vertical-align: top;'>Nội dung lời nhắn:</td>
                                    <td style='padding: 12px; border: 1px solid #eee; color: #212529; line-height: 1.5; white-space: pre-line;'>{request.Message}</td>
                                </tr>
                            </table>
                            
                            <hr style='border: 0; border-top: 1px solid #eee; margin: 25px 0;' />
                            <p style='font-size: 13px; color: #666; fst-italic: italic; text-align: center;'>
                                Bạn có thể bấm nút Reply (Phản hồi) trực tiếp trên email này để trả lời cho khách hàng qua địa chỉ <strong>{request.Email}</strong>.
                            </p>
                        </div>
                    </div>
                ";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "Trạm Giày Sneaker - Liên Hệ"),
                    Subject = $"[Trạm Giày Sneaker] Lời nhắn mới từ khách hàng: {request.Name}",
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.ReplyToList.Add(new MailAddress(request.Email));
                mailMessage.To.Add("phuc512dz@gmail.com");

                await smtpClient.SendMailAsync(mailMessage);

                return Ok(new { success = true, message = "Lời nhắn của bạn đã được gửi thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi thực thi gửi email liên hệ: " + ex.Message);
                return StatusCode(500, new { message = "Lỗi hệ thống khi gửi email liên hệ!" });
            }
        }
    }

    public class ContactRequest
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
    }
}