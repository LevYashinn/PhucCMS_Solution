using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using CMS.Data.Entities;
using System.Linq;

namespace CMS.Backend.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách khách hàng
        public IActionResult Index()
        {
            var customers = _context.Customers.OrderByDescending(c => c.Id).ToList();
            return View(customers);
        }

        // GET: Form Thêm mới
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Xử lý Thêm mới (ĐÃ SỬA: Gán mật khẩu mặc định để tránh lỗi NOT NULL trong SQL Server)
        [HttpPost]
        public IActionResult Create(Customer model)
        {
            if (model != null)
            {
                // Nếu mô hình thực thể yêu cầu Password, gán mật khẩu mặc định là 123456 để không bị lỗi trống dữ liệu
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

        // POST: Xử lý Cập nhật (ĐÃ SỬA: Giữ lại mật khẩu cũ trong DB)
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

            // Giữ nguyên mật khẩu cũ nếu model truyền lên bị trống, tránh ghi đè dữ liệu null
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