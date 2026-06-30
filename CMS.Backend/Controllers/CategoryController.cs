using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Backend.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. GIAO DIỆN QUẢN TRỊ (ADMIN)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var data = await _context.Categories.OrderByDescending(c => c.Id).ToListAsync();
            return View(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 🌟 ĐÃ SỬA: Dùng [Bind] để chặn lỗi kiểm duyệt ngầm của C#
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Category model)
        {
            // Xóa bỏ xác thực khóa ngoại để tránh lỗi ngầm
            ModelState.Remove("Posts");

            if (ModelState.IsValid)
            {
                _context.Categories.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // 🌟 ĐÃ SỬA: Dùng [Bind] để chặn lỗi kiểm duyệt ngầm của C#
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category model)
        {
            if (id != model.Id) return NotFound();

            ModelState.Remove("Posts");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.Id == model.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // 🚀 2. API CUNG CẤP DỮ LIỆU CHO BÊN REACT (FRONTEND)
        // =========================================================================
        [AllowAnonymous]
        [HttpGet("api/post-categories")]
        public async Task<IActionResult> GetPostCategoriesForReact()
        {
            var categories = await _context.Categories.OrderByDescending(c => c.Id).ToListAsync();
            return Json(categories);
        }
    }
}