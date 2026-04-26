using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SourceCode.Models
{
    [Table("tblUngTuyen")]
    public class UngTuyen
    {
        [Key]
        public string PK_sMaUngTuyen { get; set; }
        public string FK_sMaTin { get; set; }
        public string FK_sMaHoSo { get; set; }
        public DateTime dNgayUngTuyen { get; set; }
        public string sTrangThaiUngTuyen { get; set; }

        [ForeignKey("FK_sMaTin")]
        public virtual TinTuyenDung TinTuyenDung { get; set; }

        [ForeignKey("FK_sMaHoSo")]
        public virtual HoSoSinhVien HoSoSinhVien { get; set; }
    }
}
