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

            if (enterprise == null)
            {
                return NotFound();
            }

            return View(enterprise); 
        }
        public async Task<IActionResult> Enterprise()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dn = await _context.Doanhnghieps.FirstOrDefaultAsync(d => d.FK_sUserID == userId);
            if (dn == null) return RedirectToAction("Login", "Account");

            // Danh sách tin của doanh nghiệp
            var tins = await _context.TinTuyenDungs
                .Where(t => t.FK_sMaDN == dn.PK_sMaDN)
                .OrderByDescending(t => t.dNgayDang)
                .ToListAsync();

            // Danh sách ứng viên cho các tin đó
            var maTins = tins.Select(t => t.PK_sMaTin).ToList();
            var ungTuyens = await _context.UngTuyens
                .Include(u => u.TinTuyenDung)
                .Include(u => u.HoSoSinhVien)
                .Where(u => maTins.Contains(u.FK_sMaTin))
                .ToListAsync();

            ViewBag.TinTuyenDungs = tins;
            ViewBag.UngTuyens = ungTuyens;
            ViewBag.TenDN = dn.sTenDN;
            return View();
        }
    }
}