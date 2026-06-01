using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Shared.Contracts.BillingPayroll;
using Shared.Infrastructure.Persistence.BillingPayroll;
using System.ComponentModel;

namespace Mcp.BillingPayroll.Tools;

[McpServerToolType]
public sealed class PayrollTools
{
    private static string RedactMemberId(string memberId) =>
        memberId.Length <= 4 ? "****" : new string('*', memberId.Length - 4) + memberId[^4..];

    [McpServerTool, Description("Returns payroll deductions for a member within a pay period range.")]
    public static async Task<List<PayrollDeductionDto>> GetDeductionsAsync(
        BillingPayrollDbContext db,
        ILogger<PayrollTools> logger,
        string memberId,
        DateOnly payPeriodStart,
        DateOnly payPeriodEnd)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        logger.LogInformation("GetDeductions for Member {MemberId}, Period={Start} to {End}",
            RedactMemberId(memberId), payPeriodStart, payPeriodEnd);

        var deductions = await db.PayrollDeductions
            .AsNoTracking()
            .Where(d => d.MemberId == memberId
                        && d.PayPeriodStart >= payPeriodStart
                        && d.PayPeriodEnd <= payPeriodEnd)
            .Select(d => new PayrollDeductionDto(
                d.DeductionId, d.PlanId, d.PayPeriodStart, d.PayPeriodEnd,
                d.PreTaxAmount, d.PostTaxAmount, d.Status))
            .ToListAsync();

        return deductions;
    }

    [McpServerTool, Description("Explains differences in payroll deductions between two pay dates, showing per-plan diffs.")]
    public static async Task<PaycheckDeltaDto> ExplainPaycheckDeltaAsync(
        BillingPayrollDbContext db,
        ILogger<PayrollTools> logger,
        string memberId,
        DateOnly currentPayDate,
        DateOnly previousPayDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        logger.LogInformation("ExplainPaycheckDelta for Member {MemberId}, Current={Current}, Previous={Previous}",
            RedactMemberId(memberId), currentPayDate, previousPayDate);

        var currentDeductions = await db.PayrollDeductions
            .AsNoTracking()
            .Where(d => d.MemberId == memberId && d.PayPeriodStart == currentPayDate)
            .ToListAsync();

        var previousDeductions = await db.PayrollDeductions
            .AsNoTracking()
            .Where(d => d.MemberId == memberId && d.PayPeriodStart == previousPayDate)
            .ToListAsync();

        var allPlanIds = currentDeductions.Select(d => d.PlanId)
            .Union(previousDeductions.Select(d => d.PlanId))
            .Distinct();

        var diffs = allPlanIds.Select(planId =>
        {
            var prevTotal = previousDeductions
                .Where(d => d.PlanId == planId)
                .Sum(d => d.PreTaxAmount + d.PostTaxAmount);
            var currTotal = currentDeductions
                .Where(d => d.PlanId == planId)
                .Sum(d => d.PreTaxAmount + d.PostTaxAmount);

            return new PlanDeductionDiffDto(planId, prevTotal, currTotal, currTotal - prevTotal);
        }).ToList();

        return new PaycheckDeltaDto(RedactMemberId(memberId), currentPayDate, previousPayDate, diffs);
    }
}
