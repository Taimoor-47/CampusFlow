namespace CampusFlow.DTO
{
    public class CourseGradeDto
    {
        public string? CourseCode { get; set; }
        public string ?CourseTitle { get; set; }
        public int CreditHours { get; set; }
        public int MarksObtained { get; set; }
        public int TotalMarks { get; set; }
        public double GradePoints { get; set; }
        public string ?LetterGrade { get; set; }
    }
}
