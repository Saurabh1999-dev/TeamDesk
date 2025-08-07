namespace TeamDesk.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder = "tasks");
        Task<bool> DeleteFileAsync(string filePath);
        bool IsValidFileType(IFormFile file);
        bool IsValidFileSize(IFormFile file, long maxSizeInMB = 10);
        string GetFileUrl(string filePath);
    }
}
