using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VKITActivityManager.Models
{
    [Table("SinhVienHocBong")]
    public class SinhVienHocBong
    {
        [Key] public int Id { get; set; }
        public int LoaiHocBongId { get; set; }
        public string? MaSV { get; set; }
        public string? TenSinhVien { get; set; }
        public string? Lop { get; set; }
        public string? HinhAnh { get; set; }
        public DateTime NgayNhan { get; set; } = DateTime.Now;

        [ForeignKey("LoaiHocBongId")]
        public LoaiHocBong LoaiHocBong { get; set; }
    }
}