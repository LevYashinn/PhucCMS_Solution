/*
 * Sinh viên: Trần Trọng Phúc
 * Mã sinh viên: 2123110142
 * Lớp: CCQ2311D
 * Ngày tạo: 16/05/2026
 * Mô tả: Thực hiện quản lý danh mục
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Data.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public int CustomerId { get; set; }
        public int Status { get; set; }
        public string? Notes { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }

    }
}
