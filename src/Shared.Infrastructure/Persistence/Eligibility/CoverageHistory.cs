using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Eligibility;

public partial class CoverageHistory
{
    public long HistoryId { get; set; }

    public string CoverageId { get; set; } = null!;

    public string MemberId { get; set; } = null!;

    public string ChangeType { get; set; } = null!;

    public string? OldValueJson { get; set; }

    public string? NewValueJson { get; set; }

    public DateTime ChangedUtc { get; set; }

    public string? ChangedBy { get; set; }

    public virtual Member Member { get; set; } = null!;
}
