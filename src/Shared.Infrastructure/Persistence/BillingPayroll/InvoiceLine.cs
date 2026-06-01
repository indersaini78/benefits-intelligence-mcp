using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.BillingPayroll;

public partial class InvoiceLine
{
    public long InvoiceLineId { get; set; }

    public string InvoiceId { get; set; } = null!;

    public string PlanId { get; set; } = null!;

    public string LineDescription { get; set; } = null!;

    public decimal PremiumAmount { get; set; }

    public decimal EmployerPortion { get; set; }

    public decimal EmployeePortion { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual BenefitPlan Plan { get; set; } = null!;
}
