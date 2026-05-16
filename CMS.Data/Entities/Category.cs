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

namespace CMS.Data.Entities
{
    // Lớp Category dùng để lưu thông tin danh mục bài viết
    public class Category
    {
        // Khóa chính của danh mục
        public int Id { get; set; }

        // Tên danh mục
        public string Name { get; set; }

        // Mô tả chi tiết cho danh mục
        public string Description { get; set; }

        // Danh sách các bài viết thuộc danh mục
        public virtual ICollection<Post> Posts { get; set; }
    }
}