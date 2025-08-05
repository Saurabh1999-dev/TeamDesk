using TeamDesk.Enum;

namespace TeamDesk.Models.Entities
{
    public class Staff
    {
        public int StaffId { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
