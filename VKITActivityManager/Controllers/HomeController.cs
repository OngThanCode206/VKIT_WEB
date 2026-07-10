using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VKITActivityManager.Models;
using Microsoft.Extensions.Configuration; // Dùng để đọc appsettings.json
using System.Net.Http;                    // Gọi API Google
using System.Text;                        // Xử lý chuỗi
using System.Text.Json;

namespace VKITActivityManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration; // 1. Khai báo IConfiguration

        // 2. Tiêm IConfiguration vào Constructor
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.DanhSachHoatDong
                .Where(x => x.PhanLoai != 1 && x.PhanLoai != 2)
                .OrderByDescending(a => a.NgayTao).ToListAsync();

            ViewBag.DanhSachHocPhi = await _context.DanhSachHocPhi
                                                   .OrderBy(x => x.NganhDaoTao)
                                                   .ToListAsync();

            ViewBag.DanhSachLoaiHocBong = _context.LoaiHocBongs.ToList();

            ViewBag.VideoLon = await _context.Videos.Where(v => v.PhanLoaiVideoId == 1).OrderByDescending(v => v.NgayTao).FirstOrDefaultAsync();
            ViewBag.VideoNho = await _context.Videos.Where(v => v.PhanLoaiVideoId == 2).OrderByDescending(v => v.NgayTao).ToListAsync();

            ViewBag.DanhSachUuDiem = await _context.UuDiems.ToListAsync();

            ViewBag.DanhSachHoatDongNoiBat = await _context.HoatDongNoiBats
                                                   .OrderByDescending(x => x.NgayTao)
                                                   .ToListAsync();

            // ---> THÊM DÒNG NÀY VÀO ĐỂ LẤY DANH SÁCH SINH VIÊN VINH DANH <---
            ViewBag.DanhSachSinhVienVinhDanh = await _context.SinhVienHocBongs
                                                   .Include(sv => sv.LoaiHocBong) // Để lấy được Tên Học Bổng
                                                   .OrderByDescending(sv => sv.NgayNhan)
                                                   .ToListAsync();

            return View(data);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var hoatDong = await _context.DanhSachHoatDong.FirstOrDefaultAsync(m => m.Id == id);
            if (hoatDong == null) return NotFound();
            return View(hoatDong);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // 1. KHI NGƯỜI DÙNG CLICK VÀO NGÀNH (Hiển thị Ảnh 1)
        public IActionResult DanhSachHoatDongNganh(int id)
        {
            var nganh = _context.ChuyenNganhs.Find(id);
            if (nganh == null) return RedirectToAction("Index");

            // Chỉ lấy bài viết thuộc về đúng ID ngành được click
            var danhSach = _context.HoatDongChuyenNganhs
                .Where(x => x.ChuyenNganhId == id)
                .OrderByDescending(x => x.NgayTao)
                .ToList();

            ViewBag.TenNganh = nganh.TenNganh;
            return View(danhSach);
        }

        // 2. KHI NGƯỜI DÙNG CLICK VÀO 1 THẺ BÀI VIẾT (Hiển thị Ảnh 2)
        public IActionResult ChiTietHoatDongNganh(int id)
        {
            // Lấy bài viết dựa vào ID của chính bài viết đó
            var baiViet = _context.HoatDongChuyenNganhs.Find(id);
            if (baiViet == null) return RedirectToAction("Index");

            return View(baiViet);
        }

        public IActionResult DanhSachSinhVienHocBong(int id)
        {
            var loaiHB = _context.LoaiHocBongs.Find(id);
            if (loaiHB == null) return RedirectToAction("Index");

            var danhSachSV = _context.SinhVienHocBongs
                .Where(x => x.LoaiHocBongId == id)
                .OrderByDescending(x => x.NgayNhan)
                .ToList();

            ViewBag.LoaiHocBong = loaiHB;
            return View(danhSachSV);
        }

        // 3. KHI NGƯỜI DÙNG CLICK VÀO THẺ ƯU ĐIỂM Ở TRANG CHỦ
        public async Task<IActionResult> ChiTietUuDiem(int id)
        {
            // Lấy Ưu điểm cùng với toàn bộ Hình ảnh của nó
            var uuDiem = await _context.UuDiems
                .Include(u => u.DanhSachHinhAnh)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (uuDiem == null) return RedirectToAction("Index");

            return View(uuDiem);
        }

        [HttpGet]
        public async Task<IActionResult> GetChatboxData()
        {
            var data = await _context.CauHoiThuongGaps
                .OrderBy(x => x.PhanLoai)
                .ThenBy(x => x.Id)
                .ToListAsync();
            return Json(data);
        }

        // =========================================================================
        // 3. HÀM XỬ LÝ CHATBOT AI VỚI GEMINI
        // =========================================================================
        [HttpPost]
        public async Task<IActionResult> AskAI(string userQuestion)
        {
            if (string.IsNullOrEmpty(userQuestion)) return Json(new { success = false, answer = "Vui lòng nhập câu hỏi." });

            try
            {
                // 1. Kiểm tra API Key
                string? apiKey = _configuration["GeminiConfig:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    return Json(new { success = false, answer = "Lỗi: Chưa đọc được API Key từ appsettings.json." });
                }

                // 2. CẤU HÌNH MODEL TẠI ĐÂY (Sử dụng dòng 2.0 mới nhất thay cho bản 1.5 đã đóng)
                string modelName = "gemini-3.1-flash-lite";
                string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

                // 3. Lấy dữ liệu làm kiến thức cho AI
                var danhSachKienThuc = await _context.CauHoiThuongGaps.ToListAsync();
                string kienThucHocThuat = "";
                foreach (var item in danhSachKienThuc)
                {
                    kienThucHocThuat += $"- Câu hỏi: {item.CauHoi} \n- Trả lời: {item.TraLoi}\n\n";
                }

                string prompt = $@"Bạn là trợ lý ảo tư vấn tuyển sinh VKIT. Hãy trả lời câu hỏi dựa trên dữ liệu sau: {kienThucHocThuat}. Câu hỏi của sinh viên: ""{userQuestion}""";

                using (var client = new HttpClient())
                {
                    var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
                    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                        string botReply = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "";

                        // Xóa các dấu ** do Markdown của AI tạo ra để giao diện hiển thị đẹp hơn
                        botReply = botReply.Replace("**", "");

                        return Json(new { success = true, answer = botReply });
                    }
                    else
                    {
                        // Nếu vẫn lỗi, in ra chi tiết để kiểm tra lại tên model
                        var errorDetails = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, answer = $"Google báo lỗi {response.StatusCode}: {errorDetails}" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, answer = "Lỗi hệ thống: " + ex.Message });
            }
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult SubmitDangKy(DangKyTuVan model, [FromServices] EmailService emailService)
        {
            // 1. Kiểm tra không được bỏ trống các trường (dựa vào cấu hình [Required] ở Model)
            if (!ModelState.IsValid)
            {
                // Lấy thông báo lỗi đầu tiên để trả về cho người dùng
                var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return Json(new { success = false, message = firstError ?? "Vui lòng nhập đầy đủ thông tin hợp lệ." });
            }

            try
            {
                // 2. Ràng buộc: Kiểm tra trùng Số Điện Thoại
                bool isPhoneExist = _context.DangKyTuVans.Any(x => x.SoDienThoai == model.SoDienThoai);
                if (isPhoneExist)
                {
                    return Json(new { success = false, message = "Số điện thoại này đã được đăng ký. Vui lòng sử dụng số khác!" });
                }

                // 3. Ràng buộc: Kiểm tra trùng Email
                bool isEmailExist = _context.DangKyTuVans.Any(x => x.Email == model.Email);
                if (isEmailExist)
                {
                    return Json(new { success = false, message = "Email này đã được đăng ký. Vui lòng sử dụng email khác!" });
                }

                // 4. Nếu qua hết các bài kiểm tra -> Lưu vào Database
                _context.DangKyTuVans.Add(model);
                _context.SaveChanges();

                // 5. Gửi mail thông báo
                try
                {
                    string body = $"<h3>Có đăng ký tư vấn mới!</h3>" +
                                  $"<p>Họ tên: {model.HoTen}</p>" +
                                  $"<p>SĐT: {model.SoDienThoai}</p>" +
                                  $"<p>Email: {model.Email}</p>" +
                                  $"<p>Tỉnh/Thành phố: {model.TinhThanh}</p>" + // <-- ĐÃ THÊM THÔNG TIN TỈNH THÀNH Ở ĐÂY
                                  $"<p>Ngành: {model.NganhQuanTam}</p>";

                    emailService.SendEmail("Thông báo đăng ký tư vấn mới", body);
                }
                catch (Exception mailEx)
                {
                    return Json(new { success = true, message = "Đăng ký thành công! (Chú ý: Mail nội bộ chưa được gửi do lỗi SMTP)" });
                }

                return Json(new { success = true, message = "Đăng ký thành công! Chuyên gia của VKIT sẽ sớm liên hệ với bạn." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi lưu cơ sở dữ liệu: " + ex.Message });
            }
        }
    }
}