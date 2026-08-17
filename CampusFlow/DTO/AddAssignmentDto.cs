using System.ComponentModel.DataAnnotations;

namespace CampusFlow.DTO;

public class AddAssignmentDto
{
    [Required]
    public Guid CourseSectionId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime DueDate { get; set; }

    public IFormFile? File { get; set; }
}