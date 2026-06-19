using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using System.Linq;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/posts
        [HttpGet]
        public IActionResult GetAll()
        {
            var posts = _context.Posts
               .OrderByDescending(p => p.Id)
               .Select(p => new {
                   p.Id,
                   p.Title,
                   p.Content,     // <--- ĐÃ THÊM DÒNG NÀY ĐỂ REACT CÓ NỘI DUNG
                   p.ImageUrl,
                   p.CreatedDate,
                   CategoryName = p.Category.Name
               })
               .ToList();
            return Ok(posts);
        }

        // GET: api/posts/category/5
        [HttpGet("category/{categoryId}")]
        public IActionResult GetByCategory(int categoryId)
        {
            var posts = _context.Posts
                .Where(p => p.CategoryId == categoryId)
                .Select(p => new {
                    p.Id,
                    p.Title,
                    p.Content,    // <--- ĐÃ THÊM DÒNG NÀY
                    p.ImageUrl,
                    p.CreatedDate
                })
                .ToList();
            return Ok(posts);
        }

        // GET: api/posts/5
        [HttpGet("{id}")]
        public IActionResult GetDetail(int id)
        {
            // Hàm này bạn trả về nguyên object 'post' nên nó đã tự động có đủ Content rồi, không cần sửa
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);

            if (post == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết này trong hệ thống" });
            }

            return Ok(post);
        }
    }
}