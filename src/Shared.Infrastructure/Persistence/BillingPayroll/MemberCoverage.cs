using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.BillingPayroll;

public partial class MemberCoverage
{
    public string CoverageId { get; set; } = null!;

    public string MemberId { get; set; } = null!;

    public string PlanId { get; set; } = null!;

    public string Tier { get; set; } = null!;

    public DateOnly EffectiveDate { get; set; }

    public DateOnly? TermDate { get; set; }

    public string Status { get; set; } = null!;

    public string? PrimaryCareProvNpi { get; set; }

    public DateTime CreatedUtc { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual BenefitPlan Plan { get; set; } = null!;
}
