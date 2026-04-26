using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SourceCode.Models
{
    [Table("tblBaoCao")]
    public class BaoCao
    {
        [Key]
        public string PK_sMaBaoCao { get; set; }

        public string sTenBC { get; set; }

        public string sLoaiBC { get; set; }

        public DateTime dNgayLap { get; set; } = DateTime.Now;

        public string sNguoiLap { get; set; }

        public int iTongTin { get; set; } 

        public int iTongHoSo { get; set; } 

        public int iSoSVCoViec { get; set; }

        public double fTiLeCoViec { get; set; } 

        public string sFileBC { get; set; }
    }
}