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

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TinTuyenDung job)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var dn = await _context.Doanhnghieps.FirstOrDefaultAsync(d => d.FK_sUserID == userId);
            if (dn == null) return RedirectToAction("Login", "Account");

            job.PK_sMaTin = "TT" + DateTime.Now.Ticks.ToString().Substring(10);
            job.FK_sMaDN = dn.PK_sMaDN; 
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
                return RedirectToAction("Index"); 
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
                .Where(j => j.sTrangThaiTin == "Đã duyệt")
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
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dn = await _context.Doanhnghieps.FirstOrDefaultAsync(d => d.FK_sUserID == userId);
            if (dn == null) return RedirectToAction("Login", "Account");

            var jobs = await _context.TinTuyenDungs
                .Where(j => j.FK_sMaDN == dn.PK_sMaDN)
                .OrderByDescending(j => j.dNgayDang)
                .ToListAsync();

            return View(jobs);
        }
        [HttpGet]
        public async Task<IActionResult> EditJob(string id) 
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var job = await _context.TinTuyenDungs.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dn = await _context.Doanhnghieps.FirstOrDefaultAsync(d => d.FK_sUserID == userId);

            if (job == null || job.FK_sMaDN != dn?.PK_sMaDN)
            {
                return NotFound(); 
            }

            return View(job); 
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(TinTuyenDung model)
        {
            // Loại bỏ kiểm tra các trường không có trong form để tránh IsValid = false
            ModelState.Remove("FK_sMaDN");
            ModelState.Remove("sTrangThaiTin");

            // Kiểm tra thời gian (MS_03)
            if (model.dHanNop < DateTime.Now)
            {
                ModelState.AddModelError("dHanNop", "Hạn nộp hồ sơ không được nhỏ hơn ngày hiện tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var jobInDb = await _context.TinTuyenDungs.FindAsync(model.PK_sMaTin);
                    if (jobInDb != null)
                    {
                        // Cập nhật dữ liệu từ form
                        jobInDb.sViTriCV = model.sViTriCV;
                        jobInDb.tMoTaCV = model.tMoTaCV;
                        jobInDb.sYeuCauChuyenMon = model.sYeuCauChuyenMon; // Cập nhật thêm trường này
                        jobInDb.iSoLuong = model.iSoLuong;
                        jobInDb.fMucLuong = model.fMucLuong;
                        jobInDb.sDiaDiem = model.sDiaDiem;
                        jobInDb.dHanNop = model.dHanNop;

                        // THIẾT LẬP TRẠNG THÁI VÀ NGÀY CẬP NHẬT
                        jobInDb.sTrangThaiTin = "Chờ duyệt";
                        // jobInDb.dNgayCapNhat = DateTime.Now; // Nếu model của bạn có trường này

                        _context.Update(jobInDb);
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "Cập nhật thành công (MS_Success)";
                        return RedirectToAction("Index"); // Quay về trang quản lý
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống khi cập nhật (MS_05)");
                }
            }
            // Nếu có lỗi, trả về View kèm thông báo lỗi cụ thể
            return View(model);
        }
    }

}