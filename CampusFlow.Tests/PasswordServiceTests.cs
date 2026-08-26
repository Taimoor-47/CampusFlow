using CampusFlow.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace CampusFlow.Tests;

public class PasswordServiceTests
{
    private readonly PasswordService _sut = new(new PasswordHasher<object>());

    private static string LegacySha256Hex(string password)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password))).ToLowerInvariant();

    [Fact]
    public void Hash_ThenVerify_WithCorrectPassword_ReturnsSuccess()
    {
        var hash = _sut.Hash("correct horse battery staple");

        Assert.Equal(
            PasswordVerificationResult.Success,
            _sut.Verify(hash, "correct horse battery staple"));
    }

    [Fact]
    public void Hash_DoesNotProduceLegacyUnsaltedSha256Format()
    {
        var hash = _sut.Hash("any-password-1");

        // Legacy format was exactly 64 lowercase hex characters (unsalted SHA-256).
        Assert.NotEqual(LegacySha256Hex("any-password-1"), hash);
        Assert.False(hash.Length == 64 && hash.All(Uri.IsHexDigit));
    }

    [Fact]
    public void Hash_Twice_ProducesDifferentHashesBecauseOfPerUserSalt()
    {
        Assert.NotEqual(_sut.Hash("same-password"), _sut.Hash("same-password"));
    }

    [Fact]
    public void Verify_LegacySha256WithMatchingPassword_ReturnsSuccessRehashNeeded()
    {
        var stored = LegacySha256Hex("legacy-password");

        Assert.Equal(
            PasswordVerificationResult.SuccessRehashNeeded,
            _sut.Verify(stored, "legacy-password"));
    }

    [Fact]
    public void Verify_LegacySha256WithWrongPassword_ReturnsFailed()
    {
        var stored = LegacySha256Hex("legacy-password");

        Assert.Equal(PasswordVerificationResult.Failed, _sut.Verify(stored, "wrong-password"));
    }

    [Fact]
    public void Verify_ModernHashWithWrongPassword_ReturnsFailed()
    {
        var hash = _sut.Hash("right-password");

        Assert.Equal(PasswordVerificationResult.Failed, _sut.Verify(hash, "wrong-password"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public void Verify_MalformedStoredValue_ReturnsFailedWithoutThrowing(string stored)
    {
        Assert.Equal(PasswordVerificationResult.Failed, _sut.Verify(stored, "whatever-password"));
    }
}
