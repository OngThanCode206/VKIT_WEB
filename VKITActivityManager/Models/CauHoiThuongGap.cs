using System.ComponentModel.DataAnnotations;

namespace VKITActivityManager.Models
{
    public class CauHoiThuongGap
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập câu hỏi")]
        [StringLength(500)]
        public string CauHoi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung trả lời")]
        public string TraLoi { get; set; } // Bạn có thể lưu dạng HTML để định dạng in đậm, xuống dòng
        // THÊM CỘT NÀY: 0 = Câu hỏi chung, 1 đến 9 = Tương ứng với 9 ngành đào tạo
        public int PhanLoai { get; set; } = 0;
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}