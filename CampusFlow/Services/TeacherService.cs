using CampusFlow.Data;
using CampusFlow.DTO;
using CampusFlow.Helpers;
using CampusFlow.Model;
using Microsoft.EntityFrameworkCore;

namespace CampusFlow.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly AppDbContext _context;

        public TeacherService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Teacher> Register(RegisterTeacherDto dto)
        {
            var teacher = new Teacher
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = PasswordHelper.Hash(dto.Password)
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
            return teacher;
        }

        public async Task<Teacher> Login(LoginDto dto)
        {
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Email == dto.Email);

            if (teacher == null || !PasswordHelper.Verify(dto.Password, teacher.Password))
                return null;

            return teacher;
        }

        public async Task<List<Student>> GetAllStudents()
        {
            return await _context.Students
                .Include(s => s.StudentGPA)
                .Include(s => s.Schedules)
                .Include(s => s.Assignments)
                .ToListAsync();
        }

        public async Task<StudentGPA> AddGpa(AddGpaDto dto)
        {
            var studentExists = await _context.Students.AnyAsync(s => s.Id == dto.StudentId);
            if (!studentExists)
                throw new KeyNotFoundException($"No student found with ID {dto.StudentId}.");

            var gpa = new StudentGPA
            {
                StudentId = dto.StudentId,
                Semester = dto.Semester,
                Gpa = dto.Gpa
            };

            _context.StudentGPA.Add(gpa);
            await _context.SaveChangesAsync();
            return gpa;
        }

        public async Task<Schedules> AddSchedule(AddScheduleDto dto)
        {
            var studentExists = await _context.Students.AnyAsync(s => s.Id == dto.StudentId);
            if (!studentExists)
                throw new KeyNotFoundException($"No student found with ID {dto.StudentId}.");

            var schedule = new Schedules
            {
                StudentId = dto.StudentId,
                CourseTitle = dto.CourseTitle,
                Room = dto.Room,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task<Assignment> AddAssignment(AddAssignmentDto dto)
        {
            var studentExists = await _context.Students.AnyAsync(s => s.Id == dto.StudentId);
            if (!studentExists)
                throw new KeyNotFoundException($"No student found with ID {dto.StudentId}.");

            var assignment = new Assignment
            {
                StudentId = dto.StudentId,
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }
    }
}
