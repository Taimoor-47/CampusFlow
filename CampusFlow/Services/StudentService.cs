using CampusFlow.Data;
using CampusFlow.Helpers;
using CampusFlow.Model;
using CampusFlow.DTO;
using CampusFlow.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampusFlow.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repository;
        private readonly AppDbContext _context;

        public StudentService(IStudentRepository repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<Student> RegisterStudent(StudentDto dto)
        {
            var student = new Student
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Age = dto.Age,
                IsActive = true,
                Password = PasswordHelper.Hash(dto.Password)
            };

            return await _repository.RegisterStudent(student);
        }

        public async Task<Student> LoginStudent(LoginDto dto)
        {
            var student = await _repository.GetByEmail(dto.Email);

            if (student == null || !PasswordHelper.Verify(dto.Password, student.Password))
                return null;

            return student;
        }

        public async Task<List<Schedules>> GetMySchedules(Guid studentId)
        {
            return await _context.Schedules
                .Where(s => s.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<List<Assignment>> GetMyAssignments(Guid studentId)
        {
            return await _context.Assignments
                .Where(a => a.StudentId == studentId)
                .ToListAsync();
        }
    }
}
