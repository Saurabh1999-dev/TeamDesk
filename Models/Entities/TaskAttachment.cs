using System.ComponentModel.DataAnnotations;

namespace TeamDesk.Models.Entities
{
    public class TaskAttachment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TaskId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FileType { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public Guid UploadedById { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Task Task { get; set; } = null!;
        public User UploadedBy { get; set; } = null!;
    }
}
