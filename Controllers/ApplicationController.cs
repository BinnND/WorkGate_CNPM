using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SourceCode.Data;
using SourceCode.Models;
using System.Security.Claims;

namespace SourceCode.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApplicationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "sinhvien")]
        public async Task<IActionResult> Apply(int jobId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdString);
            var userCv = await _context.CVs.FirstOrDefaultAsync(c => c.UserId == userId);
            if (userCv == null)
            {
                TempData["Error"] = "Bạn cần tạo CV trước khi ứng tuyển!";
                return RedirectToAction("Index", "CV");
            }
            var application = new Application
            {
                JobId = jobId,
                UserId = userId,
                CVId = userCv.Id,
                ApplyDate = DateTime.Now,
                Status = "Pending" 
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ứng tuyển thành công!";
            return RedirectToAction("SinhVien", "Home");
        }
    }
}