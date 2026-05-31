namespace CampusFlow.DTO
{
    public class GradeCardDto
    {
        public string StudentName { get; set; }
        public string RollNumber { get; set; }
        public string Program { get; set; }
        public string Department { get; set; }
        public List<SemesterResultDto> SemesterResults { get; set; }
        public double Cgpa { get; set; }
        public int TotalCreditHours { get; set; }
        public int TotalEarnedCredits { get; set; }
    }
}
