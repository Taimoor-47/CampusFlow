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
                Name        = dto.Name,
                Email       = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Age         = dto.Age,
                IsActive    = true,
                Password    = PasswordHelper.Hash(dto.Password)
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

        public async Task<IReadOnlyList<StudentAssignmentDto>> GetMyAssignments(Guid studentId)
        {
            var assignments = await _repository.GetAllAssignments();
            var submissions = await _repository.GetSubmissionsByStudentId(studentId);

            // Index this student's submissions by assignment for an O(1) lookup.
            var byAssignment = submissions.ToDictionary(s => s.AssignmentId);

            return assignments.Select(a =>
            {
                byAssignment.TryGetValue(a.Id, out var submission);
                return new StudentAssignmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    DueDate = a.DueDate,
                    FilePath = a.FilePath,
                    Submitted = submission is not null,
                    SubmissionFilePath = submission?.FilePath,
                    SubmittedAt = submission?.SubmittedAt
                };
            }).ToList();
        }

        public async Task<Submission> SubmitAssignment(Guid assignmentId, Guid studentId, IFormFile file)
        {
            var assignment = await _repository.GetAssignmentById(assignmentId);
            if (assignment is null)
                throw new KeyNotFoundException($"No assignment found with ID {assignmentId}.");

            var existing = await _repository.GetSubmission(assignmentId, studentId);
            if (existing is not null)
                throw new InvalidOperationException("You have already submitted this assignment.");

            // Persist the file first; if storage throws, nothing is written to the DB.
            var filePath = await _fileStorage.SaveAsync(file, "uploads/submissions");

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
