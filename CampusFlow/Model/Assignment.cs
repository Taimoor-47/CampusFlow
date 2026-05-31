using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusFlow.Model
{
    // An assignment created by a teacher for the whole class.
    // The teacher may attach a brief file (PDF/doc); students respond via Submissions.
    public class Assignment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime DueDate { get; set; }

        // Public URL of the teacher's uploaded brief, e.g. "/uploads/assignments/{guid}.pdf".
        // Null when the teacher created the assignment without attaching a file.
        public string? FilePath { get; set; }

        public Guid? TeacherId { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public Teacher? Teacher { get; set; }

        // Student submissions against this assignment.
        public List<Submission> Submissions { get; set; } = new();
    }
}
