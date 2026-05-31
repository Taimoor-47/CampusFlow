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
        private readonly IFileStorageService _fileStorage;

        public TeacherService(AppDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
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
                .Include(s => s.Submissions)
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

        public async Task<Assignment> AddAssignment(AddAssignmentDto dto, Guid? teacherId)
        {
            // Save the brief file first (if any) so a storage failure aborts before we
            // write a half-formed row.
            string? filePath = null;
            if (dto.File is not null && dto.File.Length > 0)
                filePath = await _fileStorage.SaveAsync(dto.File, "uploads/assignments");

            var assignment = new Assignment
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                FilePath = filePath,
                TeacherId = teacherId
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        public async Task<IReadOnlyList<Submission>> GetSubmissions(Guid assignmentId)
        {
            return await _context.Submissions
                .Where(s => s.AssignmentId == assignmentId)
                .Include(s => s.Student)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }
    }
}
