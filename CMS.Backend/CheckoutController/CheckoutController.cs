using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Backend.Controllers
{
    [AllowAnonymous]
    [Route("api/checkout")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("place-order")]
        public IActionResult PlaceOrder([FromBody] CheckoutRequest request)
        {
            if (request == null || request.CartItems == null || request.CartItems.Count == 0)
                return BadRequest(new { message = "Giỏ hàng trống!" });

            var order = new Order
            {
                CustomerId = request.CustomerId,
                OrderDate = DateTime.Now,        // Đã sửa thành OrderDate
                Notes = "Đơn hàng từ Website",   // Đã bổ sung Notes
                TotalAmount = request.TotalAmount,
                Status = 1,
                ShippingAddress = request.Address ?? "",
                ShippingPhone = request.Phone ?? ""
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in request.CartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price      // Đã sửa thành UnitPrice
                };
                _context.OrderDetails.Add(orderDetail);
            }
            _context.SaveChanges();

            return Ok(new { message = "Đặt hàng thành công!", orderId = order.Id });
        }

        [HttpGet("history/{customerId}")]
        public IActionResult GetOrderHistory(int customerId)
        {
            var orders = _context.Orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.Id)
                .Select(o => new {
                    o.Id,
                    CreatedDate = o.OrderDate.ToString("dd/MM/yyyy HH:mm"), // Ép tên về CreatedDate cho React dễ đọc
                    o.TotalAmount,
                    o.Status,
                    o.ShippingAddress,
                    o.ShippingPhone,
                    Items = _context.OrderDetails
                        .Where(od => od.OrderId == o.Id)
                        .Select(od => new {
                            ProductName = od.Product.Name,
                            ProductImage = od.Product.ImageUrl,
                            Quantity = od.Quantity,
                            Price = od.UnitPrice // Ép tên về Price cho React dễ đọc
                        }).ToList()
                })
                .ToList();

            return Ok(orders);
        }
    }

    public class CheckoutRequest
    {
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public List<CartItemRequest> CartItems { get; set; }
    }

    public class CartItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}