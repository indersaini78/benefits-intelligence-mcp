using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.BillingPayroll;

public partial class PremiumRate
{
    public string RateId { get; set; } = null!;

    public string PlanId { get; set; } = null!;

    public string Tier { get; set; } = null!;

    public string? AgeBand { get; set; }

    public string? SalaryBand { get; set; }

    public decimal MonthlyPremium { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual BenefitPlan Plan { get; set; } = null!;
}
