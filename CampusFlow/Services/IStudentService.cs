using CampusFlow.DTO;
using CampusFlow.Model;
using Microsoft.AspNetCore.Http;

namespace CampusFlow.Services
{
    public interface IStudentService
    {
        // ── Auth ──────────────────────────────────────────────────────────────
        Task<Student>  RegisterStudent(StudentDto dto);
        Task<Student?> LoginStudent(LoginDto dto);

        // ── Student-owned data ────────────────────────────────────────────────
        // Return domain models — no input DTOs used as output types.
        Task<IReadOnlyList<StudentGPA>>  GetGpaRecords(Guid studentId);
        Task<IReadOnlyList<Schedules>>   GetMySchedules(Guid studentId);

        // Every teacher assignment, annotated with this student's submission status.
        Task<IReadOnlyList<StudentAssignmentDto>>  GetMyAssignments(Guid studentId);

        // Student uploads a file against an assignment. Throws KeyNotFoundException if
        // the assignment doesn't exist, InvalidOperationException if already submitted,
        // or ArgumentException for a bad file.
        Task<Submission>  SubmitAssignment(Guid assignmentId, Guid studentId, IFormFile file);
    }
}
