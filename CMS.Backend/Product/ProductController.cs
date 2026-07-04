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
using System.Threading.Tasks;

namespace CMS.Backend.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 🌟 1. HIỂN THỊ SẢN PHẨM (CÓ LỌC DANH MỤC & PHÂN TRANG)
        // ==========================================
        public async Task<IActionResult> Index(int? categoryId, int page = 1)
        {
            int pageSize = 5; // 🌟 Giới hạn hiển thị 5 sản phẩm trên 1 trang

            var query = _context.Products
                                .Include(p => p.CategoryProduct)
                                .AsQueryable();

            // Nếu người dùng chọn lọc theo danh mục
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryProductId == categoryId.Value);
                ViewBag.SelectedCategoryId = categoryId; // Giữ lại ID để nhúng vào link phân trang
            }

            // Tính toán tổng số sản phẩm và tổng số trang
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Cắt lấy đúng 5 sản phẩm của trang hiện tại
            var products = await query.OrderByDescending(p => p.Id)
                                      .Skip((page - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToListAsync();

            ViewBag.Categories = await _context.CategoryProducts.ToListAsync();

            // Đẩy dữ liệu trang xuống View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

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

        [AllowAnonymous]
        [HttpGet("api/products")]
        public IActionResult GetProductsForReact()
        {
            var products = _context.Products.OrderByDescending(p => p.Id).ToList();
            return Json(products);
        }

        [AllowAnonymous]
        [HttpGet("api/products/{id}")]
        public IActionResult GetProductDetailForReact(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return Json(product);
        }

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

        [AllowAnonymous]
        [HttpGet("api/categories")]
        public IActionResult GetCategoriesForReact()
        {
            var categories = _context.CategoryProducts.ToList();
            return Json(categories);
        }

        [AllowAnonymous]
        [HttpGet("api/products/search")]
        public IActionResult SearchProducts([FromQuery] string? keyword, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                var keywordLower = keyword.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(keywordLower));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            var results = query.OrderByDescending(p => p.Id).ToList();
            return Json(results);
        }

        [AllowAnonymous]
        [HttpGet("api/products/best-sellers")]
        public IActionResult GetBestSellers([FromQuery] int limit = 4)
        {
            var bestSellerIds = _context.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(limit)
                .Select(x => x.ProductId)
                .ToList();

            var products = _context.Products
                .Where(p => bestSellerIds.Contains(p.Id))
                .ToList();

            var sortedProducts = products
                .OrderBy(p => bestSellerIds.IndexOf(p.Id))
                .ToList();

            if (!sortedProducts.Any())
            {
                sortedProducts = _context.Products
                    .OrderByDescending(p => p.Id)
                    .Take(limit)
                    .ToList();
            }

            return Json(sortedProducts);
        }
    }
}