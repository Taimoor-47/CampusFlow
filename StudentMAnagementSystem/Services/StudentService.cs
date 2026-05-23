using StudentMAnagementSystem.Data;
using StudentMAnagementSystem.Model;
using StudentMAnagementSystem.DTO;
using StudentMAnagementSystem.Repositories;
using Azure;
namespace StudentMAnagementSystem.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repository;
        private readonly JwtServicescs _jwtService;
        public StudentService(IStudentRepository repository, JwtServicescs jwtService)
        {

            _repository = repository;
            _jwtService = jwtService;
        }
        public async Task<Student> registerStudent(StudentDto dto)
        {

            var student = new Student
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Age = dto.Age,
                isActice = dto.isActice,
                password = dto.password


            };
            return await _repository.RegisterStudent(student);
        }

        public async Task<Student> loginStudent(LoginDto dto)
        {
            var student = await _repository.GetByEmail(dto.Email);
            if (student == null || student.password != dto.password)
            {
                return null;
            }


            return student;
        }
    }
}
