using TeamDesk.Enum;

namespace TeamDesk.DTOs
{
    public class AuthResponse
    {
        public int StaffId { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public UserRole Role { get; set; } = UserRole.Staff;
        //public required string Token { get; set; }
    }

}
