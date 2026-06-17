using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VKITActivityManager.Models;

namespace VKITActivityManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Tối ưu: Loại bỏ truy vấn Tin tức (1) và Video (2)
            var data = await _context.DanhSachHoatDong
                .Where(x => x.PhanLoai != 1 && x.PhanLoai != 2)
                .OrderByDescending(a => a.NgayTao).ToListAsync();

            // Lấy danh sách học phí và SẮP XẾP THEO NGÀNH để đảm bảo các dòng cùng ngành nằm cạnh nhau
            ViewBag.DanhSachHocPhi = await _context.DanhSachHocPhi
                                                   .OrderBy(x => x.NganhDaoTao)
                                                   .ToListAsync();

            ViewBag.DanhSachLoaiHocBong = _context.LoaiHocBongs.ToList();
            // ---> THÊM 2 DÒNG NÀY VÀO ĐỂ LẤY VIDEO <---
            ViewBag.VideoLon = await _context.Videos.Where(v => v.PhanLoaiVideoId == 1).OrderByDescending(v => v.NgayTao).FirstOrDefaultAsync();
            ViewBag.VideoNho = await _context.Videos.Where(v => v.PhanLoaiVideoId == 2).OrderByDescending(v => v.NgayTao).ToListAsync();
            // Lấy danh sách Ưu điểm ra mặt tiền
            ViewBag.DanhSachUuDiem = await _context.UuDiems.ToListAsync();
            // Thêm dòng này để lấy danh sách Hoạt động nổi bật mới nhất
            ViewBag.DanhSachHoatDongNoiBat = await _context.HoatDongNoiBats
                                                   .OrderByDescending(x => x.NgayTao)
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
        [HttpGet]
        public async Task<IActionResult> GetChatboxData()
        {
            var data = await _context.CauHoiThuongGaps
                .OrderBy(x => x.PhanLoai)
                .ThenBy(x => x.Id)
                .ToListAsync();
            return Json(data);
        }
    }
}