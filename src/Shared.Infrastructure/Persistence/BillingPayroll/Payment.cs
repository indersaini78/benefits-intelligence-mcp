using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.BillingPayroll;

public partial class Payment
{
    public string PaymentId { get; set; } = null!;

    public string? InvoiceId { get; set; }

    public string? MemberId { get; set; }

    public string? GroupPolicyId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public DateOnly PaymentDate { get; set; }

    public string ConfirmationNumber { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual EmployerGroup? GroupPolicy { get; set; }

    public virtual Invoice? Invoice { get; set; }

    public virtual Member? Member { get; set; }
}
