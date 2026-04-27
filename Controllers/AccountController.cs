using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SourceCode.Data;
using SourceCode.Models;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Register() => View();
    public IActionResult Login() => View();

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }

    
    private (bool isValid, string errorMessage) ValidateAccountData(
        string email, string password, string role,
        string hoTen, string sdt, string lop, string nganh, string khoa,   // sinh viên
        string maSV,
        string tenDN, string diaChi, string nguoiDD, string maSoThue, string giayPhep) // doanh nghiệp
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            return (false, "Vui lòng điền đầy đủ thông tin bắt buộc.");

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return (false, "Định dạng Email không hợp lệ.");

        if (email.Length > 50)
            return (false, "Email không được vượt quá 50 ký tự.");

        if (password.Length < 6)
            return (false, "Mật khẩu phải có ít nhất 6 ký tự.");

        if (role == "sinhvien")
        {
            if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(maSV) ||
                string.IsNullOrWhiteSpace(lop) || string.IsNullOrWhiteSpace(nganh) || string.IsNullOrWhiteSpace(khoa))
                return (false, "Vui lòng điền đầy đủ thông tin sinh viên.");
        }
        else if (role == "doanhnghiep")
        {
            if (string.IsNullOrWhiteSpace(tenDN) || string.IsNullOrWhiteSpace(diaChi) ||
                string.IsNullOrWhiteSpace(nguoiDD) || string.IsNullOrWhiteSpace(maSoThue) || string.IsNullOrWhiteSpace(giayPhep))
                return (false, "Vui lòng điền đầy đủ thông tin doanh nghiệp.");
        }
        else
        {
            return (false, "Vai trò không hợp lệ.");
        }

        return (true, "");
    }

  
    private async Task<(bool isUnique, string errorMessage)> CheckPersistenceAsync(
        string email, string role, string maSV, string maSoThue)
    {
        // Kiểm tra email trùng
        if (await _context.Users.AnyAsync(u => u.sEmail == email))
            return (false, "Email này đã được sử dụng.");

        if (role == "sinhvien" && !string.IsNullOrWhiteSpace(maSV))
        {
            if (await _context.SinhViens.AnyAsync(s => s.PK_sMaSV == maSV))
                return (false, "Mã sinh viên đã tồn tại trong hệ thống.");
        }

        if (role == "doanhnghiep" && !string.IsNullOrWhiteSpace(maSoThue))
        {
            if (await _context.Doanhnghieps.AnyAsync(d => d.FK_sMaSoThue == maSoThue))
                return (false, "Mã số thuế đã tồn tại trong hệ thống.");
        }

        return (true, "");
    }

  
    private async Task<(bool success, string errorMessage)> ExecuteRegistrationAsync(
        string email, string password, string role,
        string hoTen, string sdt, string lop, string nganh, string khoa, string maSV,
        string tenDN, string diaChi, string nguoiDD, string maSoThue, string giayPhep)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            string userId = "USR" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            string hashedPassword = HashPassword(password);

            string trangThaiTK = role == "sinhvien" ? "active" : "Chờ duyệt";

            var newUser = new User
            {
                PK_sUserID   = userId,
                sHoten       = role == "sinhvien" ? hoTen : nguoiDD,
                sEmail       = email,
                sMatKhau     = hashedPassword,
                sVaiTro      = role,
                sTrangThaiTK = trangThaiTK,
                sSDT         = sdt ?? "",
                dNgayTao     = DateTime.Now
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

           
            if (role == "sinhvien")
            {
                var sv = new SinhVien
                {
                    PK_sMaSV           = maSV,
                    FK_sUserID         = userId,
                    sHoTen             = hoTen,
                    sSDT               = sdt ?? "",
                    sLop               = lop,
                    sNganhHoc          = nganh,
                    sKhoaHoc           = khoa,
                    sTinhTrangViecLam  = "Đang tìm việc"
                };
                _context.SinhViens.Add(sv);
            }
            else
            {
                string maDN = "DN" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                var dn = new Doanhnghiep
                {
                    PK_sMaDN         = maDN,
                    FK_sUserID       = userId,
                    FK_sMaSoThue     = maSoThue,
                    sTenDN           = tenDN,
                    sDiaChi          = diaChi,
                    sNguoiDaiDien    = nguoiDD,
                    sGiayPhepKD      = giayPhep,
                    sTrangThaiDuyet  = "Chờ duyệt",
                    dNgayKichHoat    = null
                };
                _context.Doanhnghieps.Add(dn);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, "");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, "Lỗi hệ thống, vui lòng thử lại. " + (ex.InnerException?.Message ?? ex.Message));
        }
    }

   
    [HttpPost]
    public async Task<IActionResult> Register(
        string Email, string Password, string Role,
        
        string HoTen, string SDT, string Lop, string NganhHoc, string KhoaHoc, string MaSV,
        
        string TenDN, string DiaChi, string NguoiDaiDien, string MaSoThue, string GiayPhepKD)
    {
        string role = Role == "Student" ? "sinhvien" : "doanhnghiep";

        var (isValid, validErr) = ValidateAccountData(
            Email, Password, role,
            HoTen, SDT, Lop, NganhHoc, KhoaHoc, MaSV,
            TenDN, DiaChi, NguoiDaiDien, MaSoThue, GiayPhepKD);

        if (!isValid)
        {
            ViewBag.Error = validErr;
            ViewBag.Role = Role;
            return View();
        }

        var (isUnique, uniqueErr) = await CheckPersistenceAsync(Email, role, MaSV, MaSoThue);
        if (!isUnique)
        {
            ViewBag.Error = uniqueErr;
            ViewBag.Role = Role;
            return View();
        }

        var (success, execErr) = await ExecuteRegistrationAsync(
            Email, Password, role,
            HoTen, SDT, Lop, NganhHoc, KhoaHoc, MaSV,
            TenDN, DiaChi, NguoiDaiDien, MaSoThue, GiayPhepKD);

        if (!success)
        {
            ViewBag.Error = execErr;
            ViewBag.Role = Role;
            return View();
        }

        if (role == "doanhnghiep")
        {
            TempData["Success"] = "Đăng ký thành công! Tài khoản đang chờ Admin phê duyệt. Bạn sẽ có thể đăng nhập sau khi được duyệt.";
        }
        else
        {
            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
        }
        return RedirectToAction("Login");
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Vui lòng nhập đầy đủ Email và Mật khẩu.";
            return View();
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ViewBag.Error = "Định dạng Email không hợp lệ.";
            return View();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.sEmail.Trim() == email.Trim());

        if (user == null || !VerifyPassword(password, user.sMatKhau))
        {
            ViewBag.Error = "Email hoặc mật khẩu không chính xác.";
            return View();
        }

        var trangThai = user.sTrangThaiTK?.ToLower().Trim();

        if (trangThai == "chờ duyệt" || trangThai == "cho duyet")
        {
            ViewBag.Error = "Tài khoản đang chờ Admin phê duyệt. Vui lòng thử lại sau.";
            return View();
        }

        if (trangThai == "từ chối" || trangThai == "tu choi")
        {
            ViewBag.Error = "Tài khoản đã bị từ chối. Vui lòng liên hệ nhà trường.";
            return View();
        }

        if (trangThai == "locked" || trangThai == "khóa")
        {
            ViewBag.Error = "Tài khoản đã bị khóa. Vui lòng liên hệ Admin.";
            return View();
        }

        if (trangThai != "active")
        {
            ViewBag.Error = "Tài khoản không hợp lệ. Vui lòng liên hệ Admin.";
            return View();
        }

        string userRole = user.sVaiTro.ToLower().Trim();

        HttpContext.Session.SetString("UserRole", userRole);
        HttpContext.Session.SetString("PK_sUserID", user.PK_sUserID);
        HttpContext.Session.SetString("sEmail", user.sEmail);
        HttpContext.Session.SetString("sHoTen", user.sHoten ?? "");

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.PK_sUserID),
        new Claim(ClaimTypes.Name,           user.sHoten ?? ""),
        new Claim(ClaimTypes.Email,          user.sEmail),
        new Claim(ClaimTypes.Role,           userRole)
    };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (userRole == "admin")
            return RedirectToAction("Index", "Admin");
        else if (userRole == "sinhvien")
            return RedirectToAction("SinhVien", "Home");
        else if (userRole == "doanhnghiep")
            return RedirectToAction("Enterprise", "Home");

        ViewBag.Error = "Vai trò tài khoản không hợp lệ.";
        return View();
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    private static bool VerifyPassword(string password, string hashed)
    {
        return string.Equals(HashPassword(password), hashed, StringComparison.OrdinalIgnoreCase);
    }
    public async Task<IActionResult> Profile()
    {
        var userId = HttpContext.Session.GetString("PK_sUserID");
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login");

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return RedirectToAction("Login");

        if (user.sVaiTro?.ToLower().Trim() == "doanhnghiep")
        {
            var dn = await _context.Doanhnghieps.FirstOrDefaultAsync(d => d.FK_sUserID == userId);
            ViewBag.DiaChi = dn?.sDiaChi ?? "";
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(string sHoTen, string sSDT, string sDiaChi, string oldPassword, string newPassword)
    {
        var userId = HttpContext.Session.GetString("PK_sUserID");
        var role = HttpContext.Session.GetString("UserRole");

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login");
        }

        // 1. Kiểm tra dữ liệu cơ bản
        if (string.IsNullOrWhiteSpace(sHoTen))
        {
            TempData["Error"] = "Họ và tên không được để trống.";
            return RedirectToAction("Profile");
        }

        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // 2. LOGIC ĐỔI MẬT KHẨU
            // Chỉ xử lý nếu một trong hai ô mật khẩu có dữ liệu
            if (!string.IsNullOrWhiteSpace(oldPassword) || !string.IsNullOrWhiteSpace(newPassword))
            {
                if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
                {
                    TempData["Error"] = "Vui lòng nhập đầy đủ cả mật khẩu cũ và mới.";
                    return RedirectToAction("Profile");
                }

                // Kiểm tra mật khẩu cũ (Sử dụng hàm VerifyPassword có sẵn trong file của bạn)
                if (!VerifyPassword(oldPassword, user.sMatKhau))
                {
                    TempData["Error"] = "Mật khẩu cũ không chính xác.";
                    return RedirectToAction("Profile");
                }

                if (newPassword.Length < 6)
                {
                    TempData["Error"] = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                    return RedirectToAction("Profile");
                }

                // Hash mật khẩu mới (Sử dụng hàm HashPassword có sẵn trong file của bạn)
                user.sMatKhau = HashPassword(newPassword);
            }

            // 3. CẬP NHẬT THÔNG TIN CÁ NHÂN
            user.sHoten = sHoTen;
            user.sSDT = sSDT ?? "";

            if (role == "sinhvien")
            {
                var sv = await _context.SinhViens.FirstOrDefaultAsync(s => s.FK_sUserID == userId);
                if (sv != null)
                {
                    sv.sHoTen = sHoTen;
                    sv.sSDT = sSDT ?? "";
                }
            }
            else if (role == "doanhnghiep")
            {
                var dn = await _context.Doanhnghieps.FirstOrDefaultAsync(d => d.FK_sUserID == userId);
                if (dn != null)
                {
                    dn.sNguoiDaiDien = sHoTen; 
                    dn.sDiaChi = sDiaChi ?? "";
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật thông tin thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
        }

        return RedirectToAction("Profile");
    }

    /*chức năng quên mật khẩu*/

    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string sEmail)
    {
        if (string.IsNullOrWhiteSpace(sEmail))
        {
            ViewBag.Error = "Vui lòng nhập Email.";
            return View();
        }

        if (!Regex.IsMatch(sEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ViewBag.Error = "Định dạng Email không hợp lệ (phải có @ và .com, .vn...).";
            return View();
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.sEmail.Trim() == sEmail.Trim());

        if (user == null)
        {
            ViewBag.Error = "Email này chưa được đăng ký trong hệ thống.";
            return View();
        }

        string tempPassword = GenerateRandomPassword(8);

        user.sMatKhau = HashPassword(tempPassword);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Lỗi hệ thống khi cập nhật mật khẩu.";
            return View();
        }

        bool isMailSent = await SendEmailAsync(user.sEmail, tempPassword);

        if (isMailSent)
        {
            TempData["Success"] = $"Mật khẩu mới đã được gửi đến {user.sEmail}. Vui lòng kiểm tra hòm thư (kể cả mục Thư rác/Spam).";
            return RedirectToAction("Login");
        }
        else
        {
            ViewBag.Error = "Đã cập nhật mật khẩu nhưng hệ thống gửi mail đang gặp sự cố. Vui lòng liên hệ Admin.";
            return View();
        }
    }

    private string GenerateRandomPassword(int length)
    {
        const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$";
        Random random = new Random();
        return new string(Enumerable.Repeat(validChars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private async Task<bool> SendEmailAsync(string toEmail, string newPassword)
    {
        try
        {
            string fromEmail = "nguyenthanhbinh16r@gmail.com";
            string appPassword = "hvny mzfb ugwr fvau"; 

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, "Hệ thống WorkGate"),
                Subject = "Khôi phục mật khẩu - Hệ thống WorkGate",
                Body = $"<h2>Yêu cầu cấp lại mật khẩu</h2>" +
                       $"<p>Chào bạn,</p>" +
                       $"<p>Hệ thống WorkGate đã nhận được yêu cầu khôi phục mật khẩu cho tài khoản <b>{toEmail}</b>.</p>" +
                       $"<p>Mật khẩu tạm thời của bạn là: <strong style='color:red; font-size:18px;'>{newPassword}</strong></p>" +
                       $"<p>Vui lòng đăng nhập và đổi mật khẩu ngay để đảm bảo an toàn.</p>",
                IsBodyHtml = true, 
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
