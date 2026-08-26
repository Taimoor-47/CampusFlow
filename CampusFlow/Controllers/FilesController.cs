using CampusFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusFlow.Controllers
{
    // Authorized file downloads. Uploaded files are NOT served as public static
    // files anymore; every request is checked against the caller's JWT identity.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private static readonly IReadOnlyDictionary<string, string> ContentTypesByExtension =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = "application/pdf",
                [".doc"] = "application/msword",
                [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                [".ppt"] = "application/vnd.ms-powerpoint",
                [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                [".xls"] = "application/vnd.ms-excel",
                [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                [".txt"] = "text/plain",
                [".zip"] = "application/zip",
                [".png"] = "image/png",
                [".jpg"] = "image/jpeg",
                [".jpeg"] = "image/jpeg"
            };

        private readonly IFileDownloadService _downloads;
        private readonly IWebHostEnvironment _environment;

        public FilesController(IFileDownloadService downloads, IWebHostEnvironment environment)
        {
            _downloads = downloads;
            _environment = environment;
        }

        // GET /api/files/assignments/{fileName}
        [HttpGet("assignments/{fileName}")]
        public async Task<IActionResult> AssignmentBrief(string fileName)
        {
            if (!TryGetCaller(out var userId, out var role))
                return Unauthorized();

            var assignment = await _downloads.GetAccessibleAssignmentFileAsync(
                userId,
                role,
                SafeFileName(fileName));

            if (assignment is null)
                return NotFound();

            return PhysicalUploadFile(assignment.FilePath!, "assignments", "assignment-brief");
        }

        // GET /api/files/submissions/{fileName}
        [HttpGet("submissions/{fileName}")]
        public async Task<IActionResult> SubmissionFile(string fileName)
        {
            if (!TryGetCaller(out var userId, out var role))
                return Unauthorized();

            var submission = await _downloads.GetAccessibleSubmissionAsync(
                userId,
                role,
                SafeFileName(fileName));

            if (submission is null)
                return NotFound();

            return PhysicalUploadFile(submission.FilePath!, "submissions", "submission");
        }

        private bool TryGetCaller(out Guid userId, out string role)
        {
            role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var rawId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(rawId, out var parsed))
            {
                userId = parsed;
                return true;
            }

            userId = default;
            return false;
        }

        // Strips any path tricks; only a bare file name may reach the service.
        private static string SafeFileName(string fileName) =>
            Path.GetFileName(fileName);

        private IActionResult PhysicalUploadFile(string storedPath, string folder, string downloadBaseName)
        {
            var extension = Path.GetExtension(storedPath);
            if (!ContentTypesByExtension.TryGetValue(extension, out var contentType))
                return NotFound();

            var webRoot = _environment.WebRootPath
                          ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var fullPath = Path.GetFullPath(Path.Combine(webRoot, storedPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

            // Defense in depth: the resolved path must stay inside the uploads folder.
            var uploadsRoot = Path.GetFullPath(Path.Combine(webRoot, "uploads"));
            if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
                return NotFound();

            return PhysicalFile(fullPath, contentType, $"{downloadBaseName}{extension.ToLowerInvariant()}");
        }
    }
}
