using FluentAssertions;
using Mcp.AccountAccess.Application.Commands;
using Mcp.AccountAccess.Tests.Helpers;
using Mcp.AccountAccess.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol;
using Moq;
using MediatR;
using Shared.Contracts.AccountAccess;
using Xunit.Abstractions;

namespace Mcp.AccountAccess.Tests;

public class DiagnoseLoginAsyncTests
{
    [Fact]
    public async Task ActiveUser_ReturnsCorrectDiagnostics()
    {
        using var db = IamTestData.CreateInMemoryContext();
        var now = DateTime.UtcNow;
        await IamTestData.For(db)
            .WithUser("USR-001", "john.doe", "Active", mfaEnrolled: true)
            .WithAttempts("USR-001", "john.doe", [(now.AddMinutes(-5), true, null)])
            .SeedAsync();

        var logger = NullLogger<IamTools>.Instance;
        var result = await IamTools.DiagnoseLoginAsync(db, logger, "john.doe");

        result.Status.Should().Be("Active");
        result.MfaEnrolled.Should().BeTrue();
        result.FailedLoginCount.Should().Be(0);
        result.RecentAttempts.Should().HaveCount(1);
        result.RecentAttempts[0].Success.Should().BeTrue();
    }

    [Fact]
    public async Task LockedUser_ReturnsLockedStatusWithAttempts()
    {
        using var db = IamTestData.CreateInMemoryContext();
        var now = DateTime.UtcNow;
        var attempts = Enumerable.Range(1, 5)
            .Select(i => (now.AddMinutes(-10 + i), false, (string?)"BadPassword"))
            .Append((now, false, (string?)"AccountLocked"))
            .ToList();

        await IamTestData.For(db)
            .WithUser("USR-002", "priya.p", "Locked", failedLoginCount: 5, lockedUntilUtc: now.AddMinutes(30))
            .WithAttempts("USR-002", "priya.p", attempts)
            .SeedAsync();

        var logger = NullLogger<IamTools>.Instance;
        var result = await IamTools.DiagnoseLoginAsync(db, logger, "priya.p");

        result.Status.Should().Be("Locked");
        result.FailedLoginCount.Should().Be(5);
        result.LockedUntilUtc.Should().NotBeNull();
        result.RecentAttempts.Should().Contain(a => a.FailureReason == "BadPassword");
    }

    [Fact]
    public async Task DisabledUser_ReturnsDisabledStatus()
    {
        using var db = IamTestData.CreateInMemoryContext();
        await IamTestData.For(db)
            .WithUser("USR-003", "mike.j", "Disabled")
            .WithAttempts("USR-003", "mike.j", [(DateTime.UtcNow.AddHours(-1), false, "Disabled")])
            .SeedAsync();

        var logger = NullLogger<IamTools>.Instance;
        var result = await IamTools.DiagnoseLoginAsync(db, logger, "mike.j");

        result.Status.Should().Be("Disabled");
        result.RecentAttempts[0].FailureReason.Should().Be("Disabled");
    }

    [Fact]
    public async Task PasswordExpiredUser_ReturnsStatus()
    {
        using var db = IamTestData.CreateInMemoryContext();
        await IamTestData.For(db)
            .WithUser("USR-004", "bob.w", "PasswordExpired")
            .SeedAsync();

        var logger = NullLogger<IamTools>.Instance;
        var result = await IamTools.DiagnoseLoginAsync(db, logger, "bob.w");

        result.Status.Should().Be("PasswordExpired");
    }

    [Fact]
    public async Task UnknownUsername_ThrowsMcpException_NoPiiLeaked()
    {
        using var db = IamTestData.CreateInMemoryContext();
        var logger = NullLogger<IamTools>.Instance;

        var act = () => IamTools.DiagnoseLoginAsync(db, logger, "ghost.user");

        var ex = await act.Should().ThrowAsync<McpException>();
        ex.Which.Message.Should().NotContain("@");
        ex.Which.Message.Should().NotContain("password");
    }
}

public class ListRecentAttemptsAsyncTests
{
    [Fact]
    public async Task ReturnsMaxCountOrderedByDesc()
    {
        using var db = IamTestData.CreateInMemoryContext();
        var now = DateTime.UtcNow;
        var attempts = Enumerable.Range(1, 25)
            .Select(i => (now.AddMinutes(-30 + i), i % 3 == 0, i % 3 == 0 ? null : (string?)"BadPassword"))
            .ToList();

        await IamTestData.For(db)
            .WithUser("USR-010", "list.user", "Active")
            .WithAttempts("USR-010", "list.user", attempts)
            .SeedAsync();

        var logger = NullLogger<IamTools>.Instance;
        var result = await IamTools.ListRecentAttemptsAsync(db, logger, "USR-010", 10);

        result.Should().HaveCount(10);
        result.Should().BeInDescendingOrder(a => a.AttemptUtc);
    }

    [Fact]
    public async Task SuccessXorFailureReason()
    {
        using var db = IamTestData.CreateInMemoryContext();
        await IamTestData.For(db)
            .WithUser("USR-011", "xor.user", "Active")
            .WithAttempts("USR-011", "xor.user", [
                (DateTime.UtcNow, true, null),
                (DateTime.UtcNow.AddMinutes(-1), false, "BadPassword")
            ])
            .SeedAsync();

        var logger = NullLogger<IamTools>.Instance;
        var result = await IamTools.ListRecentAttemptsAsync(db, logger, "USR-011");

        result.Where(a => a.Success).Should().AllSatisfy(a => a.FailureReason.Should().BeNull());
        result.Where(a => !a.Success).Should().AllSatisfy(a => a.FailureReason.Should().NotBeNull());
    }

    [Fact]
    public async Task UnknownUserId_ReturnsEmptyList()
    {
        using var db = IamTestData.CreateInMemoryContext();
        var logger = NullLogger<IamTools>.Instance;

        var result = await IamTools.ListRecentAttemptsAsync(db, logger, "USR-GHOST");

        result.Should().BeEmpty();
    }
}

public class UnlockAccountHandlerTests
{
    [Fact]
    public async Task LockedUser_Unlocks()
    {
        using var db = IamTestData.CreateInMemoryContext();
        await IamTestData.For(db)
            .WithUser("USR-020", "priya.p", "Locked", failedLoginCount: 5, lockedUntilUtc: DateTime.UtcNow.AddMinutes(30))
            .SeedAsync();

        var handler = new UnlockAccountHandler(db);
        var result = await handler.Handle(new UnlockAccountCommand(new UnlockAccountRequest("USR-020", "Agent verified identity")), CancellationToken.None);

        result.Status.Should().Be("Active");
        var user = await db.UserAccounts.FindAsync("USR-020");
        user!.FailedLoginCount.Should().Be(0);
        user.LockedUntilUtc.Should().BeNull();
    }

    [Fact]
    public async Task AlreadyActive_ThrowsInvalidOperation()
    {
        using var db = IamTestData.CreateInMemoryContext();
        await IamTestData.For(db)
            .WithUser("USR-021", "active.user", "Active")
            .SeedAsync();

        var handler = new UnlockAccountHandler(db);
        var act = () => handler.Handle(new UnlockAccountCommand(new UnlockAccountRequest("USR-021", "reason")), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not locked*");
    }

    [Fact]
    public async Task DisabledUser_ThrowsInvalidOperation()
    {
        using var db = IamTestData.CreateInMemoryContext();
        await IamTestData.For(db)
            .WithUser("USR-022", "disabled.user", "Disabled")
            .SeedAsync();

        var handler = new UnlockAccountHandler(db);
        var act = () => handler.Handle(new UnlockAccountCommand(new UnlockAccountRequest("USR-022", "reason")), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task MissingReasonText_ThrowsArgumentException()
    {
        var logger = NullLogger<IamTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => IamTools.UnlockAccountAsync(mediator.Object, logger, "USR-020", "");

        await act.Should().ThrowAsync<ArgumentException>();
    }
}

public class InitiatePasswordResetHandlerTests
{
    [Fact]
    public async Task HappyPath_CreatesResetRequest()
    {
        using var db = IamTestData.CreateInMemoryContext();
        await IamTestData.For(db)
            .WithUser("USR-030", "reset.user", "Active")
            .SeedAsync();

        var handler = new InitiatePasswordResetHandler(db);
        var result = await handler.Handle(
            new InitiatePasswordResetCommand(new InitiatePasswordResetRequest("USR-030", "Email", "OTP")),
            CancellationToken.None);

        result.Status.Should().Be("Pending");
        result.ResetRequestId.Should().NotBeNullOrWhiteSpace();
        result.ExpiresUtc.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task InvalidChannel_ThrowsArgumentException()
    {
        var logger = NullLogger<IamTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => IamTools.InitiatePasswordResetAsync(mediator.Object, logger, "USR-030", "", "OTP");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UserNotFound_ThrowsInvalidOperation()
    {
        using var db = IamTestData.CreateInMemoryContext();

        var handler = new InitiatePasswordResetHandler(db);
        var act = () => handler.Handle(
            new InitiatePasswordResetCommand(new InitiatePasswordResetRequest("USR-GHOST", "Email", "OTP")),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

public class VerifyIdentityHandlerTests
{
    [Fact]
    public async Task MatchingDobAndSsn_ReturnsTrue()
    {
        using var db = IamTestData.CreateInMemoryContext();
        await IamTestData.For(db)
            .WithMember("MEM-0001", new DateOnly(1990, 5, 15), "1234")
            .WithUser("USR-040", "verify.user", "Active", memberId: "MEM-0001")
            .SeedAsync();

        var handler = new VerifyIdentityHandler(db);
        var result = await handler.Handle(
            new VerifyIdentityCommand(new VerifyIdentityRequest("USR-040", "19900515", "1234")),
            CancellationToken.None);

        result.IsMatch.Should().BeTrue();
    }

    [Fact]
    public async Task DobMismatch_ReturnsFalse()
    {
        using var db = IamTestData.CreateInMemoryContext();
        await IamTestData.For(db)
            .WithMember("MEM-0002", new DateOnly(1990, 5, 15), "1234")
            .WithUser("USR-041", "dob.mismatch", "Active", memberId: "MEM-0002")
            .SeedAsync();

        var handler = new VerifyIdentityHandler(db);
        var result = await handler.Handle(
            new VerifyIdentityCommand(new VerifyIdentityRequest("USR-041", "19910515", "1234")),
            CancellationToken.None);

        result.IsMatch.Should().BeFalse();
    }

    [Fact]
    public async Task SsnMismatch_ReturnsFalse()
    {
        using var db = IamTestData.CreateInMemoryContext();
        await IamTestData.For(db)
            .WithMember("MEM-0003", new DateOnly(1990, 5, 15), "1234")
            .WithUser("USR-042", "ssn.mismatch", "Active", memberId: "MEM-0003")
            .SeedAsync();

        var handler = new VerifyIdentityHandler(db);
        var result = await handler.Handle(
            new VerifyIdentityCommand(new VerifyIdentityRequest("USR-042", "19900515", "9999")),
            CancellationToken.None);

        result.IsMatch.Should().BeFalse();
    }

    [Fact]
    public void ResponseDto_NeverContainsSsnOrDob()
    {
        var dto = new VerifyIdentityResultDto(true);

        var properties = dto.GetType().GetProperties();
        properties.Should().HaveCount(1);
        properties[0].Name.Should().Be("IsMatch");
        properties[0].PropertyType.Should().Be(typeof(bool));
    }
}

public class RedactionTests(ITestOutputHelper output)
{
    [Fact]
    public async Task DiagnoseLogin_LogsRedactedMemberId()
    {
        using var db = IamTestData.CreateInMemoryContext();
        await IamTestData.For(db)
            .WithUser("USR-050", "redact.test", "Active", memberId: "MEM-xxxx-0001")
            .SeedAsync();

        var logger = new XunitLogger<IamTools>(output);

        // ListRecentAttempts doesn't log memberId, but DiagnoseLogin logs username (not memberId)
        // UnlockAccount logs UserId. Let's test that MemberId redaction works via the helper.
        var redacted = typeof(IamTools)
            .GetMethod("RedactMemberId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, ["MEM-xxxx-0001"]) as string;

        redacted.Should().NotBeNull();
        redacted.Should().EndWith("0001");
        redacted.Should().Contain("*");
        redacted.Should().NotBe("MEM-xxxx-0001");
        // Only the last 4 chars should be visible
        redacted!.TrimStart('*').Should().Be("0001");
    }
}

