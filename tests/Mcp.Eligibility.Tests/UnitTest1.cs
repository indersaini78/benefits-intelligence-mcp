using FluentAssertions;
using Mcp.Eligibility.Tests.Helpers;
using Mcp.Eligibility.Tools;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using Moq;

namespace Mcp.Eligibility.Tests;

public class CheckActiveCoverageAsyncTests
{
    private readonly ILogger<EligibilityTools> _logger = Mock.Of<ILogger<EligibilityTools>>();

    [Fact]
    public async Task ActiveCoverageExists_ReturnsRowsWithStatusActive()
    {
        // Arrange
        using var db = TestDataBuilder.CreateInMemoryContext();
        var plan = TestDataBuilder.CreatePlan();
        var coverage = TestDataBuilder.CreateCoverage(effectiveDate: new DateOnly(2025, 1, 1));
        db.BenefitPlans.Add(plan);
        db.MemberCoverages.Add(coverage);
        await db.SaveChangesAsync();

        // Act
        var result = await EligibilityTools.CheckActiveCoverageAsync(db, _logger, "MBR-123456", new DateOnly(2025, 6, 1));

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().Be("Active");
        result[0].PlanName.Should().Be("Gold PPO");
    }

    [Fact]
    public async Task MemberExistsButNoActiveCoverage_ThrowsMcpException()
    {
        // Arrange - member has coverage but status is Terminated
        using var db = TestDataBuilder.CreateInMemoryContext();
        var plan = TestDataBuilder.CreatePlan();
        var coverage = TestDataBuilder.CreateCoverage(status: "Terminated");
        db.BenefitPlans.Add(plan);
        db.MemberCoverages.Add(coverage);
        await db.SaveChangesAsync();

        // Act & Assert
        var act = () => EligibilityTools.CheckActiveCoverageAsync(db, _logger, "MBR-123456", new DateOnly(2025, 6, 1));
        await act.Should().ThrowAsync<McpException>();
    }

    [Fact]
    public async Task MemberNotFound_ThrowsMcpException()
    {
        // Arrange - empty database, no member exists
        using var db = TestDataBuilder.CreateInMemoryContext();

        // Act & Assert
        var act = () => EligibilityTools.CheckActiveCoverageAsync(db, _logger, "NONEXISTENT", new DateOnly(2025, 6, 1));
        await act.Should().ThrowAsync<McpException>();
    }

    [Fact]
    public async Task CoverageTerminatedBeforeAsOfDate_IsExcluded()
    {
        // Arrange - coverage terminated before the asOfDate
        using var db = TestDataBuilder.CreateInMemoryContext();
        var plan = TestDataBuilder.CreatePlan();
        var coverage = TestDataBuilder.CreateCoverage(
            effectiveDate: new DateOnly(2024, 1, 1),
            termDate: new DateOnly(2024, 12, 31));
        db.BenefitPlans.Add(plan);
        db.MemberCoverages.Add(coverage);
        await db.SaveChangesAsync();

        // Act & Assert - querying as of 2025-06-01, coverage ended 2024-12-31
        var act = () => EligibilityTools.CheckActiveCoverageAsync(db, _logger, "MBR-123456", new DateOnly(2025, 6, 1));
        await act.Should().ThrowAsync<McpException>();
    }
}
