using System.ComponentModel.DataAnnotations;

namespace VKITActivityManager.Models
{
    public class HocPhi
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngành đào tạo")]
        [Display(Name = "Ngành đào tạo")]
        public string NganhDaoTao { get; set; } // Ví dụ: Cử nhân

        [Display(Name = "Thời gian đào tạo")]
        public string ThoiGian { get; set; } // Ví dụ: 3.5 năm, 14 HK

        [Required(ErrorMessage = "Vui lòng nhập đơn vị áp dụng")]
        [Display(Name = "Đơn vị áp dụng")]
        public string DonViApDung { get; set; } // Ví dụ: Khóa học, 1 Học kỳ

        [Display(Name = "Học phí gốc (VNĐ)")]
        public string MucHocPhi { get; set; }

        [Display(Name = "Học phí sau học bổng 25% (VNĐ)")]
        public string HocPhiGiam25 { get; set; }

        [Display(Name = "Học phí sau học bổng 50% (VNĐ)")]
        public string HocPhiGiam50 { get; set; }
        // THÊM DÒNG NÀY VÀO:
        public string HeDaoTao { get; set; }

        // Dùng để phân loại dòng chẵn/lẻ tạo màu nền xám nhạt (bg-light-red)
        public bool LaDongPhu { get; set; }
    }
}