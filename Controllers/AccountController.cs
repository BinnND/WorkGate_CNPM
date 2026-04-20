using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Thêm để dùng các hàm Async
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SourceCode.Data; // Thay bằng namespace thực tế của bạn
using SourceCode.Models; // Thay bằng namespace thực tế của bạn

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
        // Sử dụng User.FindFirstValue để lấy ID
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToAction("Login");
        }

        int userId = int.Parse(userIdString);

        // Tìm user trong Database để lấy dữ liệu mới nhất
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    public IActionResult Register(string FullName, string Email, string Password, string Role)
    {
        if (_context.Users.Any(u => u.Email == Email))
        {
            ViewBag.Error = "Email này đã được sử dụng!";
            return View();
        }

        // Chuyển đổi Role từ giao diện sang Role trong DB
        string vaiTro = Role == "Student" ? "sinhvien" : "doanhnghiep";

        var newUser = new User
        {
            FullName = FullName,
            Email = Email,
            Password = HashPassword(Password),
            Role = vaiTro,
            Status = "Chờ duyệt",
            Phone = ""
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
        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        if (user != null && VerifyPassword(password, user.Password))
        {
            // Quan trọng: Lưu ID vào NameIdentifier để hàm Profile lấy được
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName), // Lưu tên để hiển thị cạnh chữ NA
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Điều hướng dựa trên Role
            if (user.Role == "sinhvien")
            {
                return RedirectToAction("SinhVien", "Home");
            }
            else if (user.Role == "doanhnghiep")
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