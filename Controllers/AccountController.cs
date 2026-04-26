using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SourceCode.Data; 
using SourceCode.Models; 

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Register() => View();

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.PK_sUserID == userIdString);

        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    public IActionResult Register(string FullName, string Email, string Password, string Role)
    {
        if (_context.Users.Any(u => u.sEmail == Email))
        {
            ViewBag.Error = "Email này đã được sử dụng!";
            return View();
        }

        string vaiTro = Role == "Student" ? "sinhvien" : "doanhnghiep";
        string newId = "USR" + Guid.NewGuid().ToString().Substring(0, 8);
        var newUser = new User
        {
            PK_sUserID = newId,
            sHoten = FullName,
            sEmail = Email,
            sMatKhau = HashPassword(Password),
            sVaiTro = vaiTro,
            sTrangThaiTK = "Chờ duyệt",
            sSDT = ""
        };

        _context.Users.Add(newUser);
        _context.SaveChanges();

        TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
        return RedirectToAction("Login");
    }

    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.sEmail.Trim() == email.Trim());

        if (user != null && VerifyPassword(password, user.sMatKhau))
        {
            string userRole = user.sVaiTro.ToLower().Trim();
            HttpContext.Session.SetString("UserRole", userRole);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.PK_sUserID),
            new Claim(ClaimTypes.Name, user.sHoten),
            new Claim(ClaimTypes.Role, userRole)
        };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            if (userRole == "admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (userRole == "sinhvien")
            {
                return RedirectToAction("SinhVien", "Home");
            }
            else if (userRole == "doanhnghiep")
            {
                return RedirectToAction("Enterprise", "Home");
            }
        }

        ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
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
        var hp = HashPassword(password);
        return string.Equals(hp, hashed, StringComparison.OrdinalIgnoreCase);
    }
}