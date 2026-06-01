using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.BillingPayroll;

public partial class Invoice
{
    public string InvoiceId { get; set; } = null!;

    public string GroupPolicyId { get; set; } = null!;

    public string? MemberId { get; set; }

    public DateOnly BillingPeriodStart { get; set; }

    public DateOnly BillingPeriodEnd { get; set; }

    public DateOnly InvoiceDate { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal BalanceDue { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedUtc { get; set; }

    public virtual EmployerGroup GroupPolicy { get; set; } = null!;

    public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();

    public virtual Member? Member { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
