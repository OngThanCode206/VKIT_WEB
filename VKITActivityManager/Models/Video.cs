using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VKITActivityManager.Models
{
    public class Video
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả ngắn hoặc tiêu đề video")]
        [StringLength(250)]
        public string? MoTaNgan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập link Youtube")]
        public string? DuongDanVideo { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Khóa ngoại trỏ về bảng Phân loại Video
        [Required(ErrorMessage = "Vui lòng chọn vị trí hiển thị (Phân loại)")]
        public int PhanLoaiVideoId { get; set; }

        [ForeignKey("PhanLoaiVideoId")]
        public PhanLoaiVideo? PhanLoaiVideo { get; set; }
    }
}