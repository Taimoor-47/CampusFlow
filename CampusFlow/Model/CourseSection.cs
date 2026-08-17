
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CampusFlow.Model
{
    public class CourseSection
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CourseId { get; set; }

        public string CourseCode { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = null!;

        [Required]
        public Guid TeacherId { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public Teacher Teacher { get; set; } = null!;

        [Required, MaxLength(50)]
        public string SectionName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string AcademicYear { get; set; } = string.Empty;

        public int Semester { get; set; }

        public bool IsActive { get; set; } = true;

        public List<CourseEnrollment> Enrollments { get; set; } = new();
        public List<Assignment> Assignments { get; set; } = new();
    }
}
