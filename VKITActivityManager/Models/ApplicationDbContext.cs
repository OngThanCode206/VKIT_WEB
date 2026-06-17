using Microsoft.EntityFrameworkCore;

using VKITActivityManager.Models;

namespace VKITActivityManager.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<HoatDong> DanhSachHoatDong { get; set; }
        public DbSet<HocPhi> DanhSachHocPhi { get; set; }
        // THÊM 2 BẢNG MỚI NÀY VÀO DƯỚI CÙNG
        public DbSet<PhanLoaiVideo> PhanLoaiVideos { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<ChuyenNganh> ChuyenNganhs { get; set; }
        public DbSet<HoatDongChuyenNganh> HoatDongChuyenNganhs { get; set; }
        public DbSet<LoaiHocBong> LoaiHocBongs { get; set; }
        public DbSet<SinhVienHocBong> SinhVienHocBongs { get; set; }
        // BẢNG QUẢN LÝ ƯU ĐIỂM CHƯƠNG TRÌNH
        public DbSet<UuDiem> UuDiems { get; set; }
        public DbSet<AnhUuDiem> AnhUuDiems { get; set; }
        // BẢNG QUẢN LÝ CHATBOX FAQ
        public DbSet<CauHoiThuongGap> CauHoiThuongGaps { get; set; }
        // Ghi đè hàm này để tự động tạo 2 loại Video mặc định cho Admin chọn
        public DbSet<HoatDongNoiBat> HoatDongNoiBats { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Sinh dữ liệu mẫu (Seed Data)
            modelBuilder.Entity<PhanLoaiVideo>().HasData(
                new PhanLoaiVideo { Id = 1, TenPhanLoai = "Video Giới thiệu (Toàn màn hình)" },
                new PhanLoaiVideo { Id = 2, TenPhanLoai = "Video Danh sách (Ngang ở dưới)" }
            );
            modelBuilder.Entity<CauHoiThuongGap>().HasData(
                // =========================================================================
                // NHÓM 1: CÂU HỎI TUYỂN SINH CHUNG (PhanLoai = 0)
                // Các câu này có từ khóa chính để User gõ tìm kiếm: "học phí", "xét tuyển", "học bổng"
                // =========================================================================
                new CauHoiThuongGap
                {
                    Id = 1,
                    CauHoi = "Học phí của Viện Công nghệ Việt - Hàn như thế nào?",
                    TraLoi = "<p>Học phí tại VKIT được tính theo tín chỉ, dao động khoảng <strong style='color: #c62828;'>15.000.000đ - 18.000.000đ/học kỳ</strong> tùy thuộc vào hệ đào tạo Kỹ sư hay Cử nhân.</p><p>Đặc biệt, sinh viên có cơ hội nhận các gói học bổng doanh nghiệp giảm từ 25% đến 50% học phí nếu đạt thành tích tốt.</p>",
                    PhanLoai = 0,
                    NgayTao = DateTime.Now
                },
                new CauHoiThuongGap
                {
                    Id = 2,
                    CauHoi = "Các phương thức xét tuyển năm 2026 là gì?",
                    TraLoi = "<p>Năm 2026, Viện áp dụng <strong>04 phương thức xét tuyển chính</strong> công bằng:</p><ol><li>Xét tuyển kết quả thi tốt nghiệp THPT 2026.</li><li>Xét tuyển học bạ THPT theo tổ hợp 3 môn (Điểm TB lớp 12 hoặc 3 học kỳ).</li><li>Xét điểm thi Đánh giá năng lực của ĐHQG TP.HCM 2026.</li><li>Xét điểm thi Đánh giá đầu vào đại học V-SAT 2026.</li></ol>",
                    PhanLoai = 0,
                    NgayTao = DateTime.Now
                },

                // =========================================================================
                // NHÓM 2: GIỚI THIỆU CHI TIẾT ĐỊNH DẠNG WORD CHO 9 NGÀNH ĐÀO TẠO (PhanLoai = 1 -> 9)
                // =========================================================================
                new CauHoiThuongGap
                {
                    Id = 3,
                    CauHoi = "Giới thiệu ngành Công nghệ thông tin",
                    TraLoi = "<h5 style='color: #0d47a1; font-weight: bold;'>NGÀNH CÔNG NGHỆ THÔNG TIN (Chuẩn Hàn Quốc)</h5><p>Đào tạo chuyên sâu về <strong>Kỹ nghệ phần mềm, Trí tuệ nhân tạo (AI) và An toàn thông tin</strong>.</p><ul><li>Thực hành 100% tại phòng Lab hiện đại.</li><li>Thực tập thực tế tại doanh nghiệp công nghệ lớn từ năm thứ 3.</li><li>Cơ hội chuyển tiếp du học 2+2 nhận song bằng đại học quốc tế.</li></ul>",
                    PhanLoai = 1, // Khớp với Ngành 1 trong Chatbox
                    NgayTao = DateTime.Now
                },
                new CauHoiThuongGap
                {
                    Id = 4,
                    CauHoi = "Giới thiệu ngành Công nghệ kỹ thuật ô tô",
                    TraLoi = "<h5 style='color: #0d47a1; font-weight: bold;'>NGÀNH CÔNG NGHỆ KỸ THUẬT Ô TÔ</h5><p>Trang bị kiến thức về thiết kế hệ thống, chế tạo, kiểm định và bảo dưỡng ô tô thông minh, xe điện.</p><p>Sinh viên được thực hành trực tiếp tại các xưởng cơ khí động lực liên kết doanh nghiệp quy mô lớn.</p>",
                    PhanLoai = 2, // Khớp với Ngành 2 trong Chatbox
                    NgayTao = DateTime.Now
                },
                new CauHoiThuongGap
                {
                    Id = 5,
                    CauHoi = "Giới thiệu ngành Quản trị kinh doanh",
                    TraLoi = "<h5 style='color: #0d47a1; font-weight: bold;'>NGÀNH QUẢN TRỊ KINH DOANH</h5><p>Đào tạo kiến thức khởi nghiệp, quản trị chiến lược, marketing số toàn cầu. Chương trình học tích hợp phương pháp làm việc chuẩn tư duy doanh nghiệp đa quốc gia.</p>",
                    PhanLoai = 3, // Khớp với Ngành 3 trong Chatbox
                    NgayTao = DateTime.Now
                }
                // Các ngành từ 4 đến 9 bạn có thể viết tương tự hoặc bổ sung sau ở trang Admin...
            );
        }
    }
}