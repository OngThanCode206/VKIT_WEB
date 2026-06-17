using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using VKITActivityManager.Models;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Runtime.InteropServices;

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

            ViewBag.DanhSachSVHB = await _context.SinhVienHocBongs
                .Include(s => s.LoaiHocBong)
                .OrderByDescending(s => s.NgayNhan)
                .ToListAsync();

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
                //... (Giữ nguyên các bản cập nhật nếu có)

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

        // ======================= QUẢN LÝ SV HỌC BỔNG =======================
        public IActionResult CreateSVHB()
        {
            ViewBag.LoaiHocBongId = new SelectList(_context.LoaiHocBongs, "Id", "TenHocBong");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> CreateSVHB(SinhVienHocBong sv, IFormFile? fileAnh)
        {
            ModelState.Remove("LoaiHocBong");
            if (ModelState.IsValid)
            {
                if (fileAnh != null && fileAnh.Length > 0)
                {
                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "sinhvienhocbong");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileAnh.FileName);
                    string filePath = Path.Combine(uploadFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create)) { await fileAnh.CopyToAsync(stream); }
                    sv.HinhAnh = "/images/sinhvienhocbong/" + fileName;
                }
                sv.NgayNhan = DateTime.Now;
                _context.SinhVienHocBongs.Add(sv);
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Thêm sinh viên thành công!";
                TempData["ActiveTab"] = "svhocbong";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.LoaiHocBongId = new SelectList(_context.LoaiHocBongs, "Id", "TenHocBong", sv.LoaiHocBongId);
            return View(sv);
        }

        [HttpGet]
        public async Task<IActionResult> EditSVHB(int? id)
        {
            if (id == null) return NotFound();
            var sv = await _context.SinhVienHocBongs.FindAsync(id);
            if (sv == null) return NotFound();
            ViewBag.LoaiHocBongId = new SelectList(_context.LoaiHocBongs, "Id", "TenHocBong", sv.LoaiHocBongId);
            return View(sv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1073741824)]
        public async Task<IActionResult> EditSVHB(int id, SinhVienHocBong sv, IFormFile? fileAnh)
        {
            if (id != sv.Id) return NotFound();
            ModelState.Remove("LoaiHocBong");
            ModelState.Remove("HinhAnh");

            if (ModelState.IsValid)
            {
                var data = await _context.SinhVienHocBongs.FindAsync(id);
                if (data == null) return NotFound();
                data.MaSV = sv.MaSV;
                data.TenSinhVien = sv.TenSinhVien;
                data.Lop = sv.Lop;
                data.LoaiHocBongId = sv.LoaiHocBongId;

                if (fileAnh != null && fileAnh.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "sinhvienhocbong");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileAnh.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create)) { await fileAnh.CopyToAsync(fileStream); }
                    data.HinhAnh = "/images/sinhvienhocbong/" + uniqueFileName;
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMsg"] = "Cập nhật sinh viên thành công!";
                TempData["ActiveTab"] = "svhocbong";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.LoaiHocBongId = new SelectList(_context.LoaiHocBongs, "Id", "TenHocBong", sv.LoaiHocBongId);
            return View(sv);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSVHB(int id)
        {
            var sv = await _context.SinhVienHocBongs.FindAsync(id);
            if (sv != null) { _context.SinhVienHocBongs.Remove(sv); await _context.SaveChangesAsync(); }
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
        public async Task<IActionResult> CreateCauHoi(CauHoiThuongGap model) // Đã đổi tên biến thành "model"
        {
            // 1. Xóa sạch mọi bắt lỗi ngầm mặc định của hệ thống
            ModelState.Clear();

            // 2. Tự gán các giá trị bắt buộc
            model.NgayTao = DateTime.Now;

            // 3. Tự kiểm tra thủ công
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
        public async Task<IActionResult> EditCauHoi(int id, CauHoiThuongGap model) // Đã đổi tên biến thành "model"
        {
            if (id != model.Id) return NotFound();

            // 1. Xóa sạch mọi bắt lỗi ngầm mặc định của hệ thống
            ModelState.Clear();

            // 2. Tự kiểm tra thủ công
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

        // 1. DANH SÁCH
        public async Task<IActionResult> IndexHDNB()
        {
            var danhSach = await _context.HoatDongNoiBats
                                         .OrderByDescending(x => x.NgayTao)
                                         .ToListAsync();
            return View(danhSach);
        }

        // 2. TẠO MỚI (GET)
        public IActionResult CreateHDNB()
        {
            return View();
        }

        // 3. TẠO MỚI (POST) - Có xử lý upload ảnh
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
                    // Lấy thư mục wwwroot/images/hoatdong
                    var thuMucUpload = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "hoatdong");
                    if (!Directory.Exists(thuMucUpload)) Directory.CreateDirectory(thuMucUpload);

                    // Tạo tên file ngẫu nhiên tránh trùng lặp
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
                    model.DuongDanAnh = "/images/hoatdong/default.jpg"; // Ảnh mặc định nếu không chọn
                }

                model.NgayTao = DateTime.Now;
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexHDNB));
            }
            return View(model);
        }

        // 4. CẬP NHẬT (GET)
        public async Task<IActionResult> EditHDNB(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.HoatDongNoiBats.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        // 5. CẬP NHẬT (POST)
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
                        model.DuongDanAnh = itemCu.DuongDanAnh; // Giữ nguyên ảnh cũ nếu không chọn ảnh mới
                    }

                    model.NgayTao = itemCu.NgayTao; // Giữ nguyên ngày tạo gốc
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

        // 6. XÓA (POST)
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
    }
}