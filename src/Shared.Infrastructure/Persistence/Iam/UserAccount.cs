using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Iam;

public partial class UserAccount
{
    public string UserId { get; set; } = null!;

    public string? MemberId { get; set; }

    public string UserType { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? OktaUserId { get; set; }

    public string Status { get; set; } = null!;

    public bool MfaEnrolled { get; set; }

    public DateTime? LastLoginUtc { get; set; }

    public int FailedLoginCount { get; set; }

    public DateTime? LockedUntilUtc { get; set; }

    public DateTime? PasswordChangedUtc { get; set; }

    public DateTime CreatedUtc { get; set; }

    public virtual ICollection<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();

    public virtual Member? Member { get; set; }

    public virtual ICollection<PasswordResetRequest> PasswordResetRequests { get; set; } = new List<PasswordResetRequest>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
