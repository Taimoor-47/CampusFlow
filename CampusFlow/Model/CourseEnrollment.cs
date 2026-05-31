using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusFlow.Model
{
    public class CourseEnrollment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public Student? Student { get; set; }

        public Guid CourseId { get; set; }
        [ForeignKey(nameof(CourseId))]
        public Course? Course { get; set; }

        public int MarksObtained { get; set; }
        public int TotalMarks { get; set; }
        public string? AcademicYear { get; set; }
    }
}
