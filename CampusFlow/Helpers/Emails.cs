namespace CampusFlow.Helpers
{
    // Single normalization rule for account emails so storage, lookups, and the
    // unique indexes always agree: trimmed and lowercase.
    public static class Emails
    {
        public static string Normalize(string email)
        {
            return email.Trim().ToLowerInvariant();
        }
    }
}
