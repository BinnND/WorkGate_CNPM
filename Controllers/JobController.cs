using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SourceCode.Models;
using System.Security.Claims;
using SourceCode.Data;
using Microsoft.EntityFrameworkCore;

namespace SourceCode.Controllers
{
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var jobs = _context.TinTuyenDungs
                .Where(j => j.sTrangThaiTin == "Approved")
                .ToList();

            return View(jobs);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TinTuyenDung job)
        {
            var businessId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(businessId)) return RedirectToAction("Login", "Account");
            job.PK_sMaTin = "TT" + DateTime.Now.Ticks.ToString().Substring(10);
            job.FK_sMaDN = businessId;
            job.dNgayDang = DateTime.Now;
            job.sTrangThaiTin = "Chờ duyệt";

            if (string.IsNullOrEmpty(job.sDiaDiem)) job.sDiaDiem = "Toàn quốc";
            if (string.IsNullOrEmpty(job.sYeuCauChuyenMon)) job.sYeuCauChuyenMon = "Trao đổi khi phỏng vấn";
            job.sGhiChuTuChoi = "";

            try
            {
                _context.TinTuyenDungs.Add(job);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đăng tin tuyển dụng thành công!";
                return RedirectToAction("Enterprise", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi lưu dữ liệu: " + (ex.InnerException?.Message ?? ex.Message));
                return View(job);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(string idTin)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var hoSo = await _context.HoSoSinhViens.FirstOrDefaultAsync(h => h.FK_sMaSV == userId);

            if (hoSo == null)
            {
                TempData["Error"] = "Bạn cần tạo hồ sơ trước khi ứng tuyển!";
                return RedirectToAction("Create", "Profile");
            }

            var daUngTuyen = await _context.UngTuyens
                .AnyAsync(u => u.FK_sMaTin == idTin && u.FK_sMaHoSo == hoSo.PK_sMaHoSo);

            if (daUngTuyen)
            {
                TempData["Error"] = "Bạn đã ứng tuyển công việc này rồi!";
                return RedirectToAction("SinhVien");
            }

            var application = new UngTuyen
            {
                PK_sMaUngTuyen = "UT" + DateTime.Now.Ticks.ToString().Substring(10),
                FK_sMaTin = idTin,
                FK_sMaHoSo = hoSo.PK_sMaHoSo,
                dNgayUngTuyen = DateTime.Now,
                sTrangThaiUngTuyen = "Pending"
            };

            try
            {
                _context.UngTuyens.Add(application);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ứng tuyển thành công! Nhà tuyển dụng sẽ sớm liên hệ với bạn.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return RedirectToAction("SinhVien");
        }

        public IActionResult SinhVien()
        {
            var jobs = _context.TinTuyenDungs
                .Where(j => j.sTrangThaiTin == "Approved")
                .OrderByDescending(j => j.dNgayDang)
                .ToList();
            return View("~/Views/Home/SinhVien.cshtml", jobs);
        }
        public async Task<IActionResult> EnterpriseDetails(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var doanhNghiep = await _context.Doanhnghieps
                .FirstOrDefaultAsync(m => m.PK_sMaDN == id);

            if (doanhNghiep == null) return NotFound();

            return View("~/Views/Home/EnterpriseDetails.cshtml", doanhNghiep);
        }
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PK_sUserID == userId);

            if (user == null)
            {
                return NotFound();
            }
            return View("~/Views/Account/Profile.cshtml", user);
        }
       
    }

}