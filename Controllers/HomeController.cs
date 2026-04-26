using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using SourceCode.Data;
using SourceCode.Models;
using System.Linq;

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
        public IActionResult Enterprise()
        {
            return View();
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
    }
}