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
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Action hiển thị danh sách sản phẩm (Bao gồm cả lọc danh mục cho Admin)
        public IActionResult Index(int? categoryId)
        {
            var query = _context.Products
                                .Include(p => p.CategoryProduct)
                                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryProductId == categoryId.Value);
                ViewBag.SelectedCategoryId = categoryId;
            }

            var products = query.OrderByDescending(p => p.Id).ToList();
            ViewBag.Categories = _context.CategoryProducts.ToList();

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
            if (model.CategoryProductId <= 0) ModelState.AddModelError("CategoryProductId", "Vui lòng chọn danh mục.");

            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.Length > 0)
                {
                    string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadImage.FileName);
                    string filePath = Path.Combine(folder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create)) uploadImage.CopyTo(stream);
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

        // 5. POST: Xử lý Cập nhật
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
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                    }
                    productInDb.ImageUrl = "/uploads/" + fileName;
                }

                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Categories = new SelectList(_context.CategoryProducts.ToList(), "Id", "Name", model.CategoryProductId);
            return View(model);
        }

        // 6. GET/POST: Thực thi Xóa sản phẩm
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

        // 7. Chi tiết sản phẩm
        public IActionResult Details(int id)
        {
            var product = _context.Products.Include(p => p.CategoryProduct).FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // =========================================================================
        // 🚀 CÁC HÀM API TRẢ VỀ JSON CHO REACT GỌI (ĐÃ CHUẨN HÓA ROUTE)
        // =========================================================================

        // Lấy tất cả sản phẩm
        [AllowAnonymous]
        [HttpGet("api/products")]
        public IActionResult GetProductsForReact()
        {
            var products = _context.Products.OrderByDescending(p => p.Id).ToList();
            return Json(products);
        }

        // Lấy chi tiết 1 sản phẩm theo ID
        [AllowAnonymous]
        [HttpGet("api/products/{id}")]
        public IActionResult GetProductDetailForReact(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return Json(product);
        }

        // Lọc sản phẩm theo ID Danh mục
        [AllowAnonymous]
        [HttpGet("api/products/category/{categoryId}")]
        public IActionResult GetProductsByCategory(int categoryId)
        {
            var products = _context.Products
                                   .Where(p => p.CategoryProductId == categoryId)
                                   .OrderByDescending(p => p.Id)
                                   .ToList();
            return Json(products);
        }

        // Lấy danh sách danh mục sản phẩm
        [AllowAnonymous]
        [HttpGet("api/categories")]
        public IActionResult GetCategoriesForReact()
        {
            var categories = _context.CategoryProducts.ToList();
            return Json(categories);
        }
    }
}