using CampusFlow.DTO;
using CampusFlow.Model;

namespace CampusFlow.Services
{
    public interface ITeacherService
    {
        Task<Teacher> Register(RegisterTeacherDto dto);
        Task<Teacher> Login(LoginDto dto);
        Task<IReadOnlyList<StudentSummaryDto>> GetAllStudents();
        Task<StudentGPA> AddGpa(AddGpaDto dto);
        Task<Schedules> AddSchedule(AddScheduleDto dto);

        // Creates an assignment for the class, optionally saving an attached brief file.
        Task<Assignment> AddAssignment(AddAssignmentDto dto, Guid? teacherId);
        Task<IReadOnlyList<CourseSectionOptionDto>> GetMySections(Guid teacherId);
        Task<IReadOnlyList<Submission>> GetSubmissions(Guid assignmentId, Guid teacherId);
    }
}
