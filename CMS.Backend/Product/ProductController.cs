using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Http; // Bắt buộc phải có để dùng IFormFile nhận file ảnh
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO; // Bắt buộc phải có để xử lý Path, Directory, FileStream khi upload ảnh
using System.Linq;

namespace CMS.Backend.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Tiêm DbContext kết nối SQL Server vào
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Action hiển thị danh sách sản phẩm
        public IActionResult Index()
        {
            var products = _context.Products.OrderByDescending(p => p.Id).ToList();
            return View(products);
        }

        // 2. GET: Hiển thị form Thêm sản phẩm mới
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 3. POST: Xử lý Thêm sản phẩm mới kèm upload ảnh
        [HttpPost]
        public IActionResult Create(Product model, IFormFile uploadImage)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh nếu người dùng có chọn file
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

                _context.Products.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // Nếu form nhập bị lỗi ràng buộc dữ liệu, trả lại chính dữ liệu đó kèm thông báo lỗi
            return View(model);
        }

        // 4. GET: Hiển thị form Sửa sản phẩm kèm dữ liệu cũ
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // 5. POST: Xử lý Cập nhật thay đổi sản phẩm
        [HttpPost]
        public IActionResult Edit(Product model, IFormFile uploadImage)
        {
            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.Length > 0)
                {
                    // Quy trình upload ảnh mới thay thế
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
                else
                {
                    // Nếu không chọn ảnh mới, lấy lại đường dẫn ảnh cũ từ DB để giữ nguyên
                    var oldProduct = _context.Products.AsNoTracking().FirstOrDefault(p => p.Id == model.Id);
                    if (oldProduct != null && string.IsNullOrEmpty(model.ImageUrl))
                    {
                        model.ImageUrl = oldProduct.ImageUrl;
                    }
                }

                _context.Products.Update(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // 6. GET/POST: Thực thi Xóa sản phẩm ra khỏi Database
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                // Thao tác xóa file vật lý trong thư mục wwwroot nếu muốn giải phóng bộ nhớ (Tùy chọn)
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                _context.Products.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}