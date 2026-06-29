using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

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

        // ==========================================
        // 1. API ĐẶT HÀNG & TẠO MÃ THANH TOÁN GIẢ LẬP
        // ==========================================
        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrder([FromBody] CheckoutRequest request)
        {
            if (request == null || request.CartItems == null || request.CartItems.Count == 0)
                return BadRequest(new { message = "Giỏ hàng trống!" });

            // 🌟 1. TẠO MÃ GIAO DỊCH GIẢ LẬP (VD: TXN-20231024-A1B2)
            string randomChars = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            string mockTransactionCode = $"TXN-{DateTime.Now:yyyyMMddHHmmss}-{randomChars}";

            // 🌟 2. THIẾT LẬP ĐƠN HÀNG (STATUS = 1: Đang xử lý)
            var order = new Order
            {
                CustomerId = request.CustomerId,
                OrderDate = DateTime.Now,
                // Lưu mã giao dịch và phương thức vào Ghi chú
                Notes = $"[Thanh toán: {request.PaymentMethod ?? "COD"}] [Mã GD: {mockTransactionCode}]",
                TotalAmount = request.TotalAmount,
                Status = 1, // 1 mặc định là Đang xử lý / Chờ xác nhận
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
                    UnitPrice = item.Price
                };
                _context.OrderDetails.Add(orderDetail);

                // ========================================================
                // 🌟 TỰ ĐỘNG TRỪ SỐ LƯỢNG TỒN KHO TRONG DATABASE 🌟
                // ========================================================
                var productInDb = _context.Products.Find(item.ProductId);
                if (productInDb != null)
                {
                    // Lấy số lượng kho hiện tại trừ đi số lượng khách vừa mua
                    productInDb.StockQuantity = productInDb.StockQuantity - item.Quantity;

                    // Cẩn thận: Nếu lỡ trừ mà ra số âm (bán quá tay), set nó về 0 luôn
                    if (productInDb.StockQuantity < 0)
                    {
                        productInDb.StockQuantity = 0;
                    }
                }
            }
            // Lưu lại những thay đổi (cả hóa đơn và số lượng tồn kho mới)
            _context.SaveChanges();

            // 🌟 3. GỬI EMAIL KÈM MÃ GIAO DỊCH
            var customer = _context.Customers.FirstOrDefault(c => c.Id == request.CustomerId);
            if (customer != null && !string.IsNullOrEmpty(customer.Email))
            {
                await SendOrderEmailAsync(customer.Email, customer.FullName, order, request.CartItems, mockTransactionCode, _context);
            }

            // 🌟 4. TRẢ VỀ JSON CHO POSTMAN/REACT KÈM MÃ THANH TOÁN
            return Ok(new
            {
                message = "Thanh toán thành công! Đơn hàng đang được xử lý.",
                orderId = order.Id,
                transactionCode = mockTransactionCode,
                orderStatus = "Đang xử lý"
            });
        }

        // ==========================================
        // 2. API LẤY LỊCH SỬ ĐƠN HÀNG 
        // ==========================================
        [HttpGet("history/{customerId}")]
        public IActionResult GetOrderHistory(int customerId)
        {
            var orders = _context.Orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.Id)
                .Select(o => new {
                    o.Id,
                    CreatedDate = o.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                    o.TotalAmount,
                    o.Status,
                    o.ShippingAddress,
                    o.ShippingPhone,
                    o.Notes,
                    Items = _context.OrderDetails
                        .Where(od => od.OrderId == o.Id)
                        .Select(od => new {
                            ProductName = od.Product.Name,
                            ProductImage = od.Product.ImageUrl,
                            Quantity = od.Quantity,
                            Price = od.UnitPrice
                        }).ToList()
                })
                .ToList();

            return Ok(orders);
        }

        [HttpGet("payment-methods")]
        public IActionResult GetPaymentMethods()
        {
            var methods = new List<object>
            {
                new { id = "COD", name = "Thanh toán khi nhận hàng (COD)", icon = "fa-solid fa-truck" },
                new { id = "VNPAY", name = "Thanh toán qua VNPAY", icon = "fa-solid fa-qrcode" },
                new { id = "MOMO", name = "Ví điện tử MoMo", icon = "fa-solid fa-wallet" },
                new { id = "BANK", name = "Chuyển khoản Ngân hàng", icon = "fa-solid fa-building-columns" }
            };
            return Ok(methods);
        }

        // ==========================================
        // 🌟 4. HÀM HỖ TRỢ GỬI EMAIL 
        // ==========================================
        private async Task SendOrderEmailAsync(string toEmail, string customerName, Order order, List<CartItemRequest> cartItems, string transactionCode, ApplicationDbContext context)
        {
            try
            {
                string fromEmail = "phuc512dz@gmail.com";
                string appPassword = "zbkwoezmagdmxmcg";

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, appPassword),
                    EnableSsl = true,
                };

                string itemsHtml = "";
                foreach (var item in cartItems)
                {
                    var product = context.Products.Find(item.ProductId);
                    string productName = product != null ? product.Name : "Sản phẩm giày";
                    itemsHtml += $@"
                        <tr>
                            <td style='padding: 10px; border: 1px solid #ddd;'>{productName}</td>
                            <td style='padding: 10px; border: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                            <td style='padding: 10px; border: 1px solid #ddd; text-align: right;'>{item.Price:N0} đ</td>
                        </tr>";
                }

                string body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #eee; border-radius: 10px; overflow: hidden;'>
                        <div style='background-color: #198754; padding: 20px; text-align: center; color: white;'>
                            <h2 style='margin: 0;'>THANH TOÁN THÀNH CÔNG</h2>
                        </div>
                        <div style='padding: 20px;'>
                            <p>Chào <strong>{customerName}</strong>,</p>
                            <p>Hệ thống đã nhận được thanh toán của bạn. Đơn hàng hiện đang được chuyển sang bộ phận kho để xử lý đóng gói.</p>
                            
                            <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin-bottom: 20px; border-left: 4px solid #198754;'>
                                <p style='margin: 5px 0; color: #198754; font-size: 16px;'><strong>Mã giao dịch:</strong> {transactionCode}</p>
                                <p style='margin: 5px 0; color: #dc3545;'><strong>Trạng thái:</strong> Đang xử lý</p>
                                <hr style='border: 0; border-top: 1px solid #eee; my-2;'/>
                                <p style='margin: 5px 0;'><strong>Mã đơn hàng:</strong> #{order.Id}</p>
                                <p style='margin: 5px 0;'><strong>Ngày đặt:</strong> {order.OrderDate.ToString("dd/MM/yyyy HH:mm")}</p>
                                <p style='margin: 5px 0;'><strong>Địa chỉ giao:</strong> {order.ShippingAddress}</p>
                                <p style='margin: 5px 0;'><strong>Số điện thoại:</strong> {order.ShippingPhone}</p>
                            </div>

                            <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                                <thead>
                                    <tr style='background-color: #f2f2f2;'>
                                        <th style='padding: 10px; border: 1px solid #ddd; text-align: left;'>Sản phẩm</th>
                                        <th style='padding: 10px; border: 1px solid #ddd; text-align: center;'>SL</th>
                                        <th style='padding: 10px; border: 1px solid #ddd; text-align: right;'>Đơn giá</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {itemsHtml}
                                    <tr>
                                        <td colspan='2' style='padding: 10px; border: 1px solid #ddd; text-align: right; font-weight: bold;'>ĐÃ THANH TOÁN:</td>
                                        <td style='padding: 10px; border: 1px solid #ddd; text-align: right; font-weight: bold; color: #198754; font-size: 16px;'>{order.TotalAmount:N0} đ</td>
                                    </tr>
                                </tbody>
                            </table>
                            <p>Cảm ơn bạn đã mua sắm tại <strong>Trạm Giày Sneaker</strong>!</p>
                        </div>
                    </div>
                ";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "Trạm Giày Sneaker"),
                    Subject = $"[Trạm Giày Sneaker] Thanh toán thành công mã {transactionCode}",
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi gửi email thanh toán: " + ex.Message);
            }
        }
    }

    public class CheckoutRequest
    {
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? PaymentMethod { get; set; }
        public List<CartItemRequest> CartItems { get; set; }
    }

    public class CartItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}