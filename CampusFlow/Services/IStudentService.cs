using CampusFlow.Data;
using CampusFlow.DTO;
using CampusFlow.Model;
using System.Reflection.Metadata.Ecma335;
namespace CampusFlow.Services
{
    public interface IStudentService
    {
        public Task<Student> registerStudent(StudentDto dto);

        public Task<Student> loginStudent(LoginDto dto);
    }
}
