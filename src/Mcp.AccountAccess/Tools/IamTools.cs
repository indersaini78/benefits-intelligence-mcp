using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Shared.Contracts.AccountAccess;
using Shared.Infrastructure.Persistence.Iam;
using System.ComponentModel;

namespace Mcp.AccountAccess.Tools;

[McpServerToolType]
public sealed class IamTools
{
    private static string RedactMemberId(string memberId) =>
        memberId.Length <= 4 ? "****" : new string('*', memberId.Length - 4) + memberId[^4..];

    [McpServerTool, Description("Diagnoses login issues for a user: returns status, lockout state, MFA, and last 10 login attempts.")]
    public static async Task<UserDiagnosticsDto> DiagnoseLoginAsync(
        IamDbContext db,
        ILogger<IamTools> logger,
        string username)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        if (username.Length > 100)
            throw new McpException("Username must not exceed 100 characters.");

        logger.LogInformation("DiagnoseLogin for Username={Username}", username);

        var user = await db.UserAccounts
            .AsNoTracking()
            .Include(u => u.LoginAttempts.OrderByDescending(a => a.AttemptUtc).Take(10))
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user is null)
            throw new McpException($"User with username '{username}' not found.");

        var attempts = user.LoginAttempts
            .Select(a => new LoginAttemptDto(a.AttemptId, a.AttemptUtc, a.Success, a.FailureReason, a.IpAddress, a.DeviceId))
            .ToList();

        return new UserDiagnosticsDto(
            user.UserId, user.Username, user.UserType, user.Status,
            user.MfaEnrolled, user.LastLoginUtc, user.FailedLoginCount,
            user.LockedUntilUtc, user.PasswordChangedUtc, attempts);
    }

    [McpServerTool, Description("Lists recent login attempts for a user.")]
    public static async Task<List<LoginAttemptDto>> ListRecentAttemptsAsync(
        IamDbContext db,
        ILogger<IamTools> logger,
        string userId,
        int count = 20)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        if (count is < 1 or > 100) count = 20;

        logger.LogInformation("ListRecentAttempts for UserId={UserId}, Count={Count}", userId, count);

        var attempts = await db.LoginAttempts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AttemptUtc)
            .Take(count)
            .Select(a => new LoginAttemptDto(a.AttemptId, a.AttemptUtc, a.Success, a.FailureReason, a.IpAddress, a.DeviceId))
            .ToListAsync();

        return attempts;
    }

    [McpServerTool, Description("Unlocks a locked user account. Requires a reason.")]
    public static async Task<UnlockAccountResultDto> UnlockAccountAsync(
        IMediator mediator,
        ILogger<IamTools> logger,
        string userId,
        string reasonText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reasonText);

        logger.LogInformation("UnlockAccount for UserId={UserId}, Reason={Reason}", userId, reasonText);

        return await mediator.Send(new UnlockAccountCommand(new UnlockAccountRequest(userId, reasonText)));
    }

    [McpServerTool, Description("Initiates a password reset request for a user.")]
    public static async Task<PasswordResetResultDto> InitiatePasswordResetAsync(
        IMediator mediator,
        ILogger<IamTools> logger,
        string userId,
        string channel,
        string verificationMethod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(channel);
        ArgumentException.ThrowIfNullOrWhiteSpace(verificationMethod);

        logger.LogInformation("InitiatePasswordReset for UserId={UserId}, Channel={Channel}", userId, channel);

        return await mediator.Send(new InitiatePasswordResetCommand(
            new InitiatePasswordResetRequest(userId, channel, verificationMethod)));
    }

    [McpServerTool, Description("Verifies a user's identity using DOB and last 4 of SSN. Returns only a boolean match result.")]
    public static async Task<VerifyIdentityResultDto> VerifyIdentityAsync(
        IMediator mediator,
        ILogger<IamTools> logger,
        string userId,
        string dobYYYYMMDD,
        string ssnLast4)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(dobYYYYMMDD);
        ArgumentException.ThrowIfNullOrWhiteSpace(ssnLast4);

        logger.LogInformation("VerifyIdentity for UserId={UserId}", userId);

        return await mediator.Send(new VerifyIdentityCommand(
            new VerifyIdentityRequest(userId, dobYYYYMMDD, ssnLast4)));
    }
}
