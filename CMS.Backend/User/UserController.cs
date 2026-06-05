using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization; // THÊM MỚI: Thư viện phân quyền
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CMS.Backend.Controllers
{
    [Authorize(Roles = "Admin")] // THÊM MỚI: Khóa trang này lại, CHỈ TÀI KHOẢN ADMIN mới được thao tác
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        public IActionResult Details(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(User model)
        {
            if (ModelState.IsValid)
            {
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

            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(User model, string NewPassword)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == model.Id);
                if (existingUser == null) return NotFound();

                if (!string.IsNullOrEmpty(NewPassword))
                {
                    model.PasswordHash = NewPassword;
                }
                else
                {
                    model.PasswordHash = existingUser.PasswordHash;
                }

                _context.Users.Update(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }

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