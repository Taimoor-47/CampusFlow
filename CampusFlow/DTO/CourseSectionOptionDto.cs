namespace CampusFlow.DTO;

public class CourseSectionOptionDto
{
    public Guid Id { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public int Semester { get; set; }
}