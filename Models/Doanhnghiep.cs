using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SourceCode.Models
{
    [Table("tblDoanhNghiep")]
    public class Doanhnghiep
    {
        [Key]
        public string PK_sMaDN { get; set; } 
        public string FK_sUserID { get; set; }
        public string FK_sMaSoThue { get; set; } 
        public string sTenDN { get; set; }
        public string sDiaChi { get; set; }
        public string sNguoiDaiDien { get; set; }
        public string sGiayPhepKD { get; set; }  
        public string sTrangThaiDuyet { get; set; } = "Chờ duyệt";
        public DateTime? dNgayKichHoat { get; set; } 
    }
}
