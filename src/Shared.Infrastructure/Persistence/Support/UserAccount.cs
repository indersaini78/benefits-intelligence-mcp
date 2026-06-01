using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Support;

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

    public virtual ICollection<Case> Cases { get; set; } = new List<Case>();

    public virtual ICollection<Escalation> EscalationFromUsers { get; set; } = new List<Escalation>();

    public virtual ICollection<Escalation> EscalationToUsers { get; set; } = new List<Escalation>();

    public virtual ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();

    public virtual ICollection<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();

    public virtual Member? Member { get; set; }

    public virtual ICollection<PasswordResetRequest> PasswordResetRequests { get; set; } = new List<PasswordResetRequest>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
