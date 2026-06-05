using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CMS.Backend.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Hiển thị danh sách người dùng
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        // GET: Chi tiết người dùng
        public IActionResult Details(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // GET: Form Thêm người dùng
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Xử lý Thêm người dùng
        [HttpPost]
        public IActionResult Create(User model)
        {
            // THAY ĐỔI: Bọc điều kiện kiểm tra dữ liệu đầu vào hợp lệ
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên đăng nhập đã tồn tại chưa
                var checkExist = _context.Users.Any(u => u.Username == model.Username);
                if (checkExist)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã có người dùng!");
                    return View(model);
                }

                _context.Users.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // Nếu dữ liệu lỗi, trả lại View kèm theo thông báo validation lỗi
            return View(model);
        }

        // GET: Form Sửa người dùng
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Xử lý Sửa người dùng
        [HttpPost]
        public IActionResult Edit(User model, string NewPassword)
        {
            // THAY ĐỔI: Bọc kiểm tra dữ liệu đầu vào hợp lệ trước khi sửa
            if (ModelState.IsValid)
            {
                // Tìm User gốc trong Database để lấy lại mật khẩu cũ nếu cần
                var existingUser = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == model.Id);
                if (existingUser == null) return NotFound();

                // Xử lý mật khẩu: Nếu nhập mới thì lấy cái mới, nếu trống thì lấy cái cũ
                if (!string.IsNullOrEmpty(NewPassword))
                {
                    model.PasswordHash = NewPassword; // Sau này sẽ thực hiện hash mật khẩu tại đây
                }
                else
                {
                    model.PasswordHash = existingUser.PasswordHash;
                }

                _context.Users.Update(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // Nếu dữ liệu lỗi, trả lại View cùng các thông báo lỗi
            return View(model);
        }

        // POST/GET: Xóa người dùng
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}