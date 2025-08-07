namespace TeamDesk.DTOs.Response
{
    public class TaskAttachmentResponse
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UploadAttachmentRequest
    {
        public Guid TaskId { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}
