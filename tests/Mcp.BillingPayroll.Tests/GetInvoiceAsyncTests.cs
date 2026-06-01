using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Mcp.BillingPayroll.Tests.Helpers;
using Mcp.BillingPayroll.Tools;
using ModelContextProtocol;

namespace Mcp.BillingPayroll.Tests;

public class GetInvoiceAsyncTests
{
    [Fact]
    public async Task HappyPath_ReturnsInvoiceWith4Lines()
    {
        using var db = BillingTestData.CreateInMemoryContext();
        await BillingTestData.For(db)
            .WithPlan("PLAN-MED-GOLD", "Med Gold")
            .WithPlan("PLAN-DENTAL", "Dental Basic")
            .WithPlan("PLAN-VISION", "Vision Plus")
            .WithPlan("PLAN-LIFE", "Life 1x")
            .WithInvoice("INV-10001-2603", "MBR-001", "Open", 1200m, 1200m)
            .WithLine(1, "INV-10001-2603", "PLAN-MED-GOLD", "Medical Gold Premium", 600m, 400m, 200m)
            .WithLine(2, "INV-10001-2603", "PLAN-DENTAL", "Dental Basic Premium", 300m, 200m, 100m)
            .WithLine(3, "INV-10001-2603", "PLAN-VISION", "Vision Plus Premium", 200m, 150m, 50m)
            .WithLine(4, "INV-10001-2603", "PLAN-LIFE", "Life 1x Premium", 100m, 80m, 20m)
            .SeedAsync();

        var logger = NullLogger<BillingTools>.Instance;

        var result = await BillingTools.GetInvoiceAsync(db, logger, "INV-10001-2603");

        result.InvoiceId.Should().Be("INV-10001-2603");
        result.Lines.Should().HaveCount(4);
        result.BalanceDue.Should().Be(1200m);
        result.Status.Should().Be("Open");
    }

    [Fact]
    public async Task InvoiceNotFound_ThrowsMcpException()
    {
        using var db = BillingTestData.CreateInMemoryContext();
        var logger = NullLogger<BillingTools>.Instance;

        var act = () => BillingTools.GetInvoiceAsync(db, logger, "INV-NONEXISTENT");

        await act.Should().ThrowAsync<McpException>();
    }
}

public class GetBalanceAsyncTests
{
    [Fact]
    public async Task OneOpenInvoice_ReturnsBalance()
    {
        using var db = BillingTestData.CreateInMemoryContext();
        await BillingTestData.For(db)
            .WithInvoice("INV-001", "MBR-010", "Open", 500m, 500m)
            .SeedAsync();

        var logger = NullLogger<BillingTools>.Instance;

        var result = await BillingTools.GetBalanceAsync(db, logger, "MBR-010");

        result.TotalBalanceDue.Should().Be(500m);
        result.OpenInvoiceCount.Should().Be(1);
    }

    [Fact]
    public async Task MultipleUnpaidStatuses_SumsAll()
    {
        using var db = BillingTestData.CreateInMemoryContext();
        await BillingTestData.For(db)
            .WithInvoice("INV-A", "MBR-020", "PastDue", 200m, 200m)
            .WithInvoice("INV-B", "MBR-020", "Open", 300m, 300m)
            .WithInvoice("INV-C", "MBR-020", "Disputed", 150m, 150m)
            .SeedAsync();

        var logger = NullLogger<BillingTools>.Instance;

        var result = await BillingTools.GetBalanceAsync(db, logger, "MBR-020");

        result.TotalBalanceDue.Should().Be(650m);
        result.OpenInvoiceCount.Should().Be(3);
    }

    [Fact]
    public async Task NoUnpaidInvoices_ReturnsZero()
    {
        using var db = BillingTestData.CreateInMemoryContext();
        await BillingTestData.For(db)
            .WithInvoice("INV-PAID", "MBR-030", "Paid", 500m, 0m)
            .SeedAsync();

        var logger = NullLogger<BillingTools>.Instance;

        var result = await BillingTools.GetBalanceAsync(db, logger, "MBR-030");

        result.TotalBalanceDue.Should().Be(0m);
        result.OpenInvoiceCount.Should().Be(0);
    }

    [Fact]
    public async Task MemberNotFound_ReturnsZeroBalance()
    {
        using var db = BillingTestData.CreateInMemoryContext();
        var logger = NullLogger<BillingTools>.Instance;

        var result = await BillingTools.GetBalanceAsync(db, logger, "MBR-GHOST");

        result.TotalBalanceDue.Should().Be(0m);
        result.OpenInvoiceCount.Should().Be(0);
    }
}

public class ListPaymentsAsyncTests
{
    [Fact]
    public async Task ReturnsPaymentsInDateRange()
    {
        using var db = BillingTestData.CreateInMemoryContext();
        await BillingTestData.For(db)
            .WithPayment("PAY-001", "MBR-040", "INV-001", 100m, new DateOnly(2025, 3, 1), "Posted")
            .WithPayment("PAY-002", "MBR-040", "INV-002", 50m, new DateOnly(2025, 3, 15), "Refunded")
            .WithPayment("PAY-003", "MBR-040", "INV-003", 75m, new DateOnly(2025, 4, 1), "Failed")
            .WithPayment("PAY-004", "MBR-040", null, 200m, new DateOnly(2025, 5, 1), "Posted")
            .SeedAsync();

        var logger = NullLogger<BillingTools>.Instance;

        var result = await BillingTools.ListPaymentsAsync(db, logger, "MBR-040",
            new DateOnly(2025, 3, 1), new DateOnly(2025, 4, 1));

        result.Should().HaveCount(3);
        result.Select(p => p.Status).Should().Contain("Posted").And.Contain("Refunded").And.Contain("Failed");
    }

    [Fact]
    public async Task EmptyRange_ReturnsEmptyList()
    {
        using var db = BillingTestData.CreateInMemoryContext();
        await BillingTestData.For(db)
            .WithPayment("PAY-X", "MBR-050", null, 100m, new DateOnly(2025, 6, 1))
            .SeedAsync();

        var logger = NullLogger<BillingTools>.Instance;

        var result = await BillingTools.ListPaymentsAsync(db, logger, "MBR-050",
            new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31));

        result.Should().BeEmpty();
    }
}

public class ExplainInvoiceLineAsyncTests
{
    [Fact]
    public async Task ReturnsLineWithPlanAndRate()
    {
        using var db = BillingTestData.CreateInMemoryContext();
        await BillingTestData.For(db)
            .WithPlan("PLAN-MED", "Medical Gold")
            .WithInvoice("INV-EXP", "MBR-060", "Open", 600m, 600m)
            .WithLine(100, "INV-EXP", "PLAN-MED", "Medical Gold Premium", 600m, 400m, 200m)
            .WithRate("RATE-001", "PLAN-MED", "Employee+Family", 600m, new DateOnly(2024, 1, 1), null, "30-39")
            .SeedAsync();

        var logger = NullLogger<BillingTools>.Instance;

        var result = await BillingTools.ExplainInvoiceLineAsync(db, logger, 100);

        result.PlanId.Should().Be("PLAN-MED");
        result.PlanName.Should().Be("Medical Gold");
        result.PremiumAmount.Should().Be(600m);
        result.MatchedRate.Should().NotBeNull();
        result.MatchedRate!.Tier.Should().Be("Employee+Family");
        (result.EmployerPortion + result.EmployeePortion).Should().Be(result.PremiumAmount);
    }

    [Fact]
    public async Task LineNotFound_ThrowsMcpException()
    {
        using var db = BillingTestData.CreateInMemoryContext();
        var logger = NullLogger<BillingTools>.Instance;

        var act = () => BillingTools.ExplainInvoiceLineAsync(db, logger, 99999);

        await act.Should().ThrowAsync<McpException>();
    }
}

public class GetDeductionsAsyncTests
{
    [Fact]
    public async Task ReturnsDeductionsInRange()
    {
        using var db = BillingTestData.CreateInMemoryContext();
        await BillingTestData.For(db)
            .WithDeduction("DED-001", "MBR-070", "PLAN-MED", new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 15), 100m, 20m)
            .WithDeduction("DED-002", "MBR-070", "PLAN-DENTAL", new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 15), 50m, 10m)
            .WithDeduction("DED-003", "MBR-070", "PLAN-MED", new DateOnly(2025, 4, 1), new DateOnly(2025, 4, 15), 100m, 20m)
            .SeedAsync();

        var logger = NullLogger<PayrollTools>.Instance;

        var result = await PayrollTools.GetDeductionsAsync(db, logger, "MBR-070",
            new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 15));

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task NoDeductionsInRange_ReturnsEmpty()
    {
        using var db = BillingTestData.CreateInMemoryContext();
        await BillingTestData.For(db)
            .WithDeduction("DED-X", "MBR-080", "PLAN-MED", new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 15), 100m, 20m)
            .SeedAsync();

        var logger = NullLogger<PayrollTools>.Instance;

        var result = await PayrollTools.GetDeductionsAsync(db, logger, "MBR-080",
            new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 15));

        result.Should().BeEmpty();
    }
}

public class ExplainPaycheckDeltaAsyncTests
{
    [Fact]
    public async Task BothPeriodsExist_ReturnsPerPlanDiff()
    {
        using var db = BillingTestData.CreateInMemoryContext();
        await BillingTestData.For(db)
            .WithDeduction("DED-P1", "MBR-090", "PLAN-MED", new DateOnly(2025, 2, 1), new DateOnly(2025, 2, 15), 100m, 20m)
            .WithDeduction("DED-P2", "MBR-090", "PLAN-MED", new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 15), 120m, 25m)
            .SeedAsync();

        var logger = NullLogger<PayrollTools>.Instance;

        var result = await PayrollTools.ExplainPaycheckDeltaAsync(db, logger, "MBR-090",
            new DateOnly(2025, 3, 1), new DateOnly(2025, 2, 1));

        result.Diffs.Should().HaveCount(1);
        result.Diffs[0].PlanId.Should().Be("PLAN-MED");
        result.Diffs[0].PreviousTotal.Should().Be(120m); // 100 + 20
        result.Diffs[0].CurrentTotal.Should().Be(145m);  // 120 + 25
        result.Diffs[0].Difference.Should().Be(25m);
    }

    [Fact]
    public async Task PreviousPeriodMissing_ReturnsEmptyDiffs()
    {
        using var db = BillingTestData.CreateInMemoryContext();
        await BillingTestData.For(db)
            .WithDeduction("DED-C", "MBR-100", "PLAN-MED", new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 15), 100m, 20m)
            .SeedAsync();

        var logger = NullLogger<PayrollTools>.Instance;

        var result = await PayrollTools.ExplainPaycheckDeltaAsync(db, logger, "MBR-100",
            new DateOnly(2025, 3, 1), new DateOnly(2025, 1, 1));

        // Only current exists, previous is 0
        result.Diffs.Should().HaveCount(1);
        result.Diffs[0].PreviousTotal.Should().Be(0m);
        result.Diffs[0].CurrentTotal.Should().Be(120m);
    }

    [Fact]
    public async Task AmountsIdentical_ReturnsDiffOfZero()
    {
        using var db = BillingTestData.CreateInMemoryContext();
        await BillingTestData.For(db)
            .WithDeduction("DED-S1", "MBR-110", "PLAN-MED", new DateOnly(2025, 2, 1), new DateOnly(2025, 2, 15), 100m, 20m)
            .WithDeduction("DED-S2", "MBR-110", "PLAN-MED", new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 15), 100m, 20m)
            .SeedAsync();

        var logger = NullLogger<PayrollTools>.Instance;

        var result = await PayrollTools.ExplainPaycheckDeltaAsync(db, logger, "MBR-110",
            new DateOnly(2025, 3, 1), new DateOnly(2025, 2, 1));

        result.Diffs.Should().HaveCount(1);
        result.Diffs[0].Difference.Should().Be(0m);
    }
}

