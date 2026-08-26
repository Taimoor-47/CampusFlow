using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace CampusFlow.Services
{
    public sealed class PasswordService : IPasswordService
    {
        private static readonly object UserMarker = new();
        private readonly IPasswordHasher<object> _passwordHasher;

        public PasswordService(IPasswordHasher<object> passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }
        public string Hash(string password)
        {
            return _passwordHasher.HashPassword(UserMarker, password);
        }
        public PasswordVerificationResult Verify(string storedHash, string providedPassword)
        {
            if (IsLegacySha256Hash(storedHash))
            {
                var storedHashBytes = Convert.FromHexString(storedHash);

                var providedHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(providedPassword));

                var matches = CryptographicOperations.FixedTimeEquals(storedHashBytes, providedHashBytes);

                return matches
                    ? PasswordVerificationResult.SuccessRehashNeeded
                    : PasswordVerificationResult.Failed;
            }

            try
            {
                return _passwordHasher.VerifyHashedPassword(
                    UserMarker,
                    storedHash,
                    providedPassword);
            }
            catch (FormatException)
            {
                // A malformed stored value must fail authentication.
                return PasswordVerificationResult.Failed;
            }
            catch (ArgumentException)
            {
                return PasswordVerificationResult.Failed;
            }
        }

        private static bool IsLegacySha256Hash(string hash)
        {
            return hash.Length == 64 &&
                   hash.All(Uri.IsHexDigit);
        }
    }

}

