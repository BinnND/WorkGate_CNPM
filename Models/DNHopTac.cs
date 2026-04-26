using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SourceCode.Models
{
    [Table("tblDNHopTac")]
    public class DNHopTac
    {
        [Key]
        public string PK_sMaSoThue { get; set; } 
        public string sTenDN { get; set; }
        public string sLinhVucKD { get; set; } 
        public string sTrangThaiHT { get; set; } = "Đang hợp tác";
    }
}
