using System.ComponentModel.DataAnnotations;

namespace CampusFlow.Model
{
    public class Teacher
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string Role { get; set; } = "Teacher";

        public List<CourseSection> CourseSections { get; set; } = new();
    }
}
