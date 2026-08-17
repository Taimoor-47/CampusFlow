using CampusFlow.DTO;
using CampusFlow.Helpers;
using CampusFlow.Model;
using CampusFlow.Repositories;
using Microsoft.AspNetCore.Http;

namespace CampusFlow.Services
{
    // The Service layer holds business rules only.
    // It NEVER imports AppDbContext — all data access goes through IStudentRepository.
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repository;
        private readonly IFileStorageService _fileStorage;

        public StudentService(IStudentRepository repository, IFileStorageService fileStorage)
        {
            _repository = repository;
            _fileStorage = fileStorage;
        }

        // ── Auth ──────────────────────────────────────────────────────────────

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

        public async Task<Student?> LoginStudent(LoginDto dto)
        {
            var student = await _repository.GetByEmail(dto.Email);

            // Business rule: both conditions must be true to grant access.
            if (student is null || !PasswordHelper.Verify(dto.Password, student.Password))
                return null;

            return student;
        }

        // ── Student-owned data ─────────────────────────────────────────────────
        // Pure delegation — no extra logic needed here yet.
        // If you ever add rules (e.g. "only return GPA if student is active"),
        // this is the place to add them without touching the Repository or Controller.

        public async Task<IReadOnlyList<StudentGPA>> GetGpaRecords(Guid studentId)
            => await _repository.GetGpaByStudentId(studentId);

        public async Task<IReadOnlyList<Schedules>> GetMySchedules(Guid studentId)
            => await _repository.GetSchedulesByStudentId(studentId);

        public async Task<IReadOnlyList<StudentAssignmentDto>>
     GetMyAssignments(Guid studentId)
        {
            var assignments =
                await _repository.GetAssignmentsforStudent(studentId);

            var submissions =
                await _repository.GetSubmissionsByStudentId(studentId);

            var submissionByAssignment = submissions.ToDictionary(
                submission => submission.AssignmentId);

            return assignments.Select(assignment =>
            {
                submissionByAssignment.TryGetValue(
                    assignment.Id,
                    out var submission);

                return new StudentAssignmentDto
                {
                    Id = assignment.Id,
                    CourseSectionId = assignment.CourseSectionId,
                    CourseCode =
                        assignment.CourseSection.Course.CourseCode,
                    CourseTitle =
                        assignment.CourseSection.Course.CourseTitle,
                    SectionName =
                        assignment.CourseSection.SectionName,

                    Title = assignment.Title,
                    Description = assignment.Description,
                    DueDate = assignment.DueDate,
                    FilePath = assignment.FilePath,

                    Submitted = submission is not null,
                    SubmissionFilePath = submission?.FilePath,
                    SubmittedAt = submission?.SubmittedAt
                };
            }).ToList();
        }

        public async Task<Submission> SubmitAssignment(Guid assignmentId, Guid studentId, IFormFile file)
        {
            var assignment = await _repository.GetAccessibleAssignment(
                assignmentId,
                studentId);

            if (assignment is null)
            {
                // Use Not Found instead of revealing that an inaccessible
                // assignment exists.
                throw new KeyNotFoundException(
                    "Assignment was not found.");
            }

            if (assignment.DueDate < DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "The assignment deadline has passed.");
            }

            var existing = await _repository.GetSubmission(
                assignmentId,
                studentId);

            if (existing is not null)
            {
                throw new InvalidOperationException(
                    "You have already submitted this assignment.");
            }

            var filePath = await _fileStorage.SaveAsync(
                file,
                "uploads/submissions");

            var submission = new Submission
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                FilePath = filePath
            };

            return await _repository.AddSubmission(submission);
        }
    }
}
