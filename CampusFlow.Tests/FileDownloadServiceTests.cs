using CampusFlow.Data;
using CampusFlow.Model;
using CampusFlow.Services;
using Microsoft.EntityFrameworkCore;

namespace CampusFlow.Tests;

public class FileDownloadServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly FileDownloadService _sut;

    private readonly Guid _teacherId = Guid.NewGuid();
    private readonly Guid _otherTeacherId = Guid.NewGuid();
    private readonly Guid _enrolledStudentId = Guid.NewGuid();
    private readonly Guid _otherStudentId = Guid.NewGuid();
    private const string BriefPath = "/uploads/assignments/brief-a1b2.pdf";
    private const string SubmissionPath = "/uploads/submissions/sub-c3d4.pdf";

    public FileDownloadServiceTests()
    {
        _context = CreateContext();
        Seed();
        _sut = new FileDownloadService(_context);
    }

    public void Dispose() => _context.Dispose();

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private void Seed()
    {
        var course = new Course { CourseCode = "CS-101", CourseTitle = "Intro", Program = "BS Computer Science" };
        var section = new CourseSection
        {
            Course = course,
            CourseCode = course.CourseCode,
            TeacherId = _teacherId,
            SectionName = "A",
            AcademicYear = "2026",
            Semester = 1,
            IsActive = true
        };
        var assignment = new Assignment
        {
            Title = "Ex 1",
            Description = "",
            DueDate = DateTime.UtcNow.AddDays(7),
            FilePath = BriefPath,
            CourseSection = section,
            TeacherId = _teacherId
        };
        var submission = new Submission
        {
            Assignment = assignment,
            StudentId = _enrolledStudentId,
            FilePath = SubmissionPath
        };

        _context.Courses.Add(course);
        _context.CourseSections.Add(section);
        _context.Assignments.Add(assignment);
        _context.Submissions.Add(submission);
        // Only the enrolled student has an active enrollment in this section.
        _context.CourseEnrollments.Add(new CourseEnrollment
        {
            StudentId = _enrolledStudentId,
            CourseSection = section,
            IsActive = true
        });
        _context.SaveChanges();
    }

    // ── Assignment briefs ───────────────────────────────────────────────────

    [Fact]
    public async Task Brief_EnrolledActiveStudent_IsAllowed()
    {
        var result = await _sut.GetAccessibleAssignmentFileAsync(_enrolledStudentId, "Student", "brief-a1b2.pdf");
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Brief_NotEnrolledStudent_IsDenied()
    {
        var result = await _sut.GetAccessibleAssignmentFileAsync(_otherStudentId, "Student", "brief-a1b2.pdf");
        Assert.Null(result);
    }

    [Fact]
    public async Task Brief_OwningTeacher_IsAllowed()
    {
        var result = await _sut.GetAccessibleAssignmentFileAsync(_teacherId, "Teacher", "brief-a1b2.pdf");
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Brief_TeacherOfAnotherSection_IsDenied()
    {
        var result = await _sut.GetAccessibleAssignmentFileAsync(_otherTeacherId, "Teacher", "brief-a1b2.pdf");
        Assert.Null(result);
    }

    // ── Submissions ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Submission_OwningStudent_IsAllowed()
    {
        var result = await _sut.GetAccessibleSubmissionAsync(_enrolledStudentId, "Student", "sub-c3d4.pdf");
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Submission_DifferentStudent_IsDenied()
    {
        var result = await _sut.GetAccessibleSubmissionAsync(_otherStudentId, "Student", "sub-c3d4.pdf");
        Assert.Null(result);
    }

    [Fact]
    public async Task Submission_SectionTeacher_IsAllowed()
    {
        var result = await _sut.GetAccessibleSubmissionAsync(_teacherId, "Teacher", "sub-c3d4.pdf");
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Submission_TeacherOfAnotherSection_IsDenied()
    {
        var result = await _sut.GetAccessibleSubmissionAsync(_otherTeacherId, "Teacher", "sub-c3d4.pdf");
        Assert.Null(result);
    }

    // ── Unknown / missing ───────────────────────────────────────────────────

    [Fact]
    public async Task UnknownFileName_ReturnsNullForEveryRole()
    {
        Assert.Null(await _sut.GetAccessibleAssignmentFileAsync(_teacherId, "Teacher", "nope.pdf"));
        Assert.Null(await _sut.GetAccessibleSubmissionAsync(_enrolledStudentId, "Student", "nope.pdf"));
    }
}
