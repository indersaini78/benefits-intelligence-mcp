using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Support;

public partial class Complaint
{
    public string ComplaintId { get; set; } = null!;

    public string CaseId { get; set; } = null!;

    public string MemberId { get; set; } = null!;

    public string ComplaintType { get; set; } = null!;

    public string Severity { get; set; } = null!;

    public bool RegulatoryFlag { get; set; }

    public string? RegulatorAgency { get; set; }

    public string Description { get; set; } = null!;

    public string? RootCause { get; set; }

    public string? CorrectiveAction { get; set; }

    public string Status { get; set; } = null!;

    public DateTime FiledUtc { get; set; }

    public DateTime DueUtc { get; set; }

    public DateTime? ResolvedUtc { get; set; }

    public virtual Case Case { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;
}
