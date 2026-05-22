using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using System;

namespace CMS.Backend.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Inject DbContext
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách người dùng từ SQL Server
        public IActionResult Index()
        {
            // Lấy dữ liệu thật từ bảng Users
            var users = _context.Users.ToList();

            return View(users);
        }

        // Chi tiết người dùng
        public IActionResult Details(int id)
        {
            // Tìm user theo Id
            var user = _context.Users.FirstOrDefault(x => x.Id == id);

            // Nếu không tồn tại
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
    }
}