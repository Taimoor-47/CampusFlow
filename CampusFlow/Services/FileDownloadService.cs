using CampusFlow.Data;
using CampusFlow.Model;
using Microsoft.EntityFrameworkCore;

namespace CampusFlow.Services
{
    // Authorization for uploaded files, which are no longer served publicly.
    // Assignment briefs: the owning section's teacher or actively enrolled students.
    // Submissions: only the submitting student or the section's teacher.
    public class FileDownloadService : IFileDownloadService
    {
        private readonly AppDbContext _context;

        public FileDownloadService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Assignment?> GetAccessibleAssignmentFileAsync(
            Guid userId,
            string role,
            string fileName)
        {
            var path = StoredPath("assignments", fileName);

            var assignment = await _context.Assignments
                .AsNoTracking()
                .Include(a => a.CourseSection)
                .Where(a => a.FilePath == path && a.CourseSection.IsActive)
                .SingleOrDefaultAsync();

            if (assignment is null)
                return null;

            var allowed = role switch
            {
                "Teacher" => assignment.CourseSection.TeacherId == userId,
                "Student" => await _context.CourseEnrollments.AnyAsync(enrollment =>
                    enrollment.StudentId == userId &&
                    enrollment.CourseSectionId == assignment.CourseSectionId &&
                    enrollment.IsActive),
                _ => false
            };

            return allowed ? assignment : null;
        }

        public async Task<Submission?> GetAccessibleSubmissionAsync(
            Guid userId,
            string role,
            string fileName)
        {
            var path = StoredPath("submissions", fileName);

            var submission = await _context.Submissions
                .AsNoTracking()
                .Include(s => s.Assignment)
                    .ThenInclude(a => a!.CourseSection)
                .Where(s => s.FilePath == path)
                .SingleOrDefaultAsync();

            if (submission is null || submission.Assignment is null)
                return null;

            var allowed = role switch
            {
                "Teacher" => submission.Assignment.CourseSection.TeacherId == userId,
                "Student" => submission.StudentId == userId,
                _ => false
            };

            return allowed ? submission : null;
        }

        private static string StoredPath(string folder, string fileName) =>
            $"/uploads/{folder}/{fileName}";
    }
}
