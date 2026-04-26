using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SourceCode.Models 
{
    [Table("tblUser")]
    public class User
    {
        [Key]
        public string PK_sUserID { get; set; } 
        public string sHoten { get; set; }
        public string sEmail { get; set; }
        public string sMatKhau { get; set; } 
        public string sVaiTro { get; set; }  
        public string sTrangThaiTK { get; set; } 
        public string sSDT { get; set; }
        public DateTime dNgayTao { get; set; } = DateTime.Now;
    }
}