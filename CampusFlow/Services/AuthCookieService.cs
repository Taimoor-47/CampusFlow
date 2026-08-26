using Microsoft.AspNetCore.Http;

namespace CampusFlow.Services
{
    // Single place that owns the auth cookie's name and options. Controllers
    // never build CookieOptions themselves.
    public class AuthCookieService : IAuthCookieService
    {
        public const string CookieName = "jwt";

        private readonly AuthCookieOptions _options;

        public AuthCookieService(AuthCookieOptions options)
        {
            _options = options;
        }

        public void SetAuthCookie(HttpResponse response, string token)
        {
            response.Cookies.Append(CookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = Enum.TryParse<SameSiteMode>(_options.SameSite, ignoreCase: true, out var mode)
                    ? mode
                    : SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(_options.LifetimeDays)
            });
        }

        public void ClearAuthCookie(HttpResponse response)
        {
            response.Cookies.Delete(CookieName);
        }
    }
}
