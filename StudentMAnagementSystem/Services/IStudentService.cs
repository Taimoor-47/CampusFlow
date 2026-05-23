using StudentMAnagementSystem.Data;
using StudentMAnagementSystem.DTO;
using StudentMAnagementSystem.Model;
using System.Reflection.Metadata.Ecma335;
namespace StudentMAnagementSystem.Services
{
    public interface IStudentService
    {
        public Task<Student> registerStudent(StudentDto dto);

        public Task<Student> loginStudent(LoginDto dto);
    }
}
