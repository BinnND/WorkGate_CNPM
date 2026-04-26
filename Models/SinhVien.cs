using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SourceCode.Models
{
    [Table("tblSinhVien")] 
    public class SinhVien
    {
        [Key]
        public string PK_sMaSV { get; set; } 

        public string FK_sUserID { get; set; } 

        public string sHoTen { get; set; } 

        public string sSDT { get; set; }

        public string sLop { get; set; } 

        public string sNganhHoc { get; set; }

        public string sKhoaHoc { get; set; } 

        public string sTinhTrangViecLam { get; set; } 
    }
}