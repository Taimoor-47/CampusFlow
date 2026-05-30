using System.ComponentModel.DataAnnotations;

namespace CampusFlow.DTO
{
    public class StudentDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        [Range(1, 120)]
        public int Age { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
