using Microsoft.AspNetCore.Mvc;
using SourceCode.Data;
using SourceCode.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(string jobId)
        {
            // 1. Lấy ID tài khoản đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            // 2. Tìm mã hồ sơ của sinh viên này
            var hoSo = await _context.HoSoSinhViens.FirstOrDefaultAsync(h => h.FK_sMaSV == userId);

            if (hoSo == null)
            {
                TempData["Error"] = "Vui lòng cập nhật hồ sơ (CV Online) trước khi ứng tuyển!";
                return RedirectToAction("Index", "CV");
            }

            // 3. Kiểm tra xem đã ứng tuyển tin này chưa để tránh trùng lặp
            var exists = await _context.UngTuyens
                .AnyAsync(u => u.FK_sMaTin == jobId && u.FK_sMaHoSo == hoSo.PK_sMaHoSo);

            if (exists)
            {
                TempData["Error"] = "Bạn đã ứng tuyển công việc này rồi!";
                return RedirectToAction("SinhVien", "Home");
            }

            // 4. Khởi tạo đối tượng ứng tuyển với đầy đủ các trường NOT NULL
            var apply = new UngTuyen
            {
                PK_sMaUngTuyen = "UT" + DateTime.Now.Ticks.ToString().Substring(10), // Tự tạo khóa chính
                FK_sMaTin = jobId,
                FK_sMaHoSo = hoSo.PK_sMaHoSo,
                dNgayUngTuyen = DateTime.Now,
                sTrangThaiUngTuyen = "Pending" // Trạng thái ban đầu: Chờ duyệt
            };

            try
            {
                _context.UngTuyens.Add(apply);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ứng tuyển thành công! Nhà tuyển dụng sẽ sớm liên hệ với bạn.";
            }
            catch (Exception ex)
            {
                // Bắt lỗi InnerException để biết chính xác cột nào bị NULL
                TempData["Error"] = "Lỗi hệ thống: " + (ex.InnerException?.Message ?? ex.Message);
            }

            // Điều hướng về lại trang danh sách việc làm
            return RedirectToAction("SinhVien", "Home");
        }
    }
}