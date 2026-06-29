/*
 * Sinh viên: Trần Trọng Phúc
 * Mã sinh viên: 2123110142
 * Lớp: CCQ2311D
 * Ngày tạo: 16/05/2026
 * Mô tả: Thực hiện quản lý danh mục
 */
using System;
using System.Collections.Generic;

namespace CMS.Data.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        // 🌟 TRẢ LẠI TÊN GỐC ĐỂ ADMIN KHÔNG BỊ LỖI
        public DateTime OrderDate { get; set; }
        public string? Notes { get; set; }

        // 🌟 CÁC CỘT BỔ SUNG THÊM CHO VIỆC MUA HÀNG
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingPhone { get; set; }

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}