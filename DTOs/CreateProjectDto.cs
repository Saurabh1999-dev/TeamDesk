using System.ComponentModel.DataAnnotations;
using TeamDesk.Enum;

namespace TeamDesk.DTOs
{
    public class CreateProjectDto
    {
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public string LanguageUsed { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsBillable { get; set; }
    }
}
