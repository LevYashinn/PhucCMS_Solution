using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities
{
    public class Banners
    {
        [Key]
        public int Id { get; set; }

        // Thêm dấu ? vào sau chữ string để tắt cảnh báo
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        public string? LinkUrl { get; set; }

        public int OrderDisplay { get; set; }
        public bool IsActive { get; set; }
    }
}