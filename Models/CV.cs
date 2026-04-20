using SourceCode.Models;
using System;
using System.ComponentModel.DataAnnotations;
namespace SourceCode.Models
{
    public class CV
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string Education { get; set; }
        public string Skills { get; set; }
        public string Experience { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}