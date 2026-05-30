using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusFlow.Model
{
    public class Student
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
        public int Age { get; set; }
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public bool IsActive { get; set; }
        [Required]
        public string Password { get; set; }
        public string Role { get; set; } = "Student";

        // Navigation properties 
        public List<StudentGPA> StudentGPA { get; set; }
        public List<Schedules> Schedules { get; set; } = new();
        public List<Assignment> Assignments { get; set; } = new();
    }
}
