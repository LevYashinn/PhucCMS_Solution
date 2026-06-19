using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/categoryproducts
        [HttpGet]
        public IActionResult GetAll()
        {
            var categories = _context.CategoryProducts
                .OrderBy(c => c.Name)
                .Select(c => new {
                    c.Id,
                    c.Name
                    // Nếu bạn có thêm trường mô tả hay slug thì thêm vào đây
                })
                .ToList();
            return Ok(categories);
        }

        // GET: api/categoryproducts/5
        [HttpGet("{id}")]
        public IActionResult GetDetail(int id)
        {
            var category = _context.CategoryProducts.FirstOrDefault(c => c.Id == id);
            if (category == null) return NotFound(new { message = "Không tìm thấy danh mục này" });

            return Ok(category);
        }
    }
}