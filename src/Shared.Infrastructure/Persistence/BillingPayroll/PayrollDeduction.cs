using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.BillingPayroll;

public partial class PayrollDeduction
{
    public string DeductionId { get; set; } = null!;

    public string MemberId { get; set; } = null!;

    public string PlanId { get; set; } = null!;

    public DateOnly PayPeriodStart { get; set; }

    public DateOnly PayPeriodEnd { get; set; }

    public decimal PreTaxAmount { get; set; }

    public decimal PostTaxAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedUtc { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual BenefitPlan Plan { get; set; } = null!;
}
