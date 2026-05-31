using CampusFlow.Model;

namespace CampusFlow.Repositories
{
    public interface IStudentRepository
    {
        // ── Write ────────────────────────────────────────────────────────────
        Task<Student>  RegisterStudent(Student student);

        // ── Read ─────────────────────────────────────────────────────────────
        Task<List<Student>>  GetAllStudents();
        Task<Student?>       GetByEmail(string email);
        Task<Student?>       GetById(Guid id);

        // ── Student-owned data ────────────────────────────────────────────────
        Task<IReadOnlyList<StudentGPA>>  GetGpaByStudentId(Guid studentId);
        Task<IReadOnlyList<Schedules>>   GetSchedulesByStudentId(Guid studentId);

        // ── Assignments & submissions ─────────────────────────────────────────
        Task<IReadOnlyList<Assignment>>   GetAllAssignments();
        Task<Assignment?>                 GetAssignmentById(Guid assignmentId);
        Task<IReadOnlyList<Submission>>   GetSubmissionsByStudentId(Guid studentId);
        Task<Submission?>                 GetSubmission(Guid assignmentId, Guid studentId);
        Task<Submission>                  AddSubmission(Submission submission);
    }
}
