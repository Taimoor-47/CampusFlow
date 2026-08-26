namespace CampusFlow.Services
{
    // Bound to the "AuthCookie" configuration section. Defaults mirror the
    // previous hardcoded values so enabling this section is not required.
    public class AuthCookieOptions
    {
        public const string SectionName = "AuthCookie";

        // SameSite=None is required while the frontend and API run on different
        // origins (localhost:3000 -> localhost:7288); same-site deployments can
        // tighten this to "Lax" or "Strict" via configuration.
        public string SameSite { get; set; } = "None";

        public int LifetimeDays { get; set; } = 7;
    }
}
