using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VKITActivityManager.Models
{
    [Table("DangKyTuVan")]
    public class DangKyTuVan
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string? HoTen { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn ngày sinh.")]
        public DateTime? NgaySinh { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập Email.")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành phố.")]
        public string? TinhThanh { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn Ngành quan tâm.")]
        public string? NganhQuanTam { get; set; }
        [RegularExpression(@"^(0)\d{9}$", ErrorMessage = "Số điện thoại cá nhân phải bắt đầu bằng số 0 và có đúng 10 chữ số.")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string? SoDienThoai { get; set; }
        [RegularExpression(@"^(0)\d{9}$", ErrorMessage = "Số điện thoại cá nhân phải bắt đầu bằng số 0 và có đúng 10 chữ số.")]
        public string? SoDienThoaiPhuHuynh { get; set; }

        public string? DiaChi { get; set; }

        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        public int TrangThai { get; set; } = 0;
    }
}