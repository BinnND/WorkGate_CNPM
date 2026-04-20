using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SourceCode.Models
{
    public class Application
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int JobId { get; set; }

        public int CVId { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime ApplyDate { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual User Student { get; set; } 

        [ForeignKey("JobId")]
        public virtual Job Job { get; set; }
    }
}