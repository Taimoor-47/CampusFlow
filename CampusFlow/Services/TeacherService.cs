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

        public async Task<IReadOnlyList<StudentSummaryDto>> GetAllStudents()
        {
            return await _context.Students
                .AsNoTracking()
                .OrderBy(student => student.Name)
                .Select(student => new StudentSummaryDto
                {
                    Id = student.Id,
                    Name = student.Name,
                    Email = student.Email,
                    PhoneNumber = student.PhoneNumber,
                    Age = student.Age,
                    IsActive = student.IsActive
                })
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
            var section = await _context.CourseSections
                .Include(section => section.Course)
                .FirstOrDefaultAsync(section =>
                    section.Id == dto.CourseSectionId &&
                    section.TeacherId == teacherId &&
                    section.IsActive);

            if (section is null)
            {
                throw new UnauthorizedAccessException(
                    "You cannot create assignments for this section.");
            }

            if (dto.DueDate <= DateTime.UtcNow)
            {
                throw new ArgumentException(
                    "The assignment due date must be in the future.");
            }

            string? filePath = null;

            if (dto.File is { Length: > 0 })
            {
                filePath = await _fileStorage.SaveAsync(
                    dto.File,
                    "uploads/assignments");
            }

            var assignment = new Assignment
            {
                CourseSectionId = section.Id,
                CourseSection = section,
                TeacherId = teacherId,
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                DueDate = dto.DueDate,
                FilePath = filePath
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return assignment;
        }

        public async Task<IReadOnlyList<Submission>> GetSubmissions(Guid assignmentId, Guid teacherId)
        {
            var assignment = await _context.Assignments
                .AsNoTracking()
                .Where(a => a.Id == assignmentId)
                .Select(a => new
                {
                    a.Id,
                    SectionTeacherId = a.CourseSection.TeacherId
                })
                .SingleOrDefaultAsync();

            if (assignment is null)
            {
                throw new KeyNotFoundException(
                    "Assignment was not found.");
            }

            if (assignment.SectionTeacherId != teacherId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot view submissions for this assignment.");
            }

            return await _context.Submissions
                .AsNoTracking()
                .Where(s => s.AssignmentId == assignmentId)
                .Include(s => s.Student)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<CourseSectionOptionDto>> GetMySections(
        Guid teacherId)
        {
            return await _context.CourseSections
                .AsNoTracking()
                .Where(section =>
                    section.TeacherId == teacherId &&
                    section.IsActive)
                .OrderBy(section => section.Course.CourseCode)
                .ThenBy(section => section.SectionName)
                .Select(section => new CourseSectionOptionDto
                {
                    Id = section.Id,
                    CourseCode = section.Course.CourseCode,
                    CourseTitle = section.Course.CourseTitle,
                    SectionName = section.SectionName,
                    AcademicYear = section.AcademicYear,
                    Semester = section.Semester
                })
                .ToListAsync();
        }

    }
}

