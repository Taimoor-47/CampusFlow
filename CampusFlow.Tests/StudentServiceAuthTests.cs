using CampusFlow.DTO;
using CampusFlow.Model;
using CampusFlow.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace CampusFlow.Tests;

public class StudentServiceAuthTests
{
    private readonly FakeStudentRepository _repository = new();
    private readonly IPasswordService _passwordService =
        new PasswordService(new PasswordHasher<object>());

    private StudentService CreateSut()
        => new(_repository, new FakeFileStorage(), _passwordService);

    private static string LegacySha256Hex(string password)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password))).ToLowerInvariant();

    private Student SeedStudentWithLegacyHash()
    {
        var student = new Student
        {
            Name = "Legacy User",
            Email = "legacy@test.dev",
            PhoneNumber = "+1234567",
            Age = 21,
            IsActive = true,
            Password = LegacySha256Hex("legacy-pass")
        };
        _repository.Students[student.Id] = student;
        return student;
    }

    [Fact]
    public async Task RegisterStudent_StoresNonLegacyVerifiableHash()
    {
        var sut = CreateSut();

        await sut.RegisterStudent(new StudentDto
        {
            Name = "Ann Example",
            Email = "ann@test.dev",
            PhoneNumber = "+1234567",
            Age = 20,
            Password = "password123"
        });

        var stored = _repository.Students.Values.Single();
        Assert.NotEqual(LegacySha256Hex("password123"), stored.Password);
        Assert.Equal(
            PasswordVerificationResult.Success,
            _passwordService.Verify(stored.Password, "password123"));
    }

    [Fact]
    public async Task Login_WithCorrectLegacyHash_SucceedsAndUpgradesStoredHash()
    {
        var seeded = SeedStudentWithLegacyHash();
        var originalHash = seeded.Password;
        var sut = CreateSut();

        var loggedIn = await sut.LoginStudent(new LoginDto { Email = seeded.Email, Password = "legacy-pass" });

        Assert.NotNull(loggedIn);
        var upgraded = _repository.Students[seeded.Id];
        Assert.NotEqual(originalHash, upgraded.Password);
        Assert.Equal(
            PasswordVerificationResult.Success,
            _passwordService.Verify(upgraded.Password, "legacy-pass"));
        Assert.Contains(_repository.PasswordHashUpdates, u => u.StudentId == seeded.Id);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsNullAndDoesNotUpgrade()
    {
        var seeded = SeedStudentWithLegacyHash();
        var sut = CreateSut();

        var result = await sut.LoginStudent(new LoginDto { Email = seeded.Email, Password = "not-the-password" });

        Assert.Null(result);
        Assert.Empty(_repository.PasswordHashUpdates);
        Assert.Equal(seeded.Password, _repository.Students[seeded.Id].Password);
    }

    [Fact]
    public async Task Login_WithModernHash_DoesNotRewriteTheStoredHash()
    {
        var sut = CreateSut();
        await sut.RegisterStudent(new StudentDto
        {
            Name = "Ann Example",
            Email = "ann@test.dev",
            PhoneNumber = "+1234567",
            Age = 20,
            Password = "password123"
        });
        var originalHash = _repository.Students.Values.Single().Password;

        var loggedIn = await sut.LoginStudent(new LoginDto { Email = "ann@test.dev", Password = "password123" });

        Assert.NotNull(loggedIn);
        Assert.Empty(_repository.PasswordHashUpdates);
        Assert.Equal(originalHash, _repository.Students.Values.Single().Password);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsNullWithoutUpgradingAnything()
    {
        var sut = CreateSut();

        var result = await sut.LoginStudent(new LoginDto { Email = "nobody@test.dev", Password = "whatever" });

        Assert.Null(result);
        Assert.Empty(_repository.PasswordHashUpdates);
    }
}
