using Microsoft.AspNetCore.Http;

namespace CampusFlow.Services
{
    public interface IAuthCookieService
    {
        void SetAuthCookie(HttpResponse response, string token);

        void ClearAuthCookie(HttpResponse response);
    }
}
