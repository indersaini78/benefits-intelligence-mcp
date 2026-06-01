using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Support;

public partial class LoginAttempt
{
    public long AttemptId { get; set; }

    public string? UserId { get; set; }

    public string Username { get; set; } = null!;

    public DateTime AttemptUtc { get; set; }

    public bool Success { get; set; }

    public string? FailureReason { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? DeviceId { get; set; }

    public virtual UserAccount? User { get; set; }
}
