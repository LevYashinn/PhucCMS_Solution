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

        // 1. Action hiển thị danh sách sản phẩm (ĐÃ THÊM LỌC DANH MỤC)
        public IActionResult Index(int? categoryId)
        {
            // Bắt đầu với truy vấn tất cả sản phẩm
            var query = _context.Products
                                .Include(p => p.CategoryProduct)
                                .AsQueryable();

            // Nếu người dùng chọn danh mục (categoryId có giá trị), lọc theo ID đó
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryProductId == categoryId.Value);
            }

            var products = query.OrderByDescending(p => p.Id).ToList();

            // Lưu trạng thái danh mục để Highlight trên menu (nếu cần)
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Categories = _context.CategoryProducts.ToList(); // Đảm bảo sidebar có dữ liệu

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

        // 4. GET: Hiển thị form Sửa sản phẩm
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
        public IActionResult Edit(Product model, IFormFile? uploadImage)
        {
            ModelState.Remove("CategoryProduct");
            ModelState.Remove("uploadImage");

            if (model.CategoryProductId <= 0)
            {
                ModelState.AddModelError("CategoryProductId", "Vui lòng chọn danh mục sản phẩm.");
            }

            if (ModelState.IsValid)
            {
                var productInDb = _context.Products.FirstOrDefault(p => p.Id == model.Id);
                if (productInDb == null) return NotFound();

                productInDb.Name = model.Name;
                productInDb.CategoryProductId = model.CategoryProductId;
                productInDb.Price = model.Price;
                productInDb.StockQuantity = model.StockQuantity;
                productInDb.Description = model.Description;

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

                    if (!string.IsNullOrEmpty(productInDb.ImageUrl))
                    {
                        string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", productInDb.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    productInDb.ImageUrl = "/uploads/" + fileName;
                }

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(_context.CategoryProducts.ToList(), "Id", "Name", model.CategoryProductId);
            return View(model);
        }

        // 6. Xóa sản phẩm
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                }
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // 7. Chi tiết
        public IActionResult Details(int id)
        {
            var product = _context.Products.Include(p => p.CategoryProduct).FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}