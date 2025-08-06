﻿namespace TeamDesk.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Role { get; set; } = 0;
        public string FullName => $"{FirstName} {LastName}";
    }
}
