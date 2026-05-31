namespace CampusFlow.DTO
{
    public class SemesterResultDto
    {
        public int Semester { get; set; }
        public string AcademicYear { get; set; }
        public List<CourseGradeDto> Courses { get; set; }
        public double SemesterGpa { get; set; }
        public int TotalCreditHours { get; set; }
        public int EarnedCreditHours { get; set; }
    }
}
