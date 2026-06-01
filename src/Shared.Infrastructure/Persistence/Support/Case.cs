using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Support;

public partial class Case
{
    public string CaseId { get; set; } = null!;

    public string? MemberId { get; set; }

    public string? GroupPolicyId { get; set; }

    public string CaseType { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public string Channel { get; set; } = null!;

    public string? AssignedToUserId { get; set; }

    public string? AssignedQueue { get; set; }

    public DateTime? SlaDueUtc { get; set; }

    public bool SlaBreached { get; set; }

    public string? CorrelationId { get; set; }

    public string CreatedByUserId { get; set; } = null!;

    public string CreatedBySource { get; set; } = null!;

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }

    public DateTime? ResolvedUtc { get; set; }

    public DateTime? ClosedUtc { get; set; }

    public string? ResolutionNotes { get; set; }

    public virtual UserAccount? AssignedToUser { get; set; }

    public virtual ICollection<CaseNote> CaseNotes { get; set; } = new List<CaseNote>();

    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    public virtual ICollection<Escalation> Escalations { get; set; } = new List<Escalation>();

    public virtual EmployerGroup? GroupPolicy { get; set; }

    public virtual ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();

    public virtual Member? Member { get; set; }
}
