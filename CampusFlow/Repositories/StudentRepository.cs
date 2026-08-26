using CampusFlow.Data;
using CampusFlow.Model;
using Microsoft.EntityFrameworkCore;

namespace CampusFlow.Repositories
{
    // This class is the ONLY place that touches AppDbContext.
    // Nothing above this layer (Service, Controller) should ever import AppDbContext.
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;

        public StudentRepository(AppDbContext context)
        {
            _context = context;
        }

        // ── Write ─────────────────────────────────────────────────────────────

        public async Task<Student> RegisterStudent(Student student)
        {
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
            return student;
        }

        // ── Read ──────────────────────────────────────────────────────────────

        public async Task<List<Student>> GetAllStudents()
            => await _context.Students.ToListAsync();

        public async Task<Student?> GetByEmail(string email)
            => await _context.Students
                .FirstOrDefaultAsync(s => s.Email == email);

        public async Task<Student?> GetById(Guid id)
            => await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

        // ── Student-owned data ─────────────────────────────────────────────────

        public async Task<IReadOnlyList<StudentGPA>> GetGpaByStudentId(Guid studentId)
            => await _context.StudentGPA
                .Where(g => g.StudentId == studentId)
                .OrderBy(g => g.Semester)
                .ToListAsync();

        public async Task<IReadOnlyList<Schedules>> GetSchedulesByStudentId(Guid studentId)
            => await _context.Schedules
                .Where(s => s.StudentId == studentId)
                .ToListAsync();

        // ── Assignments & submissions ──────────────────────────────────────────

        public async Task<IReadOnlyList<Assignment>> GetAllAssignments()
            => await _context.Assignments
                .OrderBy(a => a.DueDate)
                .ToListAsync();

        public async Task<Assignment?> GetAssignmentById(Guid assignmentId)
            => await _context.Assignments
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

        public async Task<IReadOnlyList<Submission>> GetSubmissionsByStudentId(Guid studentId)
            => await _context.Submissions
                .Where(s => s.StudentId == studentId)
                .ToListAsync();

        public async Task<Submission?> GetSubmission(Guid assignmentId, Guid studentId)
            => await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

        public async Task<Submission> AddSubmission(Submission submission)
        {
            await _context.Submissions.AddAsync(submission);
            await _context.SaveChangesAsync();
            return submission;
        }
        public async Task<IReadOnlyList<Assignment>> GetAssignmentsForStudent(Guid studentId)
        {
            return await _context.Assignments
                .AsNoTracking()
                .Where(assignment =>
                    assignment.CourseSection.IsActive &&
                    assignment.CourseSection.Enrollments.Any(enrollment =>
                        enrollment.StudentId == studentId &&
                        enrollment.IsActive))
                .Include(assignment => assignment.CourseSection)
                    .ThenInclude(section => section.Course)
                .OrderBy(assignment => assignment.DueDate)
                .ToListAsync();
        }

        public async Task<Assignment?> GetAccessibleAssignment(
        Guid assignmentId,
        Guid studentId)
        {
            return await _context.Assignments
                .AsNoTracking()
                .Include(assignment => assignment.CourseSection)
                    .ThenInclude(section => section.Course)
                .FirstOrDefaultAsync(assignment =>
                    assignment.Id == assignmentId &&
                    assignment.CourseSection.IsActive &&
                    assignment.CourseSection.Enrollments.Any(enrollment =>
                        enrollment.StudentId == studentId &&
                        enrollment.IsActive));
        }
    }
}
