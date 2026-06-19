using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors; // Quan trọng để dùng [EnableCors]
using System.Threading.Tasks;
using System.Linq; // Thêm thư viện này để dùng được hàm .Any()

namespace CMS.Controllers
{
    // Bật CORS cho toàn bộ Controller này
    [EnableCors("AllowReactApp")]
    public class CategoryProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- DÀNH CHO ADMIN (MVC Views) ---
        public async Task<IActionResult> Index()
        {
            var categories = await _context.CategoryProducts.Include(c => c.Products).ToListAsync();
            return View(categories);
        }

        // --- DÀNH CHO REACT (API Endpoint) ---
        // Đường dẫn: GET /api/CategoryProduct
        [HttpGet("api/CategoryProduct")]
        public async Task<IActionResult> GetCategoriesApi()
        {
            var categories = await _context.CategoryProducts.ToListAsync();
            return Ok(categories); // Trả về JSON cho React
        }

        // --- CÁC ACTION KHÁC GIỮ NGUYÊN ---
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.CategoryProducts.Include(c => c.Products).FirstOrDefaultAsync(m => m.Id == id);
            if (category == null) return NotFound();
            return View(category);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] CategoryProduct categoryProduct)
        {
            if (ModelState.IsValid)
            {
                _context.CategoryProducts.Add(categoryProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoryProduct);
        }

        // ==========================================
        // PHẦN ĐƯỢC THÊM MỚI: CHỨC NĂNG SỬA (EDIT)
        // ==========================================

        // GET: CategoryProduct/Edit/5 (Hàm này lấy dữ liệu cũ hiển thị lên Form)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryProduct = await _context.CategoryProducts.FindAsync(id);
            if (categoryProduct == null)
            {
                return NotFound();
            }
            return View(categoryProduct);
        }

        // POST: CategoryProduct/Edit/5 (Hàm này nhận dữ liệu mới từ Form và lưu vào SQL Server)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] CategoryProduct categoryProduct)
        {
            // Kiểm tra xem ID trên URL có khớp với ID của form gửi lên không
            if (id != categoryProduct.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoryProduct); // Cập nhật dữ liệu
                    await _context.SaveChangesAsync(); // Lưu thay đổi xuống Database
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryProductExists(categoryProduct.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index)); // Sửa xong thì quay về trang danh sách
            }
            return View(categoryProduct); // Nếu có lỗi nhập liệu thì hiển thị lại form
        }

        // Hàm hỗ trợ kiểm tra xem danh mục có tồn tại hay không
        private bool CategoryProductExists(int id)
        {
            return _context.CategoryProducts.Any(e => e.Id == id);
        }
    }
}