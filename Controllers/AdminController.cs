using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SourceCode.Data;
using SourceCode.Models;
using ClosedXML.Excel;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Index()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(role) || role.ToLower() != "admin")
            return RedirectToAction("Login", "Account");

        ViewBag.TotalJobs         = await _context.TinTuyenDungs.CountAsync();
        ViewBag.TotalStudents     = await _context.Users.CountAsync(u => u.sVaiTro == "sinhvien");
        ViewBag.TotalApplications = await _context.UngTuyens.CountAsync();
        ViewBag.JobRate           = 0;
        ViewBag.PendingJobsCount  = await _context.TinTuyenDungs.CountAsync(t => t.sTrangThaiTin == "Chờ duyệt");

        ViewBag.RecentJobs = await _context.TinTuyenDungs
            .OrderByDescending(j => j.PK_sMaTin)
            .Take(5)
            .ToListAsync() ?? new List<TinTuyenDung>();

        ViewBag.TopCompanies = await _context.Doanhnghieps
            .Where(dn => dn.sTrangThaiDuyet == "Đã duyệt")
            .Select(dn => new {
                dn.sTenDN,
                SoTin = _context.TinTuyenDungs.Count(t => t.FK_sMaDN == dn.PK_sMaDN)
            })
            .OrderByDescending(x => x.SoTin)
            .Take(5)
            .ToListAsync();

        return View();
    }

    public IActionResult Users()
    {
        return View(_context.Users.ToList());
    }
    [HttpPost]
    public async Task<IActionResult> LockUser(string id)
    {
        // Unit 1: validatePermission
        var adminRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(adminRole) || adminRole.ToLower() != "admin")
        {
            TempData["Error"] = "Bạn không có quyền thực hiện chức năng này.";
            return RedirectToAction("Users");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("Users");
            }

            user.sTrangThaiTK = "locked";
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = $"Đã khóa tài khoản: {user.sHoten}";
        }
        catch
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Lỗi hệ thống, vui lòng thử lại.";
        }

        return RedirectToAction("Users");
    }

    [HttpPost]
    public async Task<IActionResult> UnlockUser(string id)
    {
        
        var adminRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(adminRole) || adminRole.ToLower() != "admin")
        {
            TempData["Error"] = "Bạn không có quyền thực hiện chức năng này.";
            return RedirectToAction("Users");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("Users");
            }

            user.sTrangThaiTK = "active";
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = $"Đã mở khóa tài khoản: {user.sHoten}";
        }
        catch
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Lỗi hệ thống, vui lòng thử lại.";
        }

        return RedirectToAction("Users");
    }

    public async Task<IActionResult> Companies()
    {
        var companies = await _context.Doanhnghieps
            .OrderBy(d => d.sTrangThaiDuyet)
            .ToListAsync();
        return View(companies);
    }

  
    [HttpPost]
    public async Task<IActionResult> ApproveCompany(string id)
    {
        
        var adminRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(adminRole) || adminRole.ToLower() != "admin")
        {
            TempData["Error"] = "Bạn không có quyền thực hiện chức năng này.";
            return RedirectToAction("Companies");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
           
            var dn = await _context.Doanhnghieps.FindAsync(id);
            if (dn == null)
            {
                TempData["Error"] = "Không tìm thấy doanh nghiệp.";
                return RedirectToAction("Companies");
            }

          
            dn.sTrangThaiDuyet = "Đã duyệt";
            dn.dNgayKichHoat   = DateTime.Now;

           
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PK_sUserID == dn.FK_sUserID);
            if (user != null)
                user.sTrangThaiTK = "active";

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

          
            TempData["Success"] = $"Đã duyệt doanh nghiệp: {dn.sTenDN}";
        }
        catch
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Lỗi hệ thống, vui lòng thử lại.";
        }

        return RedirectToAction("Companies");
    }

   
    [HttpPost]
    public async Task<IActionResult> RejectCompany(string id)
    {
        
        var adminRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(adminRole) || adminRole.ToLower() != "admin")
        {
            TempData["Info"] = "Bạn không có quyền thực hiện chức năng này.";
            return RedirectToAction("Companies");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
          
            var dn = await _context.Doanhnghieps.FindAsync(id);
            if (dn == null)
            {
                TempData["Error"] = "Không tìm thấy doanh nghiệp.";
                return RedirectToAction("Companies");
            }

           
            dn.sTrangThaiDuyet = "Từ chối";

          
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PK_sUserID == dn.FK_sUserID);
            if (user != null)
                user.sTrangThaiTK = "locked";

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

           
            TempData["Error"] = $"Đã từ chối doanh nghiệp: {dn.sTenDN}";
        }
        catch
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Lỗi hệ thống, vui lòng thử lại.";
        }

        return RedirectToAction("Companies");
    }

   
    public IActionResult JobsPending()
    {
        return View(_context.TinTuyenDungs
            .Where(j => j.sTrangThaiTin == "Chờ duyệt")
            .OrderByDescending(j => j.dNgayDang)
            .ToList());
    }

    [HttpPost]
    public async Task<IActionResult> ApproveJob(string id)
    {
        
        var adminRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(adminRole) || adminRole.ToLower() != "admin")
        {
            TempData["Error"] = "Bạn không có quyền thực hiện chức năng này.";
            return RedirectToAction("JobsPending");
        }

       
        var job = await _context.TinTuyenDungs.FindAsync(id);
        if (job != null)
        {
            job.sTrangThaiTin = "Đã duyệt";
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã duyệt tin: {job.sViTriCV}";
        }
        else
        {
            TempData["Error"] = "Không tìm thấy tin tuyển dụng.";
        }

        return RedirectToAction("JobsPending");
    }

    [HttpPost]
    public async Task<IActionResult> RejectJob(string id, string ghiChu)
    {
       
        var adminRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(adminRole) || adminRole.ToLower() != "admin")
        {
            TempData["Error"] = "Bạn không có quyền thực hiện chức năng này.";
            return RedirectToAction("JobsPending");
        }

      
        var job = await _context.TinTuyenDungs.FindAsync(id);
        if (job != null)
        {
            job.sTrangThaiTin  = "Từ chối";
            job.sGhiChuTuChoi  = ghiChu;
            await _context.SaveChangesAsync();
            TempData["Error"] = $"Đã từ chối tin: {job.sViTriCV}";
        }

        return RedirectToAction("JobsPending");
    }

   
    public async Task<IActionResult> Reports()
    {
        var year = DateTime.Now.Year;

        ViewBag.JobsByMonth = await _context.TinTuyenDungs
            .Where(j => j.dNgayDang.Year == year)
            .GroupBy(j => j.dNgayDang.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .OrderBy(x => x.Month)
            .ToListAsync();

        ViewBag.AppsByMonth = await _context.UngTuyens
            .Where(u => u.dNgayUngTuyen.Year == year)
            .GroupBy(u => u.dNgayUngTuyen.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .OrderBy(x => x.Month)
            .ToListAsync();

        ViewBag.TotalJobs    = await _context.TinTuyenDungs.CountAsync();
        ViewBag.ApprovedJobs = await _context.TinTuyenDungs.CountAsync(j => j.sTrangThaiTin == "Đã duyệt");
        ViewBag.RejectedJobs = await _context.TinTuyenDungs.CountAsync(j => j.sTrangThaiTin == "Từ chối");
        ViewBag.PendingJobs  = await _context.TinTuyenDungs.CountAsync(j => j.sTrangThaiTin == "Chờ duyệt");

        ViewBag.TopCompanies = await _context.Doanhnghieps
            .Select(dn => new {
                dn.sTenDN,
                SoTin = _context.TinTuyenDungs.Count(t => t.FK_sMaDN == dn.PK_sMaDN)
            })
            .OrderByDescending(x => x.SoTin)
            .Take(5)
            .ToListAsync();

        ViewBag.Year = year;
        return View();
    }

    public async Task<IActionResult> ExportReport()
    {
        var year = DateTime.Now.Year;

        var jobs = await _context.TinTuyenDungs
            .Where(j => j.dNgayDang.Year == year).ToListAsync();

        var apps = await _context.UngTuyens
            .Where(u => u.dNgayUngTuyen.Year == year).ToListAsync();

        var topCompanies = await _context.Doanhnghieps
            .Select(dn => new {
                dn.sTenDN,
                SoTin = _context.TinTuyenDungs.Count(t => t.FK_sMaDN == dn.PK_sMaDN)
            })
            .OrderByDescending(x => x.SoTin)
            .Take(10)
            .ToListAsync();

        using var wb = new XLWorkbook();

        var ws1 = wb.Worksheets.Add("Tin tuyển dụng");
        ws1.Cell(1, 1).Value = $"BÁO CÁO TIN TUYỂN DỤNG NĂM {year}";
        ws1.Range(1, 1, 1, 7).Merge().Style
            .Font.SetBold(true).Font.SetFontSize(14)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        string[] h1 = { "Mã tin", "Vị trí", "Địa điểm", "Mức lương", "Số lượng", "Trạng thái", "Ngày đăng" };
        for (int i = 0; i < h1.Length; i++)
            ws1.Cell(2, i + 1).Value = h1[i];
        ws1.Row(2).Style.Font.SetBold(true)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#2563eb"))
            .Font.SetFontColor(XLColor.White);
        int row1 = 3;
        foreach (var j in jobs)
        {
            ws1.Cell(row1, 1).Value = j.PK_sMaTin;
            ws1.Cell(row1, 2).Value = j.sViTriCV;
            ws1.Cell(row1, 3).Value = j.sDiaDiem;
            ws1.Cell(row1, 4).Value = j.fMucLuong > 0 ? j.fMucLuong.ToString("N0") + " đ" : "Thỏa thuận";
            ws1.Cell(row1, 5).Value = j.iSoLuong ?? 0;
            ws1.Cell(row1, 6).Value = j.sTrangThaiTin;
            ws1.Cell(row1, 7).Value = j.dNgayDang.ToString("dd/MM/yyyy");
            if (row1 % 2 == 0)
                ws1.Row(row1).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#f9fafb"));
            row1++;
        }
        ws1.Columns().AdjustToContents();

        var ws2 = wb.Worksheets.Add("Đơn ứng tuyển");
        ws2.Cell(1, 1).Value = $"BÁO CÁO ĐƠN ỨNG TUYỂN NĂM {year}";
        ws2.Range(1, 1, 1, 4).Merge().Style
            .Font.SetBold(true).Font.SetFontSize(14)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        string[] h2 = { "Mã đơn", "Mã tin", "Ngày ứng tuyển", "Trạng thái" };
        for (int i = 0; i < h2.Length; i++)
            ws2.Cell(2, i + 1).Value = h2[i];
        ws2.Row(2).Style.Font.SetBold(true)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#7c3aed"))
            .Font.SetFontColor(XLColor.White);
        int row2 = 3;
        foreach (var a in apps)
        {
            ws2.Cell(row2, 1).Value = a.PK_sMaUngTuyen;
            ws2.Cell(row2, 2).Value = a.FK_sMaTin;
            ws2.Cell(row2, 3).Value = a.dNgayUngTuyen.ToString("dd/MM/yyyy");
            ws2.Cell(row2, 4).Value = a.sTrangThaiUngTuyen;
            if (row2 % 2 == 0)
                ws2.Row(row2).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#f9fafb"));
            row2++;
        }
        ws2.Columns().AdjustToContents();

        var ws3 = wb.Worksheets.Add("Top doanh nghiệp");
        ws3.Cell(1, 1).Value = "TOP DOANH NGHIỆP ĐĂNG TIN NHIỀU NHẤT";
        ws3.Range(1, 1, 1, 2).Merge().Style
            .Font.SetBold(true).Font.SetFontSize(14)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws3.Cell(2, 1).Value = "Tên doanh nghiệp";
        ws3.Cell(2, 2).Value = "Số tin tuyển dụng";
        ws3.Row(2).Style.Font.SetBold(true)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#0f766e"))
            .Font.SetFontColor(XLColor.White);
        int row3 = 3;
        foreach (var c in topCompanies)
        {
            ws3.Cell(row3, 1).Value = c.sTenDN;
            ws3.Cell(row3, 2).Value = c.SoTin;
            if (row3 % 2 == 0)
                ws3.Row(row3).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#f9fafb"));
            row3++;
        }
        ws3.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;

        string fileName = $"BaoCao_WorkGate_{year}_{DateTime.Now:ddMMyyyy}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
