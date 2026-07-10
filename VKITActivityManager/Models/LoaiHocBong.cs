using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VKITActivityManager.Models
{
    [Table("LoaiHocBong")]
    public class LoaiHocBong
    {
        [Key] public int Id { get; set; }
        public string? TenHocBong { get; set; }
        public string? MoTa { get; set; }
        public int SoSuat { get; set; }
        public string? MauNen { get; set; }
        public ICollection<SinhVienHocBong>? SinhVienHocBongs { get; set; }
    }
}