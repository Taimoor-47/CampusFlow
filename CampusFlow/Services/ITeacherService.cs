using CampusFlow.DTO;
using CampusFlow.Model;

namespace CampusFlow.Services
{
    public interface ITeacherService
    {
        Task<Teacher> Register(RegisterTeacherDto dto);
        Task<Teacher> Login(LoginDto dto);
        Task<List<Student>> GetAllStudents();
        Task<StudentGPA> AddGpa(AddGpaDto dto);
        Task<Schedules> AddSchedule(AddScheduleDto dto);
        Task<Assignment> AddAssignment(AddAssignmentDto dto);
    }
}
