using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VKITActivityManager.Models
{
    [Table("HoatDong")]
    public class HoatDong
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("PhanLoai")]
        public int PhanLoai { get; set; } // 1 = Tin tức, 2 = Video Banner

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề!")]
        [Column("TieuDe")]
        public string? TieuDe { get; set; }

        [Column("TieuDe2")]
        public string? TieuDe2 { get; set; }

        [Column("MoTaNgan")]
        public string? MoTaNgan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung chi tiết!")]
        [Column("NoiDung")]
        public string? NoiDung { get; set; }

        [Column("DuongDanAnh")]
        public string? DuongDanAnh { get; set; }

        [Column("DuongDanVideo")]
        public string? DuongDanVideo { get; set; }

        [Column("NgayTao")]
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public int? NganhId { get; set; }

    
    }
}