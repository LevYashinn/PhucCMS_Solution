using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Backend.Controllers // Lưu ý sửa lại namespace cho khớp với dự án của bạn
{
    // Bắt buộc phải có 2 dòng này để định nghĩa đây là API
    [Route("api/[controller]")]
    [ApiController]
    public class ProductApiController : ControllerBase // Kế thừa ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. API Lấy toàn bộ sản phẩm (React sẽ gọi cái này)
        // Đường dẫn gọi sẽ là: GET /api/ProductApi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products
                                         .Include(p => p.CategoryProduct)
                                         .OrderByDescending(p => p.Id)
                                         .ToListAsync();

            // Trả về định dạng JSON chuẩn (Mã 200 OK)
            return Ok(products);
        }

        // 2. API Xem chi tiết 1 sản phẩm (Phòng hờ sau này React cần dùng)
        // Đường dẫn gọi sẽ là: GET /api/ProductApi/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products
                                        .Include(p => p.CategoryProduct)
                                        .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm" }); // Trả về 404 JSON
            }

            return Ok(product);
        }
    }
}