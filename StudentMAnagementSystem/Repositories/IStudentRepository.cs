using StudentMAnagementSystem.DTO;
using StudentMAnagementSystem.Model;
namespace StudentMAnagementSystem.Repositories
{
    public interface IStudentRepository
    {
        public Task<List<Student>> getAllstudents();
        public Task<Student> getStudentById(string id);

        public Task<Student> RegisterStudent(Student student);

        public Task<Student?> GetByEmail(string email);


    }
}
