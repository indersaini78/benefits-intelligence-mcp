using BenefitsIntelligence.IntegrationTests.Fixtures;
using FluentAssertions;
using Mcp.Enrollment.Application.Commands;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Enrollment;
using Shared.Infrastructure.Persistence.Enrollment;

namespace BenefitsIntelligence.IntegrationTests;

[Collection("SqlServer")]
public sealed class EnrollmentWriteTests(SqlServerFixture fixture)
{
    private EnrollmentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EnrollmentDbContext>()
            .UseSqlServer(fixture.ConnectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory"))
            .Options;
        return new EnrollmentDbContext(options);
    }

    [Fact]
    public async Task CreateEnrollmentAsync_PersistsTransactionElectionAndOutbox()
    {
        // Arrange
        await using var db = CreateDbContext();
        var handler = new CreateEnrollmentHandler(db);

        var request = new CreateEnrollmentRequest(
            MemberId: "MEM-10004",
            TransactionType: "NewHire",
            EffectiveDate: new DateOnly(2026, 7, 1),
            ElectionsJson: """[{"PlanId":"MED-BRONZE-HDHP-26","Tier":"EE+Spouse","Action":"Enroll"}]""",
            Channel: "AgentPortal");

        var command = new CreateEnrollmentCommand(request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert - EnrollmentTransaction persisted
        result.EnrollmentId.Should().NotBeNullOrEmpty();
        result.Status.Should().Be("Completed");

        await using var verifyDb = CreateDbContext();

        var transaction = await verifyDb.EnrollmentTransactions
            .FirstOrDefaultAsync(t => t.EnrollmentId == result.EnrollmentId);
        transaction.Should().NotBeNull();
        transaction!.MemberId.Should().Be("MEM-10004");
        transaction.TransactionType.Should().Be("NewHire");

        // Assert - PlanElection persisted
        var elections = await verifyDb.PlanElections
            .Where(e => e.EnrollmentId == result.EnrollmentId)
            .ToListAsync();
        elections.Should().HaveCount(1);
        elections[0].PlanId.Should().Be("MED-BRONZE-HDHP-26");
        elections[0].Tier.Should().Be("EE+Spouse");
        elections[0].Action.Should().Be("Enroll");

        // Assert - OutboxEvent persisted
        var outbox = await verifyDb.OutboxEvents
            .FirstOrDefaultAsync(o => o.AggregateId == result.EnrollmentId);
        outbox.Should().NotBeNull();
        outbox!.EventType.Should().Be("EnrollmentCreated");
        outbox.PayloadJson.Should().Contain("MEM-10004");
    }
}
