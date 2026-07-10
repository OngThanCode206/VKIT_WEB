using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq; // Thêm thư viện này để dùng LINQ GroupBy
using System.Text;
using VKITActivityManager.Models;

namespace VKITActivityManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.DanhSachHoatDong
                .Where(x => x.PhanLoai != 1 && x.PhanLoai != 2)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            ViewBag.DanhSachHocPhi = await _context.DanhSachHocPhi.ToListAsync();

            ViewBag.DanhSachHDCN = await _context.HoatDongChuyenNganhs
                .Include(x => x.ChuyenNganh)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            // ========================================================
            // CẬP NHẬT: Gom nhóm Sinh viên học bổng để hiển thị 1 dòng
            // ========================================================
            var rawSVHB = await _context.SinhVienHocBongs
                .Include(s => s.LoaiHocBong)
                .OrderByDescending(s => s.NgayNhan)
                .ToListAsync();

            ViewBag.DanhSachSVHB = rawSVHB
                .GroupBy(s => new { s.MaSV, s.TenSinhVien, s.Lop, s.HinhAnh })
                .Select(g => new SinhVienHocBong
                {
                    Id = g.First().Id, // Lấy ID đầu tiên để làm mốc Edit/Delete
                    MaSV = g.Key.MaSV,
                    TenSinhVien = g.Key.TenSinhVien,
                    Lop = g.Key.Lop,
                    HinhAnh = g.Key.HinhAnh,
                    NgayNhan = g.First().NgayNhan,
                    LoaiHocBong = new LoaiHocBong
                    {
                        // Nối tên các học bổng lại bằng dấu phẩy
                        TenHocBong = string.Join(",", g.Where(x => x.LoaiHocBong != null).Select(x => x.LoaiHocBong.TenHocBong))
                    }
                }).ToList();

            ViewBag.DanhSachLoaiHB = await _context.LoaiHocBongs.OrderByDescending(x => x.Id).ToListAsync();

            // LẤY DANH SÁCH ƯU ĐIỂM
            ViewBag.DanhSachUuDiem = await _context.UuDiems
                .Include(u => u.DanhSachHinhAnh)
                .OrderBy(u => u.Id)
                .ToListAsync();

            // LẤY DANH SÁCH VIDEO
            ViewBag.DanhSachVideo = await _context.Videos
                .Include(v => v.PhanLoaiVideo)
                .OrderByDescending(v => v.NgayTao)
                .ToListAsync();

            ViewBag.DanhSachCauHoi = await _context.CauHoiThuongGaps.OrderByDescending(x => x.Id).ToListAsync();
            ViewBag.DanhSachDangKy = _context.DangKyTuVans.OrderByDescending(x => x.NgayDangKy).ToList();
            return View(data);
        }

        [HttpGet]
        public IActionResult Create(int? type)
        {
            var model = new HoatDong { PhanLoai = type ?? 3 };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> Create(HoatDong hoatDong, IFormFile? fileAnh)
        {
            ModelState.Remove("fileAnh");

            if (hoatDong.PhanLoai == 3)
            {
                ModelState.Remove("TieuDe");
                ModelState.Remove("NoiDung");
                hoatDong.TieuDe = "Banner Slider " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                hoatDong.NoiDung = "Nội dung hình ảnh Banner";
            }

            if (ModelState.IsValid)
            {
                if (fileAnh != null && fileAnh.Length > 0)
                {
                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "hoatdong");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileAnh.FileName);
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create)) { await fileAnh.CopyToAsync(fileStream); }
                    hoatDong.DuongDanAnh = "/images/hoatdong/" + uniqueFileName;
                }

                hoatDong.NgayTao = DateTime.Now;
                _context.DanhSachHoatDong.Add(hoatDong);
                await _context.SaveChangesAsync();

                TempData["SuccessMsg"] = "Thêm mới nội dung thành công!";
                TempData["ActiveTab"] = hoatDong.PhanLoai.ToString();
                return RedirectToAction(nameof(Index));
            }
            return View(hoatDong);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var hoatDong = await _context.DanhSachHoatDong.FindAsync(id);
            return hoatDong == null ? NotFound() : View(hoatDong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> Edit(int id, HoatDong hoatDong, IFormFile? fileAnh)
        {
            if (id != hoatDong.Id) return NotFound();
            ModelState.Remove("fileAnh");

            if (hoatDong.PhanLoai == 3)
            {
                ModelState.Remove("TieuDe");
                ModelState.Remove("NoiDung");
                hoatDong.TieuDe = "Banner Slider " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                hoatDong.NoiDung = "Nội dung hình ảnh Banner";
            }

            if (ModelState.IsValid)
            {
                var hoatDongDb = await _context.DanhSachHoatDong.FindAsync(id);
                if (hoatDongDb == null) return NotFound();

                hoatDongDb.TieuDe = hoatDong.TieuDe;
                // Cập nhật thêm các trường khác nếu có

                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Cập nhật dữ liệu hoàn tất!";
                TempData["ActiveTab"] = hoatDongDb.PhanLoai.ToString();
                return RedirectToAction(nameof(Index));
            }
            return View(hoatDong);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var hoatDong = await _context.DanhSachHoatDong.FindAsync(id);
            if (hoatDong != null)
            {
                int phanLoaiTam = hoatDong.PhanLoai;
                _context.DanhSachHoatDong.Remove(hoatDong);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Đã xóa nội dung thành công!";
                TempData["ActiveTab"] = phanLoaiTam.ToString();
            }
            return RedirectToAction(nameof(Index));
        }

        // ======================= QUẢN LÝ HỌC PHÍ =======================
        [HttpGet]
        public IActionResult CreateHocPhi() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHocPhi([Bind("Id,NganhDaoTao,HeDaoTao,DonViApDung,MucHocPhi,HocPhiGiam25,HocPhiGiam50,ThoiGian,LaDongPhu")] HocPhi hocPhi)
        {
            if (ModelState.IsValid)
            {
                _context.DanhSachHocPhi.Add(hocPhi);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Thêm mức học phí mới thành công!";
                TempData["ActiveTab"] = "hocphi";
                return RedirectToAction(nameof(Index));
            }
            return View(hocPhi);
        }

        [HttpGet]
        public async Task<IActionResult> EditHocPhi(int? id)
        {
            if (id == null) return NotFound();
            var hocPhi = await _context.DanhSachHocPhi.FindAsync(id);
            if (hocPhi == null) return NotFound();
            return View(hocPhi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHocPhi(int id, [Bind("Id,NganhDaoTao,HeDaoTao,DonViApDung,MucHocPhi,HocPhiGiam25,HocPhiGiam50,ThoiGian,LaDongPhu")] HocPhi hocPhi)
        {
            if (id != hocPhi.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try { _context.Update(hocPhi); await _context.SaveChangesAsync(); }
                catch (DbUpdateConcurrencyException) { if (!_context.DanhSachHocPhi.Any(e => e.Id == id)) return NotFound(); else throw; }
                TempData["SuccessMsg"] = "Cập nhật thông tin học phí thành công!";
                TempData["ActiveTab"] = "hocphi";
                return RedirectToAction(nameof(Index));
            }
            return View(hocPhi);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHocPhi(int id)
        {
            _context.DanhSachHocPhi.Remove(await _context.DanhSachHocPhi.FindAsync(id)); await _context.SaveChangesAsync();
            TempData["ActiveTab"] = "hocphi"; return RedirectToAction(nameof(Index));
        }

        // ======================= QUẢN LÝ HOẠT ĐỘNG CHUYÊN NGÀNH =======================
        public async Task<IActionResult> IndexHDCN(int? nganhId)
        {
            var danhSachNganh = await _context.ChuyenNganhs.ToListAsync();
            ViewBag.NganhList = new SelectList(danhSachNganh, "Id", "TenNganh", nganhId);
            ViewBag.SelectedNganhId = nganhId;
            var query = _context.HoatDongChuyenNganhs.Include(h => h.ChuyenNganh).AsQueryable();
            if (nganhId.HasValue) query = query.Where(h => h.ChuyenNganhId == nganhId.Value);
            var danhSachBaiViet = await query.OrderByDescending(h => h.NgayTao).ToListAsync();
            return View(danhSachBaiViet);
        }

        public IActionResult CreateHDCN(int? nganhId)
        {
            ViewBag.ChuyenNganhId = new SelectList(_context.ChuyenNganhs, "Id", "TenNganh", nganhId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> CreateHDCN(HoatDongChuyenNganh hoatDong, IFormFile? hinhAnh)
        {
            ModelState.Remove("ChuyenNganh");
            if (ModelState.IsValid)
            {
                if (hinhAnh != null && hinhAnh.Length > 0)
                {
                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "hoatdong");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnh.FileName);
                    string filePath = Path.Combine(uploadFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create)) { await hinhAnh.CopyToAsync(stream); }
                    hoatDong.DuongDanAnh = "/images/hoatdong/" + fileName;
                }
                hoatDong.NgayTao = DateTime.Now;
                _context.HoatDongChuyenNganhs.Add(hoatDong);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Thêm thành công";
                TempData["ActiveTab"] = "hdcn";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ChuyenNganhId = new SelectList(_context.ChuyenNganhs, "Id", "TenNganh", hoatDong.ChuyenNganhId);
            return View(hoatDong);
        }

        public async Task<IActionResult> EditHDCN(int? id)
        {
            if (id == null) return NotFound();
            var hoatDong = await _context.HoatDongChuyenNganhs.FindAsync(id);
            if (hoatDong == null) return NotFound();
            ViewBag.ChuyenNganhId = new SelectList(_context.ChuyenNganhs, "Id", "TenNganh", hoatDong.ChuyenNganhId);
            return View(hoatDong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> EditHDCN(int id, HoatDongChuyenNganh hoatDong, IFormFile? hinhAnh)
        {
            if (id != hoatDong.Id) return NotFound();
            try
            {
                if (ModelState.IsValid)
                {
                    var data = await _context.HoatDongChuyenNganhs.FindAsync(id);
                    if (data == null) return NotFound();

                    data.TieuDe = hoatDong.TieuDe;
                    data.TieuDePhu = hoatDong.TieuDePhu;
                    data.NoiDung = hoatDong.NoiDung;
                    data.ChuyenNganhId = hoatDong.ChuyenNganhId;

                    if (hinhAnh != null && hinhAnh.Length > 0)
                    {
                        string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "hoatdong");
                        if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnh.FileName);
                        string filePath = Path.Combine(uploadFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create)) { await hinhAnh.CopyToAsync(stream); }
                        data.DuongDanAnh = "/images/hoatdong/" + fileName;
                    }
                    await _context.SaveChangesAsync();
                    TempData["SuccessMsg"] = "Cập nhật thành công!";
                    TempData["ActiveTab"] = "hdcn";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }
            ViewBag.ChuyenNganhId = new SelectList(_context.ChuyenNganhs, "Id", "TenNganh", hoatDong.ChuyenNganhId);
            return View(hoatDong);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHDCN(int id)
        {
            var hoatDong = await _context.HoatDongChuyenNganhs.FindAsync(id);
            if (hoatDong != null)
            {
                _context.HoatDongChuyenNganhs.Remove(hoatDong);
                await _context.SaveChangesAsync();
            }
            TempData["ActiveTab"] = "hdcn";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // QUẢN LÝ SV HỌC BỔNG (ĐÃ SỬA LỖI SQL)
        // =========================================================================
        public IActionResult CreateSVHB()
        {
            ViewBag.LoaiHocBongId = new SelectList(_context.LoaiHocBongs, "Id", "TenHocBong");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> CreateSVHB(SinhVienHocBong sv, List<int> LoaiHocBongIds, IFormFile? fileAnh)
        {
            ModelState.Remove("LoaiHocBong");
            ModelState.Remove("LoaiHocBongId"); // Bỏ qua Validate cột cũ vì ta xử lý List riêng

            if (ModelState.IsValid)
            {
                if (LoaiHocBongIds == null || !LoaiHocBongIds.Any())
                {
                    ModelState.AddModelError("", "Vui lòng chọn ít nhất 1 loại học bổng.");
                    ViewBag.LoaiHocBongId = new SelectList(_context.LoaiHocBongs, "Id", "TenHocBong");
                    return View(sv);
                }

                string imageUrl = "/images/default-avatar.png";

                if (fileAnh != null && fileAnh.Length > 0)
                {
                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "sinhvienhocbong");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileAnh.FileName);
                    string filePath = Path.Combine(uploadFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create)) { await fileAnh.CopyToAsync(stream); }
                    imageUrl = "/images/sinhvienhocbong/" + fileName;
                }

                // Chạy vòng lặp tạo ra nhiều bản ghi tương ứng với số học bổng được chọn
                foreach (var id in LoaiHocBongIds)
                {
                    var newSv = new SinhVienHocBong
                    {
                        MaSV = sv.MaSV,
                        TenSinhVien = sv.TenSinhVien,
                        Lop = sv.Lop,
                        HinhAnh = imageUrl,
                        NgayNhan = DateTime.Now,
                        LoaiHocBongId = id // Gán ID chuẩn để SQL không báo lỗi FK
                    };
                    _context.SinhVienHocBongs.Add(newSv);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Thêm sinh viên thành công!";
                TempData["ActiveTab"] = "svhocbong";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.LoaiHocBongId = new SelectList(_context.LoaiHocBongs, "Id", "TenHocBong");
            return View(sv);
        }

        [HttpGet]
        public async Task<IActionResult> EditSVHB(int? id)
        {
            if (id == null) return NotFound();
            var sv = await _context.SinhVienHocBongs.FindAsync(id);
            if (sv == null) return NotFound();

            // Tìm tất cả học bổng sinh viên này đang sở hữu để gán vào giao diện Edit
            var selectedIds = await _context.SinhVienHocBongs
                .Where(x => x.MaSV == sv.MaSV)
                .Select(x => x.LoaiHocBongId)
                .ToListAsync();

            ViewBag.LoaiHocBongId = new MultiSelectList(_context.LoaiHocBongs, "Id", "TenHocBong", selectedIds);
            return View(sv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> EditSVHB(int id, SinhVienHocBong sv, List<int> LoaiHocBongIds, IFormFile? fileAnh)
        {
            ModelState.Remove("LoaiHocBong");
            ModelState.Remove("LoaiHocBongId");
            ModelState.Remove("HinhAnh");

            if (ModelState.IsValid)
            {
                if (LoaiHocBongIds == null || !LoaiHocBongIds.Any())
                {
                    ModelState.AddModelError("", "Vui lòng chọn ít nhất 1 loại học bổng.");
                    ViewBag.LoaiHocBongId = new MultiSelectList(_context.LoaiHocBongs, "Id", "TenHocBong", LoaiHocBongIds);
                    return View(sv);
                }

                // 1. Tìm sinh viên cũ để giữ lại hình ảnh và ngày nhận
                var dataCu = await _context.SinhVienHocBongs.FindAsync(id);
                if (dataCu == null) return NotFound();

                string maSVCu = dataCu.MaSV;
                string imageUrl = dataCu.HinhAnh;

                // 2. Xử lý ảnh mới
                if (fileAnh != null && fileAnh.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "sinhvienhocbong");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileAnh.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create)) { await fileAnh.CopyToAsync(fileStream); }
                    imageUrl = "/images/sinhvienhocbong/" + uniqueFileName;
                }

                // 3. Xóa các bản ghi cũ của sinh viên này
                var allOldRecords = _context.SinhVienHocBongs.Where(x => x.MaSV == maSVCu);
                _context.SinhVienHocBongs.RemoveRange(allOldRecords);

                // 4. Thêm lại các bản ghi mới
                foreach (var hbId in LoaiHocBongIds)
                {
                    var newSv = new SinhVienHocBong
                    {
                        MaSV = sv.MaSV,
                        TenSinhVien = sv.TenSinhVien,
                        Lop = sv.Lop,
                        HinhAnh = imageUrl,
                        NgayNhan = dataCu.NgayNhan,
                        LoaiHocBongId = hbId
                    };
                    _context.SinhVienHocBongs.Add(newSv);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Cập nhật sinh viên thành công!";
                TempData["ActiveTab"] = "svhocbong";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.LoaiHocBongId = new MultiSelectList(_context.LoaiHocBongs, "Id", "TenHocBong", LoaiHocBongIds);
            return View(sv);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSVHB(int id)
        {
            var sv = await _context.SinhVienHocBongs.FindAsync(id);
            if (sv != null)
            {
                // Quét sạch tất cả dòng có cùng Mã Sinh Viên
                var allRecords = _context.SinhVienHocBongs.Where(x => x.MaSV == sv.MaSV);
                _context.SinhVienHocBongs.RemoveRange(allRecords);
                await _context.SaveChangesAsync();
            }
            TempData["ActiveTab"] = "svhocbong";
            return RedirectToAction(nameof(Index));
        }

        // ======================= QUẢN LÝ LOẠI HỌC BỔNG =======================
        [HttpGet]
        public IActionResult CreateLoaiHB() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLoaiHB([Bind("Id,TenHocBong,MoTa,SoSuat,MauNen")] LoaiHocBong loaiHB)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loaiHB);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Thêm Loại Học Bổng mới thành công!";
                TempData["ActiveTab"] = "loaihocbong";
                return RedirectToAction(nameof(Index));
            }
            return View(loaiHB);
        }

        [HttpGet]
        public async Task<IActionResult> EditLoaiHB(int? id)
        {
            if (id == null) return NotFound();
            var loaiHB = await _context.LoaiHocBongs.FindAsync(id);
            if (loaiHB == null) return NotFound();
            return View(loaiHB);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLoaiHB(int id, [Bind("Id,TenHocBong,MoTa,SoSuat,MauNen")] LoaiHocBong loaiHB)
        {
            if (id != loaiHB.Id) return NotFound();
            ModelState.Remove("SinhVienHocBongs");
            if (ModelState.IsValid)
            {
                _context.Update(loaiHB);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Cập nhật Loại Học Bổng thành công!";
                TempData["ActiveTab"] = "loaihocbong";
                return RedirectToAction(nameof(Index));
            }
            return View(loaiHB);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLoaiHB(int id)
        {
            var loaiHB = await _context.LoaiHocBongs.FindAsync(id);
            if (loaiHB != null) { _context.LoaiHocBongs.Remove(loaiHB); await _context.SaveChangesAsync(); }
            TempData["ActiveTab"] = "loaihocbong";
            return RedirectToAction(nameof(Index));
        }

        // ======================= QUẢN LÝ VIDEO =======================
        [HttpGet]
        public IActionResult CreateVideo()
        {
            ViewBag.PhanLoaiVideoId = new SelectList(_context.PhanLoaiVideos, "Id", "TenPhanLoai");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVideo(Video video)
        {
            if (ModelState.IsValid)
            {
                _context.Videos.Add(video);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Thêm Video mới thành công!";
                TempData["ActiveTab"] = "video";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.PhanLoaiVideoId = new SelectList(_context.PhanLoaiVideos, "Id", "TenPhanLoai", video.PhanLoaiVideoId);
            return View(video);
        }

        [HttpGet]
        public async Task<IActionResult> EditVideo(int? id)
        {
            if (id == null) return NotFound();
            var video = await _context.Videos.FindAsync(id);
            if (video == null) return NotFound();
            ViewBag.PhanLoaiVideoId = new SelectList(_context.PhanLoaiVideos, "Id", "TenPhanLoai", video.PhanLoaiVideoId);
            return View(video);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVideo(int id, Video video)
        {
            if (id != video.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(video);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Cập nhật Video thành công!";
                TempData["ActiveTab"] = "video";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.PhanLoaiVideoId = new SelectList(_context.PhanLoaiVideos, "Id", "TenPhanLoai", video.PhanLoaiVideoId);
            return View(video);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video != null) { _context.Videos.Remove(video); await _context.SaveChangesAsync(); }
            TempData["ActiveTab"] = "video";
            return RedirectToAction(nameof(Index));
        }

        // ======================= QUẢN LÝ ƯU ĐIỂM CHƯƠNG TRÌNH & ẢNH =======================
        [HttpGet]
        public IActionResult CreateUuDiem() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUuDiem([Bind("TenUuDiem,NoiDung,MauNen,Icon")] UuDiem uuDiem)
        {
            if (ModelState.IsValid)
            {
                _context.UuDiems.Add(uuDiem);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Thêm Ưu Điểm mới thành công!";
                TempData["ActiveTab"] = "uudiem";
                return RedirectToAction(nameof(Index));
            }
            return View(uuDiem);
        }

        [HttpGet]
        public async Task<IActionResult> EditUuDiem(int? id)
        {
            if (id == null) return NotFound();
            var uuDiem = await _context.UuDiems.FindAsync(id);
            if (uuDiem == null) return NotFound();
            return View(uuDiem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUuDiem(int id, [Bind("Id,TenUuDiem,NoiDung,MauNen,Icon")] UuDiem uuDiem)
        {
            if (id != uuDiem.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(uuDiem);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Cập nhật Ưu Điểm thành công!";
                TempData["ActiveTab"] = "uudiem";
                return RedirectToAction(nameof(Index));
            }
            return View(uuDiem);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUuDiem(int id)
        {
            var uuDiem = await _context.UuDiems.FindAsync(id);
            if (uuDiem != null) { _context.UuDiems.Remove(uuDiem); await _context.SaveChangesAsync(); }
            TempData["ActiveTab"] = "uudiem";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult CreateAnhUuDiem(int uuDiemId)
        {
            ViewBag.UuDiemId = uuDiemId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> CreateAnhUuDiem(AnhUuDiem anh, IFormFile? fileAnh)
        {
            ModelState.Remove("DuongDanAnh");
            ModelState.Remove("UuDiem");

            if (ModelState.IsValid)
            {
                if (fileAnh != null && fileAnh.Length > 0)
                {
                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "uudiem");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileAnh.FileName);
                    string filePath = Path.Combine(uploadFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create)) { await fileAnh.CopyToAsync(stream); }
                    anh.DuongDanAnh = "/images/uudiem/" + fileName;
                    anh.NgayTao = DateTime.Now;

                    _context.AnhUuDiems.Add(anh);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMsg"] = "Thêm ảnh thành công!";
                }
                TempData["ActiveTab"] = "uudiem";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.UuDiemId = anh.UuDiemId;
            return View(anh);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAnhUuDiem(int id)
        {
            var anh = await _context.AnhUuDiems.FindAsync(id);
            if (anh != null) { _context.AnhUuDiems.Remove(anh); await _context.SaveChangesAsync(); }
            TempData["ActiveTab"] = "uudiem";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // QUẢN LÝ Q&A CHATBOX
        // =========================================================================
        [HttpGet]
        public IActionResult CreateCauHoi() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCauHoi(CauHoiThuongGap model)
        {
            ModelState.Clear();
            model.NgayTao = DateTime.Now;

            if (string.IsNullOrEmpty(model.CauHoi)) ModelState.AddModelError("CauHoi", "Không được để trống câu hỏi.");
            if (string.IsNullOrEmpty(model.TraLoi)) ModelState.AddModelError("TraLoi", "Không được để trống nội dung trả lời.");

            if (ModelState.IsValid)
            {
                _context.CauHoiThuongGaps.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Thêm câu hỏi thành công!";
                TempData["ActiveTab"] = "chatbox";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCauHoi(int? id)
        {
            if (id == null) return NotFound();
            var cauHoi = await _context.CauHoiThuongGaps.FindAsync(id);
            return cauHoi == null ? NotFound() : View(cauHoi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCauHoi(int id, CauHoiThuongGap model)
        {
            if (id != model.Id) return NotFound();

            ModelState.Clear();

            if (string.IsNullOrEmpty(model.CauHoi)) ModelState.AddModelError("CauHoi", "Không được để trống câu hỏi.");
            if (string.IsNullOrEmpty(model.TraLoi)) ModelState.AddModelError("TraLoi", "Không được để trống nội dung trả lời.");

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Cập nhật câu hỏi thành công!";
                TempData["ActiveTab"] = "chatbox";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCauHoi(int id)
        {
            var cauHoi = await _context.CauHoiThuongGaps.FindAsync(id);
            if (cauHoi != null) { _context.CauHoiThuongGaps.Remove(cauHoi); await _context.SaveChangesAsync(); }
            TempData["ActiveTab"] = "chatbox";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================================
        // QUẢN LÝ HOẠT ĐỘNG NỔI BẬT
        // ==========================================================
        public async Task<IActionResult> IndexHDNB()
        {
            var danhSach = await _context.HoatDongNoiBats
                                         .OrderByDescending(x => x.NgayTao)
                                         .ToListAsync();
            return View(danhSach);
        }

        public IActionResult CreateHDNB()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> CreateHDNB(HoatDongNoiBat model, IFormFile fileTaiLen)
        {
            ModelState.Remove("DuongDanAnh");
            ModelState.Remove("NgayTao");
            if (ModelState.IsValid)
            {
                if (fileTaiLen != null && fileTaiLen.Length > 0)
                {
                    var thuMucUpload = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "hoatdong");
                    if (!Directory.Exists(thuMucUpload)) Directory.CreateDirectory(thuMucUpload);

                    var tenFile = Guid.NewGuid().ToString() + Path.GetExtension(fileTaiLen.FileName);
                    var duongDanLuu = Path.Combine(thuMucUpload, tenFile);

                    using (var stream = new FileStream(duongDanLuu, FileMode.Create))
                    {
                        await fileTaiLen.CopyToAsync(stream);
                    }
                    model.DuongDanAnh = "/images/hoatdong/" + tenFile;
                }
                else
                {
                    model.DuongDanAnh = "/images/hoatdong/default.jpg";
                }

                model.NgayTao = DateTime.Now;
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexHDNB));
            }
            return View(model);
        }

        public async Task<IActionResult> EditHDNB(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.HoatDongNoiBats.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> EditHDNB(int id, HoatDongNoiBat model, IFormFile fileTaiLen)
        {
            ModelState.Remove("DuongDanAnh");
            ModelState.Remove("NgayTao");
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var itemCu = await _context.HoatDongNoiBats.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

                    if (fileTaiLen != null && fileTaiLen.Length > 0)
                    {
                        var thuMucUpload = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "hoatdong");
                        var tenFile = Guid.NewGuid().ToString() + Path.GetExtension(fileTaiLen.FileName);
                        var duongDanLuu = Path.Combine(thuMucUpload, tenFile);

                        using (var stream = new FileStream(duongDanLuu, FileMode.Create))
                        {
                            await fileTaiLen.CopyToAsync(stream);
                        }
                        model.DuongDanAnh = "/images/hoatdong/" + tenFile;
                    }
                    else
                    {
                        model.DuongDanAnh = itemCu.DuongDanAnh;
                    }

                    model.NgayTao = itemCu.NgayTao;
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.HoatDongNoiBats.Any(e => e.Id == model.Id)) return NotFound();
                    else throw;
                }
                TempData["SuccessMsg"] = "Cập nhật hoạt động thành công!";
                return RedirectToAction(nameof(IndexHDNB));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHDNB(int id)
        {
            var item = await _context.HoatDongNoiBats.FindAsync(id);
            if (item != null)
            {
                _context.HoatDongNoiBats.Remove(item);
                await _context.SaveChangesAsync();
            }
            TempData["SuccessMsg"] = "Đã xóa hoạt động thành công!";
            return RedirectToAction(nameof(IndexHDNB));
        }


        public IActionResult ExportToExcel()
        {
            // Lấy danh sách đăng ký
            var list = _context.DangKyTuVans.OrderByDescending(x => x.NgayDangKy).ToList();
            var builder = new StringBuilder();

            // Dòng tiêu đề cột
            builder.AppendLine("ID,Họ tên,Ngày sinh,Email,Tỉnh thành,Ngành,SĐT,SĐT Phụ huynh,Địa chỉ,Ngày đăng ký");

            foreach (var item in list)
            {
                // 1. Format Ngày tháng gọn gàng
                string ngaySinh = item.NgaySinh?.ToString("dd/MM/yyyy") ?? "";
                string ngayDK = item.NgayDangKy.ToString("dd/MM/yyyy HH:mm");

                // 2. FIX LỖI SỐ ĐIỆN THOẠI: Ép Excel hiểu đây là văn bản (Text) bằng cách bọc trong ="..."
                string sdt = $"=\"{item.SoDienThoai}\"";
                string sdtPhuHuynh = string.IsNullOrEmpty(item.SoDienThoaiPhuHuynh) ? "" : $"=\"{item.SoDienThoaiPhuHuynh}\"";

                // 3. FIX LỖI DẤU PHẨY TRONG DỮ LIỆU: Bọc các chuỗi văn bản trong dấu nháy kép "..."
                // Đề phòng trường hợp người dùng nhập địa chỉ có dấu phẩy (Ví dụ: Quận 1, TP.HCM) làm vỡ cấu trúc CSV
                string hoTen = $"\"{item.HoTen}\"";
                string email = $"\"{item.Email}\"";
                string tinhThanh = $"\"{item.TinhThanh}\"";
                string nganh = $"\"{item.NganhQuanTam}\"";
                string diaChi = $"\"{item.DiaChi ?? ""}\"";

                // Gắn vào dòng
                builder.AppendLine($"{item.Id},{hoTen},{ngaySinh},{email},{tinhThanh},{nganh},{sdt},{sdtPhuHuynh},{diaChi},{ngayDK}");
            }

            // 4. FIX LỖI FONT TIẾNG VIỆT: Thêm cờ BOM (Byte Order Mark) chuẩn của UTF-8 vào đầu file
            byte[] buffer = Encoding.UTF8.GetBytes(builder.ToString());
            byte[] preamble = Encoding.UTF8.GetPreamble(); // Lấy mã BOM của UTF-8

            // Gộp mã BOM và nội dung file lại với nhau
            byte[] finalBuffer = new byte[preamble.Length + buffer.Length];
            Buffer.BlockCopy(preamble, 0, finalBuffer, 0, preamble.Length);
            Buffer.BlockCopy(buffer, 0, finalBuffer, preamble.Length, buffer.Length);

            // Trả về file đã được fix lỗi hoàn toàn
            return File(finalBuffer, "text/csv", "DanhSachDangKy.csv");
        }
        // =========================================================================
        // QUẢN LÝ ĐĂNG KÝ TƯ VẤN (SỬA VÀ XÓA)
        // =========================================================================

        [HttpGet]
        public async Task<IActionResult> EditDangKy(int? id)
        {
            if (id == null) return NotFound();

            var dangKy = await _context.DangKyTuVans.FindAsync(id);
            if (dangKy == null) return NotFound();

            return View(dangKy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDangKy(int id, DangKyTuVan model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMsg"] = "Cập nhật thông tin đăng ký thành công!";
                    TempData["ActiveTab"] = "dktv"; // Mở lại đúng tab Đăng ký TV
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.DangKyTuVans.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDangKy(int id)
        {
            var dangKy = await _context.DangKyTuVans.FindAsync(id);
            if (dangKy != null)
            {
                _context.DangKyTuVans.Remove(dangKy);
                await _context.SaveChangesAsync();

                TempData["SuccessMsg"] = "Đã xóa bản ghi đăng ký tư vấn!";
            }

            TempData["ActiveTab"] = "dktv"; // Mở lại đúng tab Đăng ký TV
            return RedirectToAction(nameof(Index));
        }
    }
}