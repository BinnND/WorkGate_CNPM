using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using SourceCode.Data;
using SourceCode.Models;
using System.Linq;
using System.Security.Claims;

namespace SourceCode.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "sinhvien")]
        public async Task<IActionResult> SinhVien(string searchString)
        {
            var jobsQuery = _context.TinTuyenDungs.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                jobsQuery = jobsQuery.Where(j => j.sViTriCV.Contains(searchString));
                ViewBag.SearchString = searchString;
            }

            var jobsList = await jobsQuery.ToListAsync();
            return View(jobsList);
        }
        [AllowAnonymous]
        public async Task<IActionResult> EnterpriseDetails(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var enterprise = await _context.Doanhnghieps
                .FirstOrDefaultAsync(m => m.PK_sMaDN == id || m.FK_sUserID == id);

            if (enterprise == null) return NotFound();

            ViewBag.ActiveJobs = await _context.TinTuyenDungs
                .Where(t => t.FK_sMaDN == enterprise.PK_sMaDN && t.sTrangThaiTin == "Đã duyệt")
                .OrderByDescending(t => t.dNgayDang)
                .ToListAsync();

            ViewBag.TotalJobs = await _context.TinTuyenDungs
                .CountAsync(t => t.FK_sMaDN == enterprise.PK_sMaDN);

            return View(enterprise);
        }
        public async Task<IActionResult> Enterprise()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dn = await _context.Doanhnghieps.FirstOrDefaultAsync(d => d.FK_sUserID == userId);
            if (dn == null) return RedirectToAction("Login", "Account");

            var maTins = await _context.TinTuyenDungs
                .Where(t => t.FK_sMaDN == dn.PK_sMaDN)
                .Select(t => t.PK_sMaTin).ToListAsync();

            ViewBag.TotalJobs = maTins.Count;
            ViewBag.TotalApps = await _context.UngTuyens.CountAsync(u => maTins.Contains(u.FK_sMaTin));
            ViewBag.PendingApps = await _context.UngTuyens.CountAsync(u => maTins.Contains(u.FK_sMaTin) && u.sTrangThaiUngTuyen == "Pending");

            return View();
        }
        public async Task<IActionResult> Applicants()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dn = await _context.Doanhnghieps.FirstOrDefaultAsync(d => d.FK_sUserID == userId);
            if (dn == null) return RedirectToAction("Login", "Account");

            var maTins = await _context.TinTuyenDungs
                .Where(t => t.FK_sMaDN == dn.PK_sMaDN)
                .Select(t => t.PK_sMaTin)
                .ToListAsync();

            var ungTuyens = await _context.UngTuyens
                .Include(u => u.TinTuyenDung)
                .Include(u => u.HoSoSinhVien)
                .Where(u => maTins.Contains(u.FK_sMaTin))
                .OrderByDescending(u => u.dNgayUngTuyen)
                .ToListAsync();

            return View(ungTuyens);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveApplicant(string id)
        {
            var app = await _context.UngTuyens.FindAsync(id);
            if (app != null) { app.sTrangThaiUngTuyen = "Approved"; await _context.SaveChangesAsync(); }
            return RedirectToAction("Applicants");
        }

        [HttpPost]
        public async Task<IActionResult> RejectApplicant(string id)
        {
            var app = await _context.UngTuyens.FindAsync(id);
            if (app != null) { app.sTrangThaiUngTuyen = "Rejected"; await _context.SaveChangesAsync(); }
            return RedirectToAction("Applicants");
        }
    }
}