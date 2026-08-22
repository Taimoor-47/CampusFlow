namespace CampusFlow.DTO
{
    public sealed class StudentSummaryDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public int Age { get; init; }
        public bool IsActive { get; init; }
    }
}