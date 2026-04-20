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

            var applications = await _context.Applications
                .Include(a => a.Job)
                .Include(a => a.Student)
                .Where(a => a.Job.CompanyId == int.Parse(userId))
                .ToListAsync();

            return View(applications);
        }

        public IActionResult CreateJob() => View();

        public async Task<IActionResult> QuanLyTuyenDung()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var jobs = await _context.Jobs.Where(j => j.CompanyId == userId).ToListAsync();
            return View(jobs);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app != null) { app.Status = "Approved"; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app != null) { app.Status = "Rejected"; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}