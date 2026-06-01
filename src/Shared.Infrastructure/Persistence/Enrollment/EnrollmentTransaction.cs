using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Enrollment;

public partial class EnrollmentTransaction
{
    public string EnrollmentId { get; set; } = null!;

    public string MemberId { get; set; } = null!;

    public string GroupPolicyId { get; set; } = null!;

    public string TransactionType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateOnly EffectiveDate { get; set; }

    public string SubmittedBy { get; set; } = null!;

    public string SubmittedChannel { get; set; } = null!;

    public string? CorrelationId { get; set; }

    public string RequestPayloadJson { get; set; } = null!;

    public string? ResultPayloadJson { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime? CompletedUtc { get; set; }

    public virtual EmployerGroup GroupPolicy { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;

    public virtual ICollection<PlanElection> PlanElections { get; set; } = new List<PlanElection>();
}
