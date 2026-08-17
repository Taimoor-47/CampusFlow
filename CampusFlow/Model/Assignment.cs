using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusFlow.Model;

public class Assignment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public string? FilePath { get; set; }

    [Required]
    public Guid CourseSectionId { get; set; }

    [ForeignKey(nameof(CourseSectionId))]
    public CourseSection CourseSection { get; set; } = null!;

    public Guid? TeacherId { get; set; }

    [ForeignKey(nameof(TeacherId))]
    public Teacher? Teacher { get; set; }

    public List<Submission> Submissions { get; set; } = new();
}