using System.ComponentModel.DataAnnotations;

namespace VKITActivityManager.Models
{
    public class UuDiem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên ưu điểm")]
        [StringLength(200)]
        public string TenUuDiem { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung/mô tả")]
        public string NoiDung { get; set; }

        // Màu nền hoặc màu viền của thẻ
        public string? MauNen { get; set; }

        // Mã icon (Ví dụ: fas fa-users, fas fa-plane-departure...)
        public string? Icon { get; set; }

        // Mối quan hệ 1-Nhiều: 1 Ưu điểm có nhiều hình ảnh bên trong
        public ICollection<AnhUuDiem>? DanhSachHinhAnh { get; set; }
    }
}