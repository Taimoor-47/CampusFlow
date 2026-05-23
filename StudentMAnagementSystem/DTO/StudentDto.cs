using System.ComponentModel.DataAnnotations;

namespace StudentMAnagementSystem.DTO
{
    public class StudentDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public int Age { get; set; }

        [Required]
        public bool isActice { get; set; }
        [Required]
        public string password { get; set; }
    }
}
