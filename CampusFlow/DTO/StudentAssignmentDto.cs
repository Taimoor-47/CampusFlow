namespace CampusFlow.DTO
{
    // What a student sees on "my-assignments": every teacher assignment plus
    // whether this student has already submitted, and where their file lives.
    public class StudentAssignmentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }

        // The teacher's brief file URL (null if none attached).
        public string? FilePath { get; set; }

        public bool Submitted { get; set; }
        public string? SubmissionFilePath { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }
}
