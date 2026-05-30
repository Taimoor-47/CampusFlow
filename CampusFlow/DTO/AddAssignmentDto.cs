using System.ComponentModel.DataAnnotations;

namespace CampusFlow.DTO
{
    public class AddAssignmentDto
    {
        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime DueDate { get; set; }
    }
}
