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
            var jobsQuery = _context.Jobs.Include(j => j.Company).AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                jobsQuery = jobsQuery.Where(j => j.Title.Contains(searchString));
                ViewBag.SearchString = searchString;
            }

            var jobsList = await jobsQuery.ToListAsync();
            return View(jobsList);
        }
        public IActionResult Enterprise()
        {
            return View();
        }
    }
}