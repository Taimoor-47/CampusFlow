using Microsoft.AspNetCore.Http;

namespace CampusFlow.Services
{
    public class FileStorageService : IFileStorageService
    {
        // Whitelist of accepted upload types. Reject everything else.
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx",
            ".txt", ".zip", ".png", ".jpg", ".jpeg"
        };

        private const long MaxBytes = 25 * 1024 * 1024; // 25 MB

        // Absolute path to the wwwroot folder where files are written.
        private readonly string _webRoot;

        public FileStorageService(IWebHostEnvironment env)
        {
            // WebRootPath is null until wwwroot exists, so derive it from the content root.
            _webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        }

        public async Task<string> SaveAsync(IFormFile file, string subFolder)
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("No file was uploaded.");

            if (file.Length > MaxBytes)
                throw new ArgumentException("File exceeds the 25 MB limit.");

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
                throw new ArgumentException($"File type '{extension}' is not allowed.");

            var folder = Path.Combine(_webRoot, subFolder);
            Directory.CreateDirectory(folder);

            // Random name avoids collisions and stops callers from controlling the path.
            var storedName = $"{Guid.NewGuid()}{extension.ToLowerInvariant()}";
            var fullPath = Path.Combine(folder, storedName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return a forward-slash URL regardless of OS path separators.
            return $"/{subFolder}/{storedName}".Replace("\\", "/");
        }
    }
}
