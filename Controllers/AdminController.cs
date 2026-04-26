using Microsoft.AspNetCore.Mvc;
using SourceCode.Data;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        ViewBag.Users = _context.Users.Count();
        ViewBag.Jobs = _context.TinTuyenDungs.Count();
        ViewBag.Applications = _context.UngTuyens.Count();

        return View();
    }

    public IActionResult Users()
    {
        return View(_context.Users.ToList());
    }

    public IActionResult ApproveCompany(int id)
    {
        var user = _context.Users.Find(id);
        user.sTrangThaiTK = "Approved";
        _context.SaveChanges();

        return RedirectToAction("Users");
    }

    public IActionResult JobsPending()
    {
        return View(_context.TinTuyenDungs.Where(j => j.sTrangThaiTin == "Pending").ToList());
    }

    public IActionResult ApproveJob(int id)
    {
        var job = _context.TinTuyenDungs.Find(id);
        job.sTrangThaiTin = "Approved";
        _context.SaveChanges();

        return RedirectToAction("JobsPending");
    }
}