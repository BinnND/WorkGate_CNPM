using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SourceCode.Models 
{
    public class Job
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Salary { get; set; }

        public DateTime Deadline { get; set; }
        public int CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual User Company { get; set; }

        public string Status { get; set; }
        public string Location { get; set; }
        public string Requirements { get; set; }
        public DateTime PostedDate { get; set; } = DateTime.Now;
    }
}