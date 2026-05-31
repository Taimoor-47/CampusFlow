using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CampusFlow.DTO
{
    // Teacher creates an assignment for the class. Sent as multipart/form-data so an
    // optional brief file (PDF/doc) can be attached alongside the text fields.
    public class AddAssignmentDto
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        // Optional brief file. Null = create the assignment with text only.
        public IFormFile? File { get; set; }
    }
}
