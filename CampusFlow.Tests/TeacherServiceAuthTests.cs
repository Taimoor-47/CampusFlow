using CampusFlow.Data;
using CampusFlow.DTO;
using CampusFlow.Model;
using CampusFlow.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CampusFlow.Tests;

public class TeacherServiceAuthTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IPasswordService _passwordService =
        new PasswordService(new PasswordHasher<object>());

    public TeacherServiceAuthTests()
    {
        _context = CreateContext();
    }

    public void Dispose() => _context.Dispose();

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static string LegacySha256Hex(string password)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password))).ToLowerInvariant();

    [Fact]
    public async Task Register_StoresNonLegacyVerifiableHash()
    {
        var sut = new TeacherService(_context, new FakeFileStorage(), _passwordService);

        await sut.Register(new RegisterTeacherDto { Name = "Teach Er", Email = "t@test.dev", Password = "teach-pass" });

        var stored = await _context.Teachers.AsNoTracking().SingleAsync();
        Assert.NotEqual(LegacySha256Hex("teach-pass"), stored.Password);
        Assert.Equal(
            PasswordVerificationResult.Success,
            _passwordService.Verify(stored.Password, "teach-pass"));
    }

    [Fact]
    public async Task Login_WithCorrectLegacyHash_SucceedsAndUpgradesDatabaseRow()
    {
        var legacyHash = LegacySha256Hex("legacy-pass");
        var teacher = new Teacher { Name = "Old Teach", Email = "old@test.dev", Password = legacyHash };
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();
        var sut = new TeacherService(_context, new FakeFileStorage(), _passwordService);

        var loggedIn = await sut.Login(new LoginDto { Email = teacher.Email, Password = "legacy-pass" });

        Assert.NotNull(loggedIn);
        var reloaded = await _context.Teachers.AsNoTracking().SingleAsync(t => t.Id == teacher.Id);
        Assert.NotEqual(legacyHash, reloaded.Password);
        Assert.Equal(
            PasswordVerificationResult.Success,
            _passwordService.Verify(reloaded.Password, "legacy-pass"));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsNullAndLeavesHashUntouched()
    {
        var legacyHash = LegacySha256Hex("legacy-pass");
        var teacher = new Teacher { Name = "Old Teach", Email = "old@test.dev", Password = legacyHash };
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();
        var sut = new TeacherService(_context, new FakeFileStorage(), _passwordService);

        var result = await sut.Login(new LoginDto { Email = teacher.Email, Password = "nope" });

        Assert.Null(result);
        var reloaded = await _context.Teachers.AsNoTracking().SingleAsync(t => t.Id == teacher.Id);
        Assert.Equal(legacyHash, reloaded.Password);
    }
}
