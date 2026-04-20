using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SourceCode.Data; // Đảm bảo đúng namespace của ApplicationDbContext
using SourceCode.Models;
using System.Security.Claims;

namespace SourceCode.Controllers
{
    [Authorize(Roles = "sinhvien")] // Chỉ cho phép sinh viên truy cập
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

            int userId = int.Parse(userIdClaim);

            var cvs = await _context.CVs
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View("CV", cvs);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CV cv)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim);

            cv.UserId = userId;
            cv.CreatedAt = DateTime.Now;

            _context.CVs.Add(cv);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}