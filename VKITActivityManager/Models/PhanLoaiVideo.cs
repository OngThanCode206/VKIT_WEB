using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace VKITActivityManager.Models
{
    public class PhanLoaiVideo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên phân loại video")]
        [StringLength(100)]
        public string? TenPhanLoai { get; set; }
        // VD: 1 = "Video giới thiệu lớn", 2 = "Video danh sách ngang"

        // Mối quan hệ 1-Nhiều: 1 Loại video sẽ có nhiều Video
        public ICollection<Video>? Videos { get; set; }
    }
}