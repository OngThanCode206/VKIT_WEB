using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VKITActivityManager.Models
{
    [Table("ChuyenNganh")] // Bắt buộc có dòng này để tránh lỗi s
    public class ChuyenNganh
    {
        [Key]
        public int Id { get; set; }
        public string TenNganh { get; set; }
        public ICollection<HoatDongChuyenNganh> HoatDongChuyenNganhs { get; set; }
    }
}