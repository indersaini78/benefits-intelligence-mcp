using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Shared.Contracts.BillingPayroll;
using Shared.Infrastructure.Persistence.BillingPayroll;
using System.ComponentModel;

namespace Mcp.BillingPayroll.Tools;

[McpServerToolType]
public sealed class BillingTools
{
    private static string RedactMemberId(string memberId) =>
        memberId.Length <= 4 ? "****" : new string('*', memberId.Length - 4) + memberId[^4..];

    [McpServerTool, Description("Returns an invoice with all its line items.")]
    public static async Task<InvoiceDto> GetInvoiceAsync(
        BillingPayrollDbContext db,
        ILogger<BillingTools> logger,
        string invoiceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);
        logger.LogInformation("GetInvoice for InvoiceId={InvoiceId}", invoiceId);

        var invoice = await db.Invoices
            .AsNoTracking()
            .Include(i => i.InvoiceLines)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

        if (invoice is null)
            throw new McpException($"Invoice '{invoiceId}' not found.");

        var lines = invoice.InvoiceLines.Select(l => new InvoiceLineDto(
            l.InvoiceLineId, l.InvoiceId, l.PlanId, l.LineDescription,
            l.PremiumAmount, l.EmployerPortion, l.EmployeePortion)).ToList();

        return new InvoiceDto(
            invoice.InvoiceId, invoice.GroupPolicyId, invoice.MemberId,
            invoice.BillingPeriodStart, invoice.BillingPeriodEnd,
            invoice.InvoiceDate, invoice.DueDate,
            invoice.TotalAmount, invoice.BalanceDue, invoice.Status, lines);
    }

    [McpServerTool, Description("Returns the total balance due across all open invoices for a member.")]
    public static async Task<BalanceDto> GetBalanceAsync(
        BillingPayrollDbContext db,
        ILogger<BillingTools> logger,
        string memberId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        logger.LogInformation("GetBalance for Member {MemberId}", RedactMemberId(memberId));

        var openInvoices = await db.Invoices
            .AsNoTracking()
            .Where(i => i.MemberId == memberId && i.Status != "Paid")
            .ToListAsync();

        var totalBalance = openInvoices.Sum(i => i.BalanceDue);

        return new BalanceDto(RedactMemberId(memberId), totalBalance, openInvoices.Count);
    }

    [McpServerTool, Description("Lists payments for a member within a date range.")]
    public static async Task<List<PaymentDto>> ListPaymentsAsync(
        BillingPayrollDbContext db,
        ILogger<BillingTools> logger,
        string memberId,
        DateOnly from,
        DateOnly to)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        logger.LogInformation("ListPayments for Member {MemberId}, From={From}, To={To}",
            RedactMemberId(memberId), from, to);

        var payments = await db.Payments
            .AsNoTracking()
            .Where(p => p.MemberId == memberId && p.PaymentDate >= from && p.PaymentDate <= to)
            .Select(p => new PaymentDto(
                p.PaymentId, p.InvoiceId, p.Amount,
                p.PaymentMethod, p.PaymentDate, p.ConfirmationNumber, p.Status))
            .ToListAsync();

        return payments;
    }

    [McpServerTool, Description("Explains an invoice line by returning the line details, plan info, and the premium rate that produced it.")]
    public static async Task<InvoiceLineExplanationDto> ExplainInvoiceLineAsync(
        BillingPayrollDbContext db,
        ILogger<BillingTools> logger,
        long invoiceLineId)
    {
        logger.LogInformation("ExplainInvoiceLine for LineId={LineId}", invoiceLineId);

        var line = await db.InvoiceLines
            .AsNoTracking()
            .Include(l => l.Plan)
            .Include(l => l.Invoice)
            .FirstOrDefaultAsync(l => l.InvoiceLineId == invoiceLineId);

        if (line is null)
            throw new McpException($"Invoice line '{invoiceLineId}' not found.");

        // Find the premium rate that was effective during the invoice billing period
        var rate = await db.PremiumRates
            .AsNoTracking()
            .Where(r => r.PlanId == line.PlanId
                        && r.EffectiveDate <= line.Invoice.BillingPeriodEnd
                        && (r.EndDate == null || r.EndDate >= line.Invoice.BillingPeriodStart))
            .Select(r => new PremiumRateDto(
                r.RateId, r.Tier, r.AgeBand, r.SalaryBand,
                r.MonthlyPremium, r.EffectiveDate, r.EndDate))
            .FirstOrDefaultAsync();

        return new InvoiceLineExplanationDto(
            line.InvoiceLineId, line.PlanId, line.Plan.PlanName,
            line.LineDescription, line.PremiumAmount,
            line.EmployerPortion, line.EmployeePortion, rate);
    }
}
