using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SourceCode.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblBaoCao",
                columns: table => new
                {
                    PK_sMaBaoCao = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    sTenBC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sLoaiBC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dNgayLap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    sNguoiLap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    iTongTin = table.Column<int>(type: "int", nullable: false),
                    iTongHoSo = table.Column<int>(type: "int", nullable: false),
                    iSoSVCoViec = table.Column<int>(type: "int", nullable: false),
                    fTiLeCoViec = table.Column<double>(type: "float", nullable: false),
                    sFileBC = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblBaoCao", x => x.PK_sMaBaoCao);
                });

            migrationBuilder.CreateTable(
                name: "tblDNHopTac",
                columns: table => new
                {
                    PK_sMaSoThue = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    sTenDN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sLinhVucKD = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sTrangThaiHT = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblDNHopTac", x => x.PK_sMaSoThue);
                });

            migrationBuilder.CreateTable(
                name: "tblDoanhNghiep",
                columns: table => new
                {
                    PK_sMaDN = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FK_sUserID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FK_sMaSoThue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sTenDN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sDiaChi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sNguoiDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sGiayPhepKD = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sTrangThaiDuyet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dNgayKichHoat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblDoanhNghiep", x => x.PK_sMaDN);
                });

            migrationBuilder.CreateTable(
                name: "tblHoSoSinhVien",
                columns: table => new
                {
                    PK_sMaHoSo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FK_sMaSV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sTrinhDoHocVan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sKyNang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tKinhNghiem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tThongTinKhac = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sFileCV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sTrangThaiHoSo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblHoSoSinhVien", x => x.PK_sMaHoSo);
                });

            migrationBuilder.CreateTable(
                name: "tblSinhVien",
                columns: table => new
                {
                    PK_sMaSV = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FK_sUserID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sHoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sSDT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sLop = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sNganhHoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sKhoaHoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sTinhTrangViecLam = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblSinhVien", x => x.PK_sMaSV);
                });

            migrationBuilder.CreateTable(
                name: "tblTinTuyenDung",
                columns: table => new
                {
                    PK_sMaTin = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FK_sMaDN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sViTriCV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tMoTaCV = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sYeuCauChuyenMon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    iSoLuong = table.Column<int>(type: "int", nullable: false),
                    fMucLuong = table.Column<double>(type: "float", nullable: false),
                    sDiaDiem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dHanNop = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dNgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    sTrangThaiTin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sGhiChuTuChoi = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblTinTuyenDung", x => x.PK_sMaTin);
                });

            migrationBuilder.CreateTable(
                name: "tblUser",
                columns: table => new
                {
                    PK_sUserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    sHoten = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sMatKhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sVaiTro = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sTrangThaiTK = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sSDT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dNgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblUser", x => x.PK_sUserID);
                });

            migrationBuilder.CreateTable(
                name: "tblUngTuyen",
                columns: table => new
                {
                    PK_sMaUngTuyen = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FK_sMaTin = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FK_sMaHoSo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    dNgayUngTuyen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    sTrangThaiUngTuyen = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblUngTuyen", x => x.PK_sMaUngTuyen);
                    table.ForeignKey(
                        name: "FK_tblUngTuyen_tblHoSoSinhVien_FK_sMaHoSo",
                        column: x => x.FK_sMaHoSo,
                        principalTable: "tblHoSoSinhVien",
                        principalColumn: "PK_sMaHoSo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblUngTuyen_tblTinTuyenDung_FK_sMaTin",
                        column: x => x.FK_sMaTin,
                        principalTable: "tblTinTuyenDung",
                        principalColumn: "PK_sMaTin",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblUngTuyen_FK_sMaHoSo",
                table: "tblUngTuyen",
                column: "FK_sMaHoSo");

            migrationBuilder.CreateIndex(
                name: "IX_tblUngTuyen_FK_sMaTin",
                table: "tblUngTuyen",
                column: "FK_sMaTin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblBaoCao");

            migrationBuilder.DropTable(
                name: "tblDNHopTac");

            migrationBuilder.DropTable(
                name: "tblDoanhNghiep");

            migrationBuilder.DropTable(
                name: "tblSinhVien");

            migrationBuilder.DropTable(
                name: "tblUngTuyen");

            migrationBuilder.DropTable(
                name: "tblUser");

            migrationBuilder.DropTable(
                name: "tblHoSoSinhVien");

            migrationBuilder.DropTable(
                name: "tblTinTuyenDung");
        }
    }
}
