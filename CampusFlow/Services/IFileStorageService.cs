using Microsoft.AspNetCore.Http;

namespace CampusFlow.Services
{
    // Abstracts where uploaded files physically live. Today: local disk under wwwroot.
    // Swap the implementation (e.g. Azure Blob / S3) without touching callers.
    public interface IFileStorageService
    {
        // Saves the uploaded file under wwwroot/{subFolder} and returns its public URL
        // (e.g. "/uploads/assignments/{guid}.pdf"). Throws ArgumentException on an
        // empty file, an oversized file, or a disallowed extension.
        Task<string> SaveAsync(IFormFile file, string subFolder);
    }
}
