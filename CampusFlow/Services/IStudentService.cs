using CampusFlow.DTO;
using CampusFlow.Model;

namespace CampusFlow.Services
{
    public interface IStudentService
    {
        Task<Student> RegisterStudent(StudentDto dto);
        Task<Student> LoginStudent(LoginDto dto);
        Task<List<Schedules>> GetMySchedules(Guid studentId);
        Task<List<Assignment>> GetMyAssignments(Guid studentId);
    }
}
