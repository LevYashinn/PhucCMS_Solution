using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;

namespace CMS.Backend.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Action hiển thị danh sách sản phẩm
        public IActionResult Index()
        {
            var products = _context.Products
                                   .Include(p => p.CategoryProduct)
                                   .OrderByDescending(p => p.Id)
                                   .ToList();
            return View(products);
        }

        // 2. GET: Hiển thị form Thêm sản phẩm mới
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.CategoryProducts.ToList(), "Id", "Name");
            return View();
        }

        // 3. POST: Xử lý Thêm sản phẩm mới kèm upload ảnh
        [HttpPost]
        public IActionResult Create(Product model, IFormFile uploadImage)
        {
            // Bỏ qua validation cho Navigation Property để tránh lỗi ModelState false ảo
            ModelState.Remove("CategoryProduct");

            if (model.CategoryProductId <= 0)
            {
                ModelState.AddModelError("CategoryProductId", "Vui lòng chọn danh mục sản phẩm.");
            }

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

                _context.Products.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(_context.CategoryProducts.ToList(), "Id", "Name");
            return View(model);
        }

        // 4. GET: Hiển thị form Sửa sản phẩm kèm dữ liệu cũ
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(_context.CategoryProducts.ToList(), "Id", "Name", product.CategoryProductId);
            return View(product);
        }

        // 5. POST: Xử lý Cập nhật thay đổi sản phẩm
        [HttpPost]
        public IActionResult Edit(Product model, IFormFile uploadImage)
        {
            // BẮT BUỘC THÊM DÒNG NÀY ĐỂ FIX LỖI VALIDATION ẢO
            ModelState.Remove("CategoryProduct");

            if (model.CategoryProductId <= 0)
            {
                ModelState.AddModelError("CategoryProductId", "Vui lòng chọn danh mục sản phẩm.");
            }

            if (ModelState.IsValid)
            {
                // TÌM SẢN PHẨM TRONG DB ĐỂ CẬP NHẬT THAY VÌ DÙNG LỆNH UPDATE(MODEL)
                var productInDb = _context.Products.FirstOrDefault(p => p.Id == model.Id);
                if (productInDb == null) return NotFound();

                // Cập nhật các trường thông tin cơ bản
                productInDb.Name = model.Name;
                productInDb.CategoryProductId = model.CategoryProductId;
                productInDb.Price = model.Price;
                productInDb.StockQuantity = model.StockQuantity;
                productInDb.Description = model.Description;

                // Xử lý ảnh nếu người dùng có chọn ảnh mới
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

                    // Xóa ảnh vật lý cũ cho nhẹ ổ cứng (nếu có)
                    if (!string.IsNullOrEmpty(productInDb.ImageUrl))
                    {
                        string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", productInDb.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Cập nhật đường dẫn ảnh mới vào DB
                    productInDb.ImageUrl = "/uploads/" + fileName;
                }
                // Nếu uploadImage == null, productInDb.ImageUrl sẽ tự động giữ nguyên ảnh cũ

                // Lưu lại thay đổi
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // Nếu form có lỗi nhập liệu, nạp lại ViewBag và trả về View
            ViewBag.Categories = new SelectList(_context.CategoryProducts.ToList(), "Id", "Name", model.CategoryProductId);
            return View(model);
        }

        // 6. GET/POST: Thực thi Xóa sản phẩm ra khỏi Database
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
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