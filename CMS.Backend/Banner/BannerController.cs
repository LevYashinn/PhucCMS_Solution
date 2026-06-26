using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace CMS.Backend.Controllers
{
    public class BannerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BannerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var banners = _context.Banners.OrderByDescending(b => b.Id).ToList();
            return View(banners);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Banners model, IFormFile uploadImage)
        {
            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.Length > 0)
                {
                    string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadImage.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create)) uploadImage.CopyTo(stream);
                    model.ImageUrl = "/uploads/" + fileName;
                }
                _context.Banners.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var banner = _context.Banners.Find(id);
            if (banner == null) return NotFound();
            return View(banner);
        }

        [HttpPost]
        public IActionResult Edit(Banners model, IFormFile uploadImage)
        {
            if (ModelState.IsValid)
            {
                var bannerInDb = _context.Banners.FirstOrDefault(b => b.Id == model.Id);
                if (bannerInDb == null) return NotFound();

                bannerInDb.Title = model.Title;
                bannerInDb.LinkUrl = model.LinkUrl;
                bannerInDb.OrderDisplay = model.OrderDisplay;
                bannerInDb.IsActive = model.IsActive;

                if (uploadImage != null && uploadImage.Length > 0)
                {
                    string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadImage.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create)) uploadImage.CopyTo(stream);

                    if (!string.IsNullOrEmpty(bannerInDb.ImageUrl))
                    {
                        string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", bannerInDb.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                    }
                    bannerInDb.ImageUrl = "/uploads/" + fileName;
                }
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public IActionResult Delete(int id)
        {
            var banner = _context.Banners.Find(id);
            if (banner != null)
            {
                if (!string.IsNullOrEmpty(banner.ImageUrl))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", banner.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                }
                _context.Banners.Remove(banner);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        [HttpGet("api/banners")]
        public IActionResult GetBannersForReact()
        {
            var banners = _context.Banners.Where(b => b.IsActive).OrderBy(b => b.OrderDisplay).ToList();
            return Json(banners);
        }
    }
}