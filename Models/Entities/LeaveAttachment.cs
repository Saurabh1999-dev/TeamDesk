namespace TeamDesk.Models.Entities
{
    public class LeaveAttachment
    {
        public Guid Id { get; set; }
        public Guid LeaveId { get; set; }
        public Leave Leave { get; set; }

        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FileType { get; set; }
        public string FilePath { get; set; }
        public string FileUrl { get; set; }
        public long FileSize { get; set; }

        public Guid UploadedById { get; set; }
        public User UploadedBy { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
