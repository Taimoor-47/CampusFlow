using CampusFlow.Model;
using CampusFlow.Repositories;
using CampusFlow.Services;
using Microsoft.AspNetCore.Http;

namespace CampusFlow.Tests;

public sealed class FakeFileStorage : IFileStorageService
{
    public Task<string> SaveAsync(IFormFile file, string subFolder)
        => Task.FromResult($"/uploads/{subFolder}/fake.bin");
}

public sealed class FakeStudentRepository : IStudentRepository
{
    public Dictionary<Guid, Student> Students { get; } = new();
    public List<(Guid StudentId, string PasswordHash)> PasswordHashUpdates { get; } = new();

    public Task<Student> RegisterStudent(Student student)
    {
        Students[student.Id] = student;
        return Task.FromResult(student);
    }

    public Task<Student?> GetByEmail(string email)
        => Task.FromResult(Students.Values.FirstOrDefault(s => s.Email == email));

    public Task UpdatePasswordHash(Guid studentId, string passwordHash)
    {
        PasswordHashUpdates.Add((studentId, passwordHash));
        if (Students.TryGetValue(studentId, out var student))
        {
            student.Password = passwordHash;
        }
        return Task.CompletedTask;
    }

    // Auth tests must never touch these members.
    private static NotSupportedException Unused() => new("Not used by authentication flows.");

    public Task<List<Student>> GetAllStudents() => throw Unused();
    public Task<Student?> GetById(Guid id) => throw Unused();
    public Task<IReadOnlyList<StudentGPA>> GetGpaByStudentId(Guid studentId) => throw Unused();
    public Task<IReadOnlyList<Schedules>> GetSchedulesByStudentId(Guid studentId) => throw Unused();
    public Task<IReadOnlyList<Assignment>> GetAssignmentsForStudent(Guid studentId) => throw Unused();
    public Task<Assignment?> GetAccessibleAssignment(Guid assignmentId, Guid studentId) => throw Unused();
    public Task<IReadOnlyList<Submission>> GetSubmissionsByStudentId(Guid studentId) => throw Unused();
    public Task<Submission?> GetSubmission(Guid assignmentId, Guid studentId) => throw Unused();
    public Task<Submission> AddSubmission(Submission submission) => throw Unused();
}
