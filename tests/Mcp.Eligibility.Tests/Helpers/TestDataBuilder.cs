using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Eligibility;

namespace Mcp.Eligibility.Tests.Helpers;

public static class TestDataBuilder
{
    public static EligibilityDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<EligibilityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new EligibilityDbContext(options);
    }

    public static BenefitPlan CreatePlan(string planId = "PLAN-001", string planName = "Gold PPO")
    {
        return new BenefitPlan
        {
            PlanId = planId,
            PlanName = planName,
            LineOfBusiness = "Medical",
            CarrierId = "CARRIER-01",
            PlanYear = 2025
        };
    }

    public static MemberCoverage CreateCoverage(
        string memberId = "MBR-123456",
        string planId = "PLAN-001",
        string status = "Active",
        DateOnly? effectiveDate = null,
        DateOnly? termDate = null,
        string tier = "Employee+Spouse",
        string? npi = "1234567890")
    {
        return new MemberCoverage
        {
            CoverageId = Guid.NewGuid().ToString(),
            MemberId = memberId,
            PlanId = planId,
            Status = status,
            EffectiveDate = effectiveDate ?? new DateOnly(2025, 1, 1),
            TermDate = termDate,
            Tier = tier,
            PrimaryCareProvNpi = npi,
            CreatedUtc = DateTime.UtcNow
        };
    }

    public static Member CreateMember(string memberId = "MBR-123456")
    {
        return new Member
        {
            MemberId = memberId,
            GroupPolicyId = "GP-001",
            FirstName = "John",
            LastName = "Doe",
            DOB = new DateOnly(1985, 5, 15),
        };
    }
}
