using Microsoft.EntityFrameworkCore;
using SourceCode.Models;

namespace SourceCode.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Đăng ký toàn bộ các bảng vào DbContext
        public DbSet<User> Users { get; set; }
        public DbSet<SinhVien> SinhViens { get; set; }
        public DbSet<Doanhnghiep> Doanhnghieps { get; set; }
        public DbSet<DNHopTac> DNHopTacs { get; set; }
        public DbSet<TinTuyenDung> TinTuyenDungs { get; set; } 
        public DbSet<HoSoSinhVien> HoSoSinhViens { get; set; }
        public DbSet<UngTuyen> UngTuyens { get; set; }
        public DbSet<BaoCao> BaoCaos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình bảng tblUser
            modelBuilder.Entity<User>(entity => {
                entity.ToTable("tblUser");
                entity.HasKey(e => e.PK_sUserID);
            });

            // 2. Cấu hình bảng tblSinhVien
            modelBuilder.Entity<SinhVien>(entity => {
                entity.ToTable("tblSinhVien");
                entity.HasKey(e => e.PK_sMaSV);
            });

            // 3. Cấu hình bảng tblDoanhNghiep
            modelBuilder.Entity<Doanhnghiep>(entity => {
                entity.ToTable("tblDoanhNghiep");
                entity.HasKey(e => e.PK_sMaDN);
            });

            // 4. Cấu hình bảng tblDNHopTac
            modelBuilder.Entity<DNHopTac>(entity => {
                entity.ToTable("tblDNHopTac");
                entity.HasKey(e => e.PK_sMaSoThue);
            });

            // 5. Cấu hình bảng tblTinTuyenDung (Thay cho bảng Job/Applications cũ)
            modelBuilder.Entity<TinTuyenDung>(entity => {
                entity.ToTable("tblTinTuyenDung");
                entity.HasKey(e => e.PK_sMaTin);
            });

            // 6. Cấu hình bảng tblHoSoSinhVien
            modelBuilder.Entity<HoSoSinhVien>(entity => {
                entity.ToTable("tblHoSoSinhVien");
                entity.HasKey(e => e.PK_sMaHoSo);
            });

            // 7. Cấu hình bảng tblUngTuyen
            modelBuilder.Entity<UngTuyen>(entity => {
                entity.ToTable("tblUngTuyen");
                entity.HasKey(e => e.PK_sMaUngTuyen);
            });

            // 8. Cấu hình bảng tblBaoCao
            modelBuilder.Entity<BaoCao>(entity => {
                entity.ToTable("tblBaoCao");
                entity.HasKey(e => e.PK_sMaBaoCao);
            });
        }
    }
}