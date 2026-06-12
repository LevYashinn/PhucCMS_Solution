using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using CMS.Data.Entities;
using System.Linq;

// Nếu bạn bỏ file này vào thư mục User, hãy đổi thành: namespace CMS.Backend.User
namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        // 1. Lấy danh sách toàn bộ nhân viên quản trị
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _context.Users
                .OrderByDescending(u => u.Id)
                .Select(u => new {
                    u.Id,
                    u.FullName,
                    u.Username, // Sửa Email thành Username
                    u.Role      // Thêm Role cho khớp DB
                    // LƯU Ý: Tuyệt đối không Select PasswordHash
                })
                .ToList();

            return Ok(users);
        }

        // GET: api/users/5
        // 2. Lấy thông tin chi tiết của 1 nhân viên
        [HttpGet("{id}")]
        public IActionResult GetDetail(int id)
        {
            var user = _context.Users
                .Where(u => u.Id == id)
                .Select(u => new {
                    u.Id,
                    u.FullName,
                    u.Username, // Sửa Email thành Username
                    u.Role      // Thêm Role
                })
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy nhân viên này trong hệ thống." });
            }

            return Ok(user);
        }

        // POST: api/users
        // 3. Thêm một nhân viên mới (Cấp tài khoản cho nhân viên)
        [HttpPost]
        public IActionResult Create([FromBody] User user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Kiểm tra xem Username đã có người dùng chưa
            var isExist = _context.Users.Any(u => u.Username == user.Username);
            if (isExist)
            {
                return BadRequest(new { message = "Tài khoản (Username) này đã tồn tại, vui lòng chọn tên khác!" });
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "Thêm nhân viên thành công!", userId = user.Id });
        }

        // DELETE: api/users/5
        // 4. Xóa quyền / Xóa tài khoản nhân viên
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy nhân viên!" });
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(new { message = "Đã xóa tài khoản nhân viên thành công!" });
        }
    }
}