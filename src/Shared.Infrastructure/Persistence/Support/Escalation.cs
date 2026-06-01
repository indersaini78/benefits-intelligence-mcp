using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Support;

public partial class Escalation
{
    public string EscalationId { get; set; } = null!;

    public string CaseId { get; set; } = null!;

    public string? FromUserId { get; set; }

    public string ToQueue { get; set; } = null!;

    public string? ToUserId { get; set; }

    public string Reason { get; set; } = null!;

    public string? ReasonDetail { get; set; }

    public string Status { get; set; } = null!;

    public DateTime EscalatedUtc { get; set; }

    public DateTime? AcknowledgedUtc { get; set; }

    public DateTime? ResolvedUtc { get; set; }

    public virtual Case Case { get; set; } = null!;

    public virtual UserAccount? FromUser { get; set; }

    public virtual UserAccount? ToUser { get; set; }
}
