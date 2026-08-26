using CampusFlow.Data;
using CampusFlow.Model;
using Microsoft.EntityFrameworkCore;

namespace CampusFlow.Services
{
    public interface IFileDownloadService
    {
        // Returns the assignment only when userId/role may download its brief;
        // null otherwise (null also covers "file does not exist" so both cases
        // are indistinguishable to callers).
        Task<Assignment?> GetAccessibleAssignmentFileAsync(Guid userId, string role, string fileName);

        Task<Submission?> GetAccessibleSubmissionAsync(Guid userId, string role, string fileName);
    }
}
