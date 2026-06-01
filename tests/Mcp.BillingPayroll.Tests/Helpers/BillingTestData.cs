using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.BillingPayroll;

namespace Mcp.BillingPayroll.Tests.Helpers;

public sealed class BillingTestData
{
    private readonly BillingPayrollDbContext _db;

    private BillingTestData(BillingPayrollDbContext db) => _db = db;

    public static BillingTestData For(BillingPayrollDbContext db) => new(db);

    public BillingTestData WithPlan(string planId, string planName, string lineOfBusiness = "Medical")
    {
        _db.BenefitPlans.Add(new BenefitPlan
        {
            PlanId = planId,
            PlanName = planName,
            LineOfBusiness = lineOfBusiness,
            CarrierId = "CARRIER-001",
            PlanYear = 2025
        });
        return this;
    }

    public BillingTestData WithInvoice(string invoiceId, string memberId, string status, decimal totalAmount, decimal balanceDue,
        DateOnly billingStart = default, DateOnly billingEnd = default)
    {
        if (billingStart == default) billingStart = new DateOnly(2025, 1, 1);
        if (billingEnd == default) billingEnd = new DateOnly(2025, 1, 31);

        _db.Invoices.Add(new Invoice
        {
            InvoiceId = invoiceId,
            GroupPolicyId = "GRP-001",
            MemberId = memberId,
            BillingPeriodStart = billingStart,
            BillingPeriodEnd = billingEnd,
            InvoiceDate = billingStart,
            DueDate = billingEnd.AddDays(30),
            TotalAmount = totalAmount,
            BalanceDue = balanceDue,
            Status = status,
            CreatedUtc = DateTime.UtcNow
        });
        return this;
    }

    public BillingTestData WithLine(long lineId, string invoiceId, string planId, string description,
        decimal premium, decimal employerPortion, decimal employeePortion)
    {
        _db.InvoiceLines.Add(new InvoiceLine
        {
            InvoiceLineId = lineId,
            InvoiceId = invoiceId,
            PlanId = planId,
            LineDescription = description,
            PremiumAmount = premium,
            EmployerPortion = employerPortion,
            EmployeePortion = employeePortion
        });
        return this;
    }

    public BillingTestData WithPayment(string paymentId, string memberId, string? invoiceId, decimal amount,
        DateOnly paymentDate, string status = "Posted")
    {
        _db.Payments.Add(new Payment
        {
            PaymentId = paymentId,
            MemberId = memberId,
            InvoiceId = invoiceId,
            Amount = amount,
            PaymentMethod = "ACH",
            PaymentDate = paymentDate,
            ConfirmationNumber = $"CONF-{paymentId}",
            Status = status
        });
        return this;
    }

    public BillingTestData WithRate(string rateId, string planId, string tier, decimal monthlyPremium,
        DateOnly effectiveDate, DateOnly? endDate = null, string? ageBand = null)
    {
        _db.PremiumRates.Add(new PremiumRate
        {
            RateId = rateId,
            PlanId = planId,
            Tier = tier,
            AgeBand = ageBand,
            MonthlyPremium = monthlyPremium,
            EffectiveDate = effectiveDate,
            EndDate = endDate
        });
        return this;
    }

    public BillingTestData WithDeduction(string deductionId, string memberId, string planId,
        DateOnly payPeriodStart, DateOnly payPeriodEnd, decimal preTax, decimal postTax, string status = "Applied")
    {
        _db.PayrollDeductions.Add(new PayrollDeduction
        {
            DeductionId = deductionId,
            MemberId = memberId,
            PlanId = planId,
            PayPeriodStart = payPeriodStart,
            PayPeriodEnd = payPeriodEnd,
            PreTaxAmount = preTax,
            PostTaxAmount = postTax,
            Status = status,
            CreatedUtc = DateTime.UtcNow
        });
        return this;
    }

    public async Task SeedAsync() => await _db.SaveChangesAsync();

    public static BillingPayrollDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BillingPayrollDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new BillingPayrollDbContext(options);
    }
}
