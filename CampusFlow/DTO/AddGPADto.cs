namespace CampusFlow.DTO
{
    public class AddGpaDto
    {
        public Guid StudentId { get; set; }

        public int Semester { get; set; }

        public double Gpa { get; set; }
    }
}
