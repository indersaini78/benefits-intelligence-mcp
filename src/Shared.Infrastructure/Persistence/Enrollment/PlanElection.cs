using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.Enrollment;

public partial class PlanElection
{
    public string ElectionId { get; set; } = null!;

    public string EnrollmentId { get; set; } = null!;

    public string PlanId { get; set; } = null!;

    public string Tier { get; set; } = null!;

    public DateOnly EffectiveDate { get; set; }

    public string Action { get; set; } = null!;

    public virtual EnrollmentTransaction Enrollment { get; set; } = null!;

    public virtual BenefitPlan Plan { get; set; } = null!;
}
