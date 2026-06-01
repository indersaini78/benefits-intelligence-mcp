using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.BillingPayroll;

public partial class BenefitPlan
{
    public string PlanId { get; set; } = null!;

    public string PlanName { get; set; } = null!;

    public string LineOfBusiness { get; set; } = null!;

    public string CarrierId { get; set; } = null!;

    public int PlanYear { get; set; }

    public string? NetworkType { get; set; }

    public decimal? DeductibleInd { get; set; }

    public decimal? DeductibleFam { get; set; }

    public decimal? OopMaxInd { get; set; }

    public decimal? OopMaxFam { get; set; }

    public decimal? Copay { get; set; }

    public decimal? Coinsurance { get; set; }

    public virtual ICollection<Accumulator> Accumulators { get; set; } = new List<Accumulator>();

    public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();

    public virtual ICollection<MemberCoverage> MemberCoverages { get; set; } = new List<MemberCoverage>();

    public virtual ICollection<PayrollDeduction> PayrollDeductions { get; set; } = new List<PayrollDeduction>();

    public virtual ICollection<PremiumRate> PremiumRates { get; set; } = new List<PremiumRate>();
}
