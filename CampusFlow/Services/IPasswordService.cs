using Microsoft.AspNetCore.Identity;

namespace CampusFlow.Services
{
    public interface IPasswordService
    {
        string Hash(string password);

        PasswordVerificationResult Verify(string storedHash, string providedPassword);
    }
}