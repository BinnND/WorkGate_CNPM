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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

            var cvs = await _context.HoSoSinhViens
                .Where(c => c.FK_sMaSV == userIdClaim)
                .ToListAsync();
            return View("CV", cvs);
        }

        [HttpPost]
        public async Task<IActionResult> Create(HoSoSinhVien cv, IFormFile fileCV)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            if (fileCV != null && fileCV.Length > 0)
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

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

            cv.PK_sMaHoSo = "CV" + Guid.NewGuid().ToString().Substring(0, 8);
            cv.FK_sMaSV = userId; 
            cv.dNgayTao = DateTime.Now;
            cv.tThongTinKhac = string.IsNullOrEmpty(cv.tThongTinKhac) ? "" : cv.tThongTinKhac;
            cv.tKinhNghiem = string.IsNullOrEmpty(cv.tKinhNghiem) ? "" : cv.tKinhNghiem;

            _context.HoSoSinhViens.Add(cv);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var cv = await _context.HoSoSinhViens.FindAsync(id);
            if (cv == null) return NotFound();
            return View(cv);
        }

        [HttpPost]
        public async Task<IActionResult> Update(HoSoSinhVien cv, IFormFile fileCV)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            cv.FK_sMaSV = userId;

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

            if (string.IsNullOrEmpty(cv.tThongTinKhac)) cv.tThongTinKhac = "N/A";
            if (string.IsNullOrEmpty(cv.sFileCV)) cv.sFileCV = "N/A";

            _context.HoSoSinhViens.Update(cv);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
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