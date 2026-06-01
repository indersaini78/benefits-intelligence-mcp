using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Iam;

namespace Mcp.AccountAccess.Tests.Helpers;

public sealed class IamTestData
{
    private readonly IamDbContext _db;

    private IamTestData(IamDbContext db) => _db = db;

    public static IamTestData For(IamDbContext db) => new(db);

    public IamTestData WithUser(string userId, string username, string status,
        bool mfaEnrolled = false, string? memberId = null, int failedLoginCount = 0, DateTime? lockedUntilUtc = null)
    {
        _db.UserAccounts.Add(new UserAccount
        {
            UserId = userId,
            Username = username,
            Email = $"{username}@test.com",
            UserType = "Member",
            Status = status,
            MfaEnrolled = mfaEnrolled,
            MemberId = memberId,
            FailedLoginCount = failedLoginCount,
            LockedUntilUtc = lockedUntilUtc,
            LastLoginUtc = DateTime.UtcNow.AddDays(-1),
            CreatedUtc = DateTime.UtcNow.AddYears(-1)
        });
        return this;
    }

    public IamTestData WithMember(string memberId, DateOnly dob, string? ssnLast4)
    {
        _db.Members.Add(new Member
        {
            MemberId = memberId,
            GroupPolicyId = "GRP-001",
            FirstName = "Test",
            LastName = "User",
            DOB = dob,
            SsnLast4 = ssnLast4,
            EmploymentStatus = "Active",
            CreatedUtc = DateTime.UtcNow
        });
        return this;
    }

    public IamTestData WithAttempts(string userId, string username, List<(DateTime ts, bool success, string? reason)> attempts)
    {
        foreach (var (ts, success, reason) in attempts)
        {
            _db.LoginAttempts.Add(new LoginAttempt
            {
                UserId = userId,
                Username = username,
                AttemptUtc = ts,
                Success = success,
                FailureReason = reason,
                IpAddress = "10.0.0.1"
            });
        }
        return this;
    }

    public IamTestData WithPasswordResetRequest(string resetId, string userId, string status)
    {
        _db.PasswordResetRequests.Add(new PasswordResetRequest
        {
            ResetRequestId = resetId,
            UserId = userId,
            RequestedUtc = DateTime.UtcNow,
            Channel = "Email",
            VerificationMethod = "OTP",
            Status = status,
            ExpiresUtc = DateTime.UtcNow.AddHours(1),
            InitiatedBy = "MCP-Agent"
        });
        return this;
    }

    public async Task SeedAsync() => await _db.SaveChangesAsync();

    public static IamDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<IamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new IamDbContext(options);
    }
}
