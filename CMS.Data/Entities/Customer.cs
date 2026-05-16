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
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CMS.Data.Entities
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email {  get; set; }
        public string? Phone {  get; set; }
        public string? Address {  get; set; }

        [Required]
        public string Password { get; set; }
        public virtual ICollection<Order>? Orders { get; set; }
    }
}
