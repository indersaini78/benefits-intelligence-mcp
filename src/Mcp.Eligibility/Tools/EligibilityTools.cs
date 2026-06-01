using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Shared.Contracts.Eligibility;
using Shared.Infrastructure.Persistence.Eligibility;
using System.ComponentModel;

namespace Mcp.Eligibility.Tools;

[McpServerToolType]
public sealed class EligibilityTools
{
    private static string RedactMemberId(string memberId) =>
        memberId.Length <= 4 ? "****" : new string('*', memberId.Length - 4) + memberId[^4..];

    [McpServerTool, Description("Returns active coverage records for a member as of a given date.")]
    public static async Task<List<CoverageDto>> CheckActiveCoverageAsync(
        EligibilityDbContext db,
        ILogger<EligibilityTools> logger,
        string memberId,
        DateOnly? asOfDate = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        var redacted = RedactMemberId(memberId);
        logger.LogInformation("CheckActiveCoverage entered for Member {MemberId}", redacted);

        var date = asOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var coverages = await db.MemberCoverages
            .AsNoTracking()
            .Include(c => c.Plan)
            .Where(c => c.MemberId == memberId
                        && c.Status == "Active"
                        && c.EffectiveDate <= date
                        && (c.TermDate == null || c.TermDate >= date))
            .Select(c => new CoverageDto(
                c.CoverageId,
                c.PlanId,
                c.Plan.PlanName,
                c.Tier,
                c.EffectiveDate,
                c.TermDate,
                c.Status,
                c.PrimaryCareProvNpi))
            .ToListAsync();

        if (coverages.Count == 0)
            throw new McpException($"No active coverage found for member {redacted} as of {date}.");

        logger.LogInformation("CheckActiveCoverage completed for Member {MemberId}, {Count} records", redacted, coverages.Count);
        return coverages;
    }

    [McpServerTool, Description("Returns accumulators (deductible, OOP) for a member in a given plan year.")]
    public static async Task<List<AccumulatorDto>> GetAccumulatorsAsync(
        EligibilityDbContext db,
        ILogger<EligibilityTools> logger,
        string memberId,
        int planYear)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        if (planYear < 2000 || planYear > 2100)
            throw new McpException("planYear must be between 2000 and 2100.");

        var redacted = RedactMemberId(memberId);
        logger.LogInformation("GetAccumulators entered for Member {MemberId}, Year {PlanYear}", redacted, planYear);

        var accumulators = await db.Accumulators
            .AsNoTracking()
            .Where(a => a.MemberId == memberId && a.PlanYear == planYear)
            .Select(a => new AccumulatorDto(
                a.AccumulatorId,
                a.PlanId,
                a.PlanYear,
                a.AccumulatorType,
                a.LimitAmount,
                a.AppliedAmount,
                a.LimitAmount - a.AppliedAmount,
                a.LastUpdatedUtc))
            .ToListAsync();

        if (accumulators.Count == 0)
            throw new McpException($"No accumulators found for member {redacted} in plan year {planYear}.");

        logger.LogInformation("GetAccumulators completed for Member {MemberId}, {Count} records", redacted, accumulators.Count);
        return accumulators;
    }

    [McpServerTool, Description("Returns plan summary details for a given plan ID.")]
    public static async Task<PlanSummaryDto> GetPlanDetailsAsync(
        EligibilityDbContext db,
        ILogger<EligibilityTools> logger,
        string planId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(planId);
        logger.LogInformation("GetPlanDetails entered for Plan {PlanId}", planId);

        var plan = await db.BenefitPlans
            .AsNoTracking()
            .Where(p => p.PlanId == planId)
            .Select(p => new PlanSummaryDto(
                p.PlanId,
                p.PlanName,
                p.LineOfBusiness,
                p.CarrierId,
                p.PlanYear,
                p.NetworkType,
                p.DeductibleInd,
                p.DeductibleFam,
                p.OopMaxInd,
                p.OopMaxFam,
                p.Copay,
                p.Coinsurance))
            .FirstOrDefaultAsync();

        if (plan is null)
            throw new McpException($"Plan '{planId}' not found.");

        logger.LogInformation("GetPlanDetails completed for Plan {PlanId}", planId);
        return plan;
    }
}
