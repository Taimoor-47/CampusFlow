using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusFlow.Model
{
    public class Schedules
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string CourseTitle { get; set; }
        public string Room { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public Guid StudentId { get; set; }

        public Student Student { get; set; }
    }
}
