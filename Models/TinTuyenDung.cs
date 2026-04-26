using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SourceCode.Models
{
    [Table("tblTinTuyenDung")] 
    public class TinTuyenDung
    {
        [Key]
        public string PK_sMaTin { get; set; }
        public string FK_sMaDN { get; set; }
        public string sViTriCV { get; set; }
        public string tMoTaCV { get; set; }
        public string sYeuCauChuyenMon { get; set; }
        public int? iSoLuong { get; set; }
        public double fMucLuong { get; set; }
        public string sDiaDiem { get; set; }
        public DateTime dHanNop { get; set; }
        public DateTime dNgayDang { get; set; } = DateTime.Now;
        public string sTrangThaiTin { get; set; } = "Chờ duyệt";
        public string? sGhiChuTuChoi { get; set; }
    }
}