/*
 * Sinh viên: Trần Trọng Phúc
 * Mã sinh viên: 2123110142
 * Lớp: CCQ2311D
 * Ngày tạo: 16/05/2026
 * Mô tả: Thực hiện quản lý danh mục
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // 🌟 TRẢ LẠI TÊN GỐC LÀ UnitPrice
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}