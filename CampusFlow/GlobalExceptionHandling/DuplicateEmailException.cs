namespace CampusFlow.GlobalExceptionHandling
{
    // Thrown by services when registration hits an email that already exists.
    // Message is a fixed, user-safe string; the global handler maps this to 409.
    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException()
            : base("An account with this email address already exists.")
        {
        }
    }
}
