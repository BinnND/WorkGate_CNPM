using Microsoft.AspNetCore.Mvc;
using SourceCode.Models;
using System.Security.Claims;
using SourceCode.Data;

public class JobController : Controller
{
    private readonly ApplicationDbContext _context;

    public JobController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var jobs = _context.Jobs
            .Where(j => j.Status == "Approved")
            .ToList();

        return View(jobs);
    }

    public IActionResult Create() => View();

    [HttpPost]
    public IActionResult Create(Job job)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return RedirectToAction("Login", "Account");
        }

        job.CompanyId = userId;
        job.Status = "Pending";
        job.PostedDate = DateTime.Now;
        if (ModelState.IsValid)
        {
            _context.Jobs.Add(job);
            _context.SaveChanges();
            return RedirectToAction("Enterprise", "Home");
        }
        return View(job);
    }
    public IActionResult SinhVien()
    {
        var jobs = _context.Jobs
            .Where(j => j.Status == "Approved")
            .ToList();
        return View(jobs);
    }
}