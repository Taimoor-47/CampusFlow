using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusFlow.Model;

public class CourseEnrollment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid StudentId { get; set; }

    [ForeignKey(nameof(StudentId))]
    public Student Student { get; set; } = null!;

    [Required]
    public Guid CourseSectionId { get; set; }

    [ForeignKey(nameof(CourseSectionId))]
    public CourseSection CourseSection { get; set; } = null!;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}