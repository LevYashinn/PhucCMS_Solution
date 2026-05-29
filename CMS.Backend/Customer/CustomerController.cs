using Microsoft.AspNetCore.Mvc;
using CMS.Data;
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

        // Action hiển thị danh sách khách hàng
        public IActionResult Index()
        {
            var Customers = _context.Customers.ToList();
            return View(Customers);
        }
    }
}