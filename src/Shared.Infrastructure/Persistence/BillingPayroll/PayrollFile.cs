using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.BillingPayroll;

public partial class PayrollFile
{
    public string PayrollFileId { get; set; } = null!;

    public string GroupPolicyId { get; set; } = null!;

    public DateOnly PayDate { get; set; }

    public string? FileBlobUri { get; set; }

    public decimal TotalDeductions { get; set; }

    public string Status { get; set; } = null!;

    public virtual EmployerGroup GroupPolicy { get; set; } = null!;
}
