using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VKITActivityManager.Models
{
    [Table("HoatDongChuyenNganh")] // Bắt buộc có dòng này
    public class HoatDongChuyenNganh
    {
        [Key]
        public int Id { get; set; }
        public int ChuyenNganhId { get; set; }
        public string? TieuDe { get; set; }
        public string? TieuDePhu { get; set; }
        public string? NoiDung { get; set; }
        public string? DuongDanAnh { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [ForeignKey("ChuyenNganhId")]
        public ChuyenNganh? ChuyenNganh { get; set; }
    }
}