using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SourceCode.Models
{
    [Table("tblHoSoSinhVien")]
    public class HoSoSinhVien
    {
        [Key]
        public string PK_sMaHoSo { get; set; } 
        public string FK_sMaSV { get; set; }
        public string sTrinhDoHocVan { get; set; }
        public string sKyNang { get; set; }
        public string tKinhNghiem { get; set; } 
        public string tThongTinKhac { get; set; } 
        public string sFileCV { get; set; }
        public string sTrangThaiHoSo { get; set; } = "Riêng tư";
        public DateTime dNgayTao { get; set; } = DateTime.Now;
    }
}
