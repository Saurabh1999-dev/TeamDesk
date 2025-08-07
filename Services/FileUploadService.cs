// Services/FileUploadService.cs
using Microsoft.AspNetCore.Hosting;
using TeamDesk.Services.Interfaces;

namespace TeamDesk.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileUploadService> _logger;

        private readonly string[] _allowedImageTypes = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly string[] _allowedDocTypes = { ".pdf", ".doc", ".docx", ".txt", ".xlsx", ".xls", ".ppt", ".pptx" };

        public FileUploadService(IWebHostEnvironment environment, IConfiguration configuration, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder = "tasks")
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("No file provided");

                if (!IsValidFileType(file))
                    throw new ArgumentException("File type not allowed");

                if (!IsValidFileSize(file))
                    throw new ArgumentException("File size exceeds limit");

                // Create unique filename
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                // Return relative path for URL generation
                return Path.Combine("uploads", folder, fileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                return false;
            }
        }

        public bool IsValidFileType(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedImageTypes.Contains(extension) || _allowedDocTypes.Contains(extension);
        }

        public bool IsValidFileSize(IFormFile file, long maxSizeInMB = 10)
        {
            var maxSizeInBytes = maxSizeInMB * 1024 * 1024;
            return file.Length <= maxSizeInBytes;
        }

        public string GetFileUrl(string filePath)
        {
            var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7201";
            return $"{baseUrl}/{filePath}";
        }
    }
}
