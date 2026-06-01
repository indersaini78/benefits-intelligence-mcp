using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Iam;

public partial class PasswordResetRequest
{
    public string ResetRequestId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public DateTime RequestedUtc { get; set; }

    public string Channel { get; set; } = null!;

    public string VerificationMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime ExpiresUtc { get; set; }

    public DateTime? CompletedUtc { get; set; }

    public string InitiatedBy { get; set; } = null!;

    public virtual UserAccount User { get; set; } = null!;
}
