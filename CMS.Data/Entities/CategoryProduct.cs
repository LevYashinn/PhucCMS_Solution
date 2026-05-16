/*
 * Sinh viên: Trần Trọng Phúc
 * Mã sinh viên: 2123110142
 * Lớp: CCQ2311D
 * Ngày tạo: 16/05/2026
 * Mô tả: Thực hiện quản lý danh mục
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities
{
    public class CategoryProduct
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Ten danh mục không được để trống" )]
        [StringLength(100)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public virtual ICollection<Product>? Products { get; set; }

    }
}
