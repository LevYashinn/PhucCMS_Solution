using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using CMS.Data.Entities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace CMS.Backend.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 🌟 1. DANH SÁCH KHÁCH HÀNG (ĐÃ THÊM PHÂN TRANG)
        // ==========================================
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 5; // Hiện 5 khách hàng trên 1 trang

            var query = _context.Customers.AsQueryable();

            // Tính tổng số mục và tổng số trang
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Cắt lấy dữ liệu trang hiện tại
            var customers = await query.OrderByDescending(c => c.Id)
                                       .Skip((page - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToListAsync();

            // Đẩy dữ liệu phân trang xuống View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(customers);
        }

        // GET: Form Thêm mới
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Xử lý Thêm mới (Gán mật khẩu mặc định để tránh lỗi NOT NULL)
        [HttpPost]
        public IActionResult Create(Customer model)
        {
            if (model != null)
            {
                if (string.IsNullOrEmpty(model.Password))
                {
                    model.Password = "123456";
                }

                _context.Customers.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: Form Sửa
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: Xử lý Cập nhật (Giữ lại mật khẩu cũ trong DB)
        [HttpPost]
        public IActionResult Edit(Customer model)
        {
            var dbCustomer = _context.Customers.Find(model.Id);
            if (dbCustomer == null) return NotFound();

            // Cập nhật các thông tin thông thường
            dbCustomer.FullName = model.FullName;
            dbCustomer.Phone = model.Phone;
            dbCustomer.Email = model.Email;
            dbCustomer.Address = model.Address;

            // Giữ nguyên mật khẩu cũ nếu model truyền lên bị trống
            if (!string.IsNullOrEmpty(model.Password))
            {
                dbCustomer.Password = model.Password;
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // Xóa khách hàng
        public IActionResult Delete(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}