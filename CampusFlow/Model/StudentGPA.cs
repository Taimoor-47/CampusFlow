using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusFlow.Model
{
    public class StudentGPA
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Semester { get; set; }
        public double Gpa { get; set; }
        //foreign key
        public Guid StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public Student student { get; set; }

    }
}
