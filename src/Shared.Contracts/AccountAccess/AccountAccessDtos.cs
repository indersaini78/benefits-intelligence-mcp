namespace Shared.Contracts.AccountAccess;

// Read DTOs
public sealed record UserDiagnosticsDto(
    string UserId,
    string Username,
    string UserType,
    string Status,
    bool MfaEnrolled,
    DateTime? LastLoginUtc,
    int FailedLoginCount,
    DateTime? LockedUntilUtc,
    DateTime? PasswordChangedUtc,
    List<LoginAttemptDto> RecentAttempts);

public sealed record LoginAttemptDto(
    long AttemptId,
    DateTime AttemptUtc,
    bool Success,
    string? FailureReason,
    string? IpAddress,
    string? DeviceId);

// Write request/response DTOs
public sealed record UnlockAccountRequest(string UserId, string ReasonText);
public sealed record UnlockAccountResultDto(string UserId, string Status, string Message);

public sealed record InitiatePasswordResetRequest(string UserId, string Channel, string VerificationMethod);
public sealed record PasswordResetResultDto(string ResetRequestId, string Status, DateTime ExpiresUtc);

public sealed record VerifyIdentityRequest(string UserId, string DobYYYYMMDD, string SsnLast4);
public sealed record VerifyIdentityResultDto(bool IsMatch);
