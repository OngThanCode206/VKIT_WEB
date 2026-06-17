using System;
using System.ComponentModel.DataAnnotations;

namespace VKITActivityManager.Models
{
    public class HoatDongNoiBat
    {
        [Key]
        public int Id { get; set; } // Khóa chính, tự động tăng

        [Required(ErrorMessage = "Vui lòng nhập mô tả hoạt động")]
        [StringLength(1000)]
        public string MoTa { get; set; } // Mô tả / Tiêu đề hoạt động

        [Required(ErrorMessage = "Vui lòng thêm ảnh")]
        public string DuongDanAnh { get; set; } // Đường dẫn ảnh

        public DateTime NgayTao { get; set; } = DateTime.Now; // Ngày tạo tự động lấy giờ hiện tại
    }
}