namespace CampusFlow.DTO;

public class StudentAssignmentDto
{
    public Guid Id { get; set; }

    public Guid CourseSectionId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }

    public string? FilePath { get; set; }

    public bool Submitted { get; set; }
    public string? SubmissionFilePath { get; set; }
    public DateTime? SubmittedAt { get; set; }
}
