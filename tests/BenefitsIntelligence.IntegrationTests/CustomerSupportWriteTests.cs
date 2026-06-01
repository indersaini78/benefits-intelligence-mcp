using BenefitsIntelligence.IntegrationTests.Fixtures;
using FluentAssertions;
using Mcp.CustomerSupport.Application.Commands;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Support;
using Shared.Infrastructure.Persistence.Support;

namespace BenefitsIntelligence.IntegrationTests;

[Collection("SqlServer")]
public sealed class CustomerSupportWriteTests(SqlServerFixture fixture)
{
    private SupportDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SupportDbContext>()
            .UseSqlServer(fixture.ConnectionString)
            .Options;
        return new SupportDbContext(options);
    }

    [Fact]
    public async Task CreateCase_AddNote_Escalate_TransitionsToEscalated()
    {
        // Step 1: Create a case
        await using var db1 = CreateDbContext();
        var createHandler = new CreateCaseHandler(db1);

        var createRequest = new CreateCaseRequest(
            MemberId: "MEM-10004",
            CaseType: "BillingDispute",
            Subject: "Integration test case",
            Description: "Testing full case lifecycle with real SQL Server",
            Priority: "High",
            Channel: "Chat",
            CorrelationId: Guid.NewGuid().ToString(),
            CreatedBy: "USR-CSR-001",
            CreatedBySource: "Agent");

        var createResult = await createHandler.Handle(new CreateCaseCommand(createRequest), CancellationToken.None);
        createResult.CaseId.Should().NotBeNullOrEmpty();
        createResult.Status.Should().Be("New");

        // Step 2: Add a note
        await using var db2 = CreateDbContext();
        var noteHandler = new AddNoteHandler(db2);

        var noteRequest = new AddNoteRequest(
            CaseId: createResult.CaseId,
            AuthorUserId: "USR-CSR-001",
            AuthorType: "Agent",
            NoteText: "Investigating billing discrepancy with member.",
            IsInternal: true);

        var noteResult = await noteHandler.Handle(new AddNoteCommand(noteRequest), CancellationToken.None);
        noteResult.CaseNoteId.Should().BeGreaterThan(0);

        // Step 3: Escalate
        await using var db3 = CreateDbContext();
        var escalateHandler = new EscalateHandler(db3);

        var escalateRequest = new EscalateRequest(
            CaseId: createResult.CaseId,
            FromUserId: "USR-CSR-001",
            ToQueue: "Tier2-Billing",
            Reason: "MemberDissatisfied",
            ReasonDetail: "Member demands supervisor review");

        var escalateResult = await escalateHandler.Handle(new EscalateCommand(escalateRequest), CancellationToken.None);
        escalateResult.CaseStatus.Should().Be("Escalated");
        escalateResult.EscalationId.Should().NotBeNullOrEmpty();

        // Verify final state
        await using var verifyDb = CreateDbContext();
        var finalCase = await verifyDb.Cases.FirstAsync(c => c.CaseId == createResult.CaseId);
        finalCase.Status.Should().Be("Escalated");

        var notes = await verifyDb.CaseNotes.Where(n => n.CaseId == createResult.CaseId).ToListAsync();
        notes.Should().HaveCount(1);

        var escalations = await verifyDb.Escalations.Where(e => e.CaseId == createResult.CaseId).ToListAsync();
        escalations.Should().HaveCount(1);
        escalations[0].ToQueue.Should().Be("Tier2-Billing");
        escalations[0].Status.Should().Be("Pending");
    }
}
