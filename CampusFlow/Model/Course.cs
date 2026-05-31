using System.ComponentModel.DataAnnotations;

namespace CampusFlow.Model
{
    public class Course
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public int CreditHours { get; set; }
        public int Semester { get; set; }
        public string Program { get; set; }

    }
}
