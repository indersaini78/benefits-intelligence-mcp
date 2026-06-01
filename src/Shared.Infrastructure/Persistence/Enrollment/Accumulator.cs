using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Enrollment;

public partial class Accumulator
{
    public long AccumulatorId { get; set; }

    public string MemberId { get; set; } = null!;

    public string PlanId { get; set; } = null!;

    public int PlanYear { get; set; }

    public string AccumulatorType { get; set; } = null!;

    public decimal LimitAmount { get; set; }

    public decimal AppliedAmount { get; set; }

    public DateTime LastUpdatedUtc { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual BenefitPlan Plan { get; set; } = null!;
}
