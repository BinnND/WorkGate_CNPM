using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SourceCode.Data;
using SourceCode.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SourceCode.Controllers
{
    [Authorize(Roles = "doanhnghiep")]
    public class EnterpriseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnterpriseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var applications = await _context.UngTuyens
                .Include(a => a.TinTuyenDung)
                .Include(a => a.HoSoSinhVien)
                // So sánh trực tiếp chuỗi với chuỗi
                .Where(a => a.TinTuyenDung.FK_sMaDN == userId)
                .ToListAsync();

            return View(applications);
        }

        public IActionResult CreateJob() => View();

        public async Task<IActionResult> QuanLyTuyenDung()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var jobs = await _context.TinTuyenDungs
                .Where(j => j.FK_sMaDN == userId)
                .ToListAsync();

            return View(jobs);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var app = await _context.UngTuyens.FindAsync(id);
            if (app != null) { app.sTrangThaiUngTuyen = "Approved"; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var app = await _context.UngTuyens.FindAsync(id);
            if (app != null) { app.sTrangThaiUngTuyen = "Rejected"; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}