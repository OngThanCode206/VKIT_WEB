using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VKITActivityManager.Models
{
    public class AnhUuDiem
    {
        [Key]
        public int Id { get; set; }

        [StringLength(1000)]
        public string? MoTaNgan { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hoặc nhập đường dẫn ảnh")]
        public string DuongDanAnh { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Khóa ngoại liên kết với bảng UuDiem
        [Required(ErrorMessage = "Vui lòng chọn Ưu điểm thuộc về")]
        public int UuDiemId { get; set; }

        [ForeignKey("UuDiemId")]
        public UuDiem? UuDiem { get; set; }
    }
}