using System.Security.Cryptography;
using System.Text;

namespace CampusFlow.Helpers
{
    // Hashes passwords with SHA-256. Never store passwords as plain text.
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }

        public static bool Verify(string plainText, string hash)
        {
            return Hash(plainText) == hash;
        }
    }
}
