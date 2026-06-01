using System;
using System.Collections.Generic;

namespace Shared.Infrastructure.Persistence.BillingPayroll;

public partial class Member
{
    public string MemberId { get; set; } = null!;

    public string GroupPolicyId { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly DOB { get; set; }

    public string? SsnLast4 { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? AddressLine1 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public string EmploymentStatus { get; set; } = null!;

    public DateOnly? HireDate { get; set; }

    public DateOnly? TerminationDate { get; set; }

    public string? SalaryBand { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }

    public virtual ICollection<Accumulator> Accumulators { get; set; } = new List<Accumulator>();

    public virtual ICollection<CoverageHistory> CoverageHistories { get; set; } = new List<CoverageHistory>();

    public virtual ICollection<Dependent> Dependents { get; set; } = new List<Dependent>();

    public virtual EmployerGroup GroupPolicy { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<MemberCoverage> MemberCoverages { get; set; } = new List<MemberCoverage>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<PayrollDeduction> PayrollDeductions { get; set; } = new List<PayrollDeduction>();
}
