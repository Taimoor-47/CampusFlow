using System.ComponentModel.DataAnnotations;

namespace CampusFlow.DTO
{
    public class AddScheduleDto
    {
        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public string CourseTitle { get; set; }

        [Required]
        public string Room { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }
    }
}
