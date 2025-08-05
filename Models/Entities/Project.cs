using System.ComponentModel.DataAnnotations;
using TeamDesk.Enum;

namespace TeamDesk.DTOs
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        public string ProjectName { get; set; }

        [Required]
        public string ClientName { get; set; }

        public string LanguageUsed { get; set; } // e.g. "React, Node.js"

        public DateTime? EndDate { get; set; }

        public bool IsBillable { get; set; }
    }
}
