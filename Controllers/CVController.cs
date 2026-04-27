using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SourceCode.Data;
using SourceCode.Models;
using System.Security.Claims;

namespace SourceCode.Controllers
{
    [Authorize(Roles = "sinhvien")]
    public class CVController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CVController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================== INDEX ==================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var sv = await _context.SinhViens
                .FirstOrDefaultAsync(x => x.FK_sUserID == userId);

            if (sv == null)
            {
                return View("CV", new List<HoSoSinhVien>());
            }

            var cvs = await _context.HoSoSinhViens
                .Where(c => c.FK_sMaSV == sv.PK_sMaSV) // ✅ FIX
                .ToListAsync();

            return View("CV", cvs);
        }

        // ================== CREATE ==================
        [HttpPost]
        public async Task<IActionResult> Create(HoSoSinhVien cv, IFormFile fileCV)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            // Upload file
            if (fileCV != null && fileCV.Length > 0)
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileCV.FileName);
                string uploadPath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await fileCV.CopyToAsync(stream);
                }

                cv.sFileCV = "/uploads/" + fileName;
            }
            else
            {
                cv.sFileCV = "N/A";
            }

            cv.PK_sMaHoSo = "CV" + Guid.NewGuid().ToString("N").Substring(0, 8);

            // ✅ LẤY SINH VIÊN (AUTO CREATE NẾU CHƯA CÓ)
            var sv = await _context.SinhViens
                .FirstOrDefaultAsync(x => x.FK_sUserID == userId);

            if (sv == null)
            {
                sv = new SinhVien
                {
                    PK_sMaSV = "SV" + Guid.NewGuid().ToString("N").Substring(0, 6),
                    FK_sUserID = userId,
                    sHoTen = User.Identity.Name ?? "Chưa cập nhật",
                    sTinhTrangViecLam = "Đang tìm việc",
                    sSDT = "",
                    sLop = "",
                    sNganhHoc = "",
                    sKhoaHoc = ""
                };

                _context.SinhViens.Add(sv);
                await _context.SaveChangesAsync();
            }

            cv.FK_sMaSV = sv.PK_sMaSV; // ✅ FIX CHUẨN
            cv.dNgayTao = DateTime.Now;
            cv.tThongTinKhac = cv.tThongTinKhac ?? "";
            cv.tKinhNghiem = cv.tKinhNghiem ?? "";

            _context.HoSoSinhViens.Add(cv);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ================== EDIT ==================
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var cv = await _context.HoSoSinhViens.FindAsync(id);
            if (cv == null) return NotFound();
            return View(cv);
        }

        // ================== UPDATE ==================
        [HttpPost]
        public async Task<IActionResult> Update(HoSoSinhVien cv, IFormFile fileCV)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sv = await _context.SinhViens
                .FirstOrDefaultAsync(x => x.FK_sUserID == userId);

            if (sv == null)
            {
                sv = new SinhVien
                {
                    PK_sMaSV = "SV" + Guid.NewGuid().ToString("N").Substring(0, 6),
                    FK_sUserID = userId,
                    sHoTen = User.Identity.Name ?? "Chưa cập nhật",
                    sTinhTrangViecLam = "Đang tìm việc"
                };

                _context.SinhViens.Add(sv);
                await _context.SaveChangesAsync();
            }

            cv.FK_sMaSV = sv.PK_sMaSV;

            if (fileCV != null && fileCV.Length > 0)
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileCV.FileName);
                string uploadPath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await fileCV.CopyToAsync(stream);
                }

                cv.sFileCV = "/uploads/" + fileName;
            }

            cv.tThongTinKhac = cv.tThongTinKhac ?? "N/A";
            cv.sFileCV = cv.sFileCV ?? "N/A";

            _context.HoSoSinhViens.Update(cv);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ================== DELETE ==================
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var cv = await _context.HoSoSinhViens.FindAsync(id);
            if (cv != null)
            {
                _context.HoSoSinhViens.Remove(cv);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}