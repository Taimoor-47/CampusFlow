using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusFlow.Model
{
    // A student's uploaded file (PDF/doc/etc.) submitted against a teacher's Assignment.
    // One row per (student, assignment). The actual file lives on disk under wwwroot;
    // only the public URL is stored here.
    public class Submission
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AssignmentId { get; set; }

        [ForeignKey(nameof(AssignmentId))]
        public Assignment? Assignment { get; set; }

        public Guid StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public Student? Student { get; set; }

        // Public URL of the uploaded file, e.g. "/uploads/submissions/{guid}.pdf".
        [Required]
        public string FilePath { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
