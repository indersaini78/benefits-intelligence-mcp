using FluentAssertions;
using Mcp.CustomerSupport.Application.Commands;
using Mcp.CustomerSupport.Tests.Helpers;
using Mcp.CustomerSupport.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol;
using Moq;
using MediatR;
using Shared.Contracts.Support;

namespace Mcp.CustomerSupport.Tests;

public class GetCaseAsyncTests
{
    [Fact]
    public async Task HappyPath_ReturnsCaseWithNotesAndEscalationAndInteraction()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        await SupportTestData.For(db)
            .WithCase("CASE-002", "MEM-10001", "Issue", "Open")
            .WithNote("CASE-002", "Agent", "First note")
            .WithNote("CASE-002", "CSR", "Second note")
            .WithEscalation("CASE-002", "Tier2", "LowConfidence")
            .WithInteraction("CASE-002", "MEM-10001")
            .SeedAsync();

        var logger = NullLogger<SupportTools>.Instance;
        var result = await SupportTools.GetCaseAsync(db, logger, "CASE-002");

        result.CaseId.Should().Be("CASE-002");
        result.Notes.Should().HaveCount(2);
        result.Escalations.Should().HaveCount(1);
        result.Interactions.Should().HaveCount(1);
    }

    [Fact]
    public async Task CaseNotFound_ThrowsMcpException()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        var logger = NullLogger<SupportTools>.Instance;

        var act = () => SupportTools.GetCaseAsync(db, logger, "CASE-GHOST");

        await act.Should().ThrowAsync<McpException>();
    }

    [Fact]
    public async Task CaseWithNoChildren_ReturnsEmptyCollections()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        await SupportTestData.For(db)
            .WithCase("CASE-EMPTY", "MEM-001", "Inquiry", "New")
            .SeedAsync();

        var logger = NullLogger<SupportTools>.Instance;
        var result = await SupportTools.GetCaseAsync(db, logger, "CASE-EMPTY");

        result.Notes.Should().NotBeNull().And.BeEmpty();
        result.Interactions.Should().NotBeNull().And.BeEmpty();
        result.Escalations.Should().NotBeNull().And.BeEmpty();
    }
}

public class ListOpenByMemberAsyncTests
{
    [Fact]
    public async Task ReturnsOnlyOpenCases()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        await SupportTestData.For(db)
            .WithCase("CASE-O1", "MEM-10001", "Issue", "New")
            .WithCase("CASE-O2", "MEM-10001", "Billing", "InProgress")
            .WithCase("CASE-O3", "MEM-10001", "Issue", "Resolved")
            .WithCase("CASE-O4", "MEM-10001", "Issue", "Closed")
            .SeedAsync();

        var logger = NullLogger<SupportTools>.Instance;
        var result = await SupportTools.ListOpenByMemberAsync(db, logger, "MEM-10001");

        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.Status != "Resolved" && c.Status != "Closed");
    }

    [Fact]
    public async Task NoOpenCases_ReturnsEmpty()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        await SupportTestData.For(db)
            .WithCase("CASE-CL", "MEM-20001", "Issue", "Closed")
            .SeedAsync();

        var logger = NullLogger<SupportTools>.Instance;
        var result = await SupportTools.ListOpenByMemberAsync(db, logger, "MEM-20001");

        result.Should().BeEmpty();
    }
}

public class ListCasesByQueueAsyncTests
{
    [Fact]
    public async Task ReturnsAssignedCases()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        await SupportTestData.For(db)
            .WithCase("CASE-Q1", "MEM-001", "Enrollment", "New", assignedQueue: "EnrollmentOps")
            .WithCase("CASE-Q2", "MEM-002", "Enrollment", "InProgress", assignedQueue: "EnrollmentOps")
            .WithCase("CASE-Q3", "MEM-003", "Billing", "New", assignedQueue: "BillingOps")
            .SeedAsync();

        var logger = NullLogger<SupportTools>.Instance;
        var result = await SupportTools.ListCasesByQueueAsync(db, logger, "EnrollmentOps");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task DefaultCountLimit_Honored()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        // Seed 55 cases
        var builder = SupportTestData.For(db);
        for (int i = 0; i < 55; i++)
            builder.WithCase($"CASE-LIM-{i:D3}", "MEM-001", "Issue", "New", assignedQueue: "BigQueue");
        await builder.SeedAsync();

        var logger = NullLogger<SupportTools>.Instance;
        var result = await SupportTools.ListCasesByQueueAsync(db, logger, "BigQueue");

        result.Should().HaveCount(50);
    }
}

public class CreateCaseHandlerTests
{
    [Fact]
    public async Task HappyPath_CreatesCase_StatusNew_UpdatedEqCreated()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        var handler = new CreateCaseHandler(db);

        var result = await handler.Handle(new CreateCaseCommand(
            new CreateCaseRequest("MEM-001", "Issue", "Test Subject", "Description", "High", "Phone", null, "USR-001", "CSR")),
            CancellationToken.None);

        result.Status.Should().Be("New");
        var entity = await db.Cases.FirstAsync(c => c.CaseId == result.CaseId);
        entity.CreatedUtc.Should().Be(entity.UpdatedUtc);
        entity.CreatedByUserId.Should().Be("USR-001");
        entity.CreatedBySource.Should().Be("CSR");
    }

    [Fact]
    public async Task InvalidCaseType_ThrowsMcpException()
    {
        var logger = NullLogger<SupportTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => SupportTools.CreateCaseAsync(mediator.Object, logger,
            "MEM-001", "InvalidType", "Subject", "Desc", "High", "Phone", null, "USR-001", "CSR");

        await act.Should().ThrowAsync<McpException>().WithMessage("*Invalid*caseType*");
    }

    [Fact]
    public async Task MissingSubject_ThrowsArgumentException()
    {
        var logger = NullLogger<SupportTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => SupportTools.CreateCaseAsync(mediator.Object, logger,
            "MEM-001", "Issue", "", "Desc", "High", "Phone", null, "USR-001", "CSR");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task NullCorrelationId_Accepted()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        var handler = new CreateCaseHandler(db);

        var result = await handler.Handle(new CreateCaseCommand(
            new CreateCaseRequest("MEM-001", "Inquiry", "Question", "Details", "Low", "Chat", null, "USR-001", "Agent")),
            CancellationToken.None);

        result.CaseId.Should().NotBeNullOrWhiteSpace();
        var entity = await db.Cases.FirstAsync(c => c.CaseId == result.CaseId);
        entity.CorrelationId.Should().BeNull();
    }
}

public class AddNoteHandlerTests
{
    [Fact]
    public async Task HappyPath_CreatesNote()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        await SupportTestData.For(db).WithCase("CASE-N1", "MEM-001", "Issue", "Open").SeedAsync();

        var handler = new AddNoteHandler(db);
        var result = await handler.Handle(new AddNoteCommand(
            new AddNoteRequest("CASE-N1", "USR-001", "Agent", "This is a note", true)),
            CancellationToken.None);

        result.CaseNoteId.Should().BeGreaterThan(0);
        var note = await db.CaseNotes.FirstAsync(n => n.CaseNoteId == result.CaseNoteId);
        note.IsInternal.Should().BeTrue();
    }

    [Fact]
    public async Task CaseNotFound_ThrowsInvalidOperation()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        var handler = new AddNoteHandler(db);

        var act = () => handler.Handle(new AddNoteCommand(
            new AddNoteRequest("CASE-GHOST", "USR-001", "Agent", "Note", true)),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task InvalidAuthorType_ThrowsMcpException()
    {
        var logger = NullLogger<SupportTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => SupportTools.AddNoteAsync(mediator.Object, logger, "CASE-001", "USR-001", "Bot", "Note", true);

        await act.Should().ThrowAsync<McpException>().WithMessage("*Invalid*authorType*");
    }

    [Fact]
    public async Task EmptyNoteText_ThrowsArgumentException()
    {
        var logger = NullLogger<SupportTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => SupportTools.AddNoteAsync(mediator.Object, logger, "CASE-001", "USR-001", "Agent", "", true);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}

public class LogInteractionHandlerTests
{
    [Fact]
    public async Task HappyPathWithCaseLink_InsertsRow()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        await SupportTestData.For(db).WithCase("CASE-INT", "MEM-001", "Issue", "Open").SeedAsync();

        var handler = new LogInteractionHandler(db);
        var result = await handler.Handle(new LogInteractionCommand(
            new LogInteractionRequest("MEM-001", "CASE-INT", "Phone", "Inbound", "BillingInquiry", "Called about bill", "SES-001", 120)),
            CancellationToken.None);

        result.InteractionId.Should().NotBeNullOrWhiteSpace();
        var interaction = await db.Interactions.FirstAsync(i => i.InteractionId == result.InteractionId);
        interaction.CaseId.Should().Be("CASE-INT");
    }

    [Fact]
    public async Task NoCaseLink_InsertsRowWithoutCase()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        var handler = new LogInteractionHandler(db);

        var result = await handler.Handle(new LogInteractionCommand(
            new LogInteractionRequest("MEM-001", null, "Chat", "Outbound", null, null, null, null)),
            CancellationToken.None);

        var interaction = await db.Interactions.FirstAsync(i => i.InteractionId == result.InteractionId);
        interaction.CaseId.Should().BeNull();
    }

    [Fact]
    public async Task InvalidChannel_ThrowsMcpException()
    {
        var logger = NullLogger<SupportTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => SupportTools.LogInteractionAsync(mediator.Object, logger,
            "MEM-001", null, "Fax", "Inbound", null, null, null, null);

        await act.Should().ThrowAsync<McpException>().WithMessage("*Invalid*channel*");
    }

    [Fact]
    public async Task InvalidDirection_ThrowsMcpException()
    {
        var logger = NullLogger<SupportTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => SupportTools.LogInteractionAsync(mediator.Object, logger,
            "MEM-001", null, "Phone", "Lateral", null, null, null, null);

        await act.Should().ThrowAsync<McpException>().WithMessage("*Invalid*direction*");
    }
}

public class EscalateHandlerTests
{
    [Fact]
    public async Task HappyPath_InsertsEscalation_UpdatesCaseStatus()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        await SupportTestData.For(db).WithCase("CASE-ESC", "MEM-001", "Issue", "Open").SeedAsync();

        var handler = new EscalateHandler(db);
        var result = await handler.Handle(new EscalateCommand(
            new EscalateRequest("CASE-ESC", "USR-001", "Tier2", "LowConfidence", "Agent unsure")),
            CancellationToken.None);

        result.CaseStatus.Should().Be("Escalated");
        var parentCase = await db.Cases.FirstAsync(c => c.CaseId == "CASE-ESC");
        parentCase.Status.Should().Be("Escalated");
        parentCase.UpdatedUtc.Should().BeAfter(parentCase.CreatedUtc.AddMilliseconds(-1));
    }

    [Fact]
    public async Task AlreadyEscalated_AddsNewEscalation()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        await SupportTestData.For(db)
            .WithCase("CASE-ESC2", "MEM-001", "Issue", "Escalated")
            .WithEscalation("CASE-ESC2", "Tier2", "LowConfidence")
            .SeedAsync();

        var handler = new EscalateHandler(db);
        var result = await handler.Handle(new EscalateCommand(
            new EscalateRequest("CASE-ESC2", null, "Tier3", "Complaint", "Member insists")),
            CancellationToken.None);

        result.EscalationId.Should().NotBeNullOrWhiteSpace();
        var escalations = await db.Escalations.Where(e => e.CaseId == "CASE-ESC2").ToListAsync();
        escalations.Should().HaveCount(2);
    }

    [Fact]
    public async Task CaseNotFound_ThrowsInvalidOperation()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        var handler = new EscalateHandler(db);

        var act = () => handler.Handle(new EscalateCommand(
            new EscalateRequest("CASE-GHOST", null, "Tier2", "LowConfidence", null)),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task InvalidReason_ThrowsMcpException()
    {
        var logger = NullLogger<SupportTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => SupportTools.EscalateAsync(mediator.Object, logger, "CASE-001", null, "Tier2", "InvalidReason", null);

        await act.Should().ThrowAsync<McpException>().WithMessage("*Invalid*reason*");
    }
}

public class FileComplaintHandlerTests
{
    [Fact]
    public async Task HappyPath_InsertsComplaint()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        await SupportTestData.For(db).WithCase("CASE-CMP", "MEM-001", "Issue", "Open").SeedAsync();

        var handler = new FileComplaintHandler(db);
        var result = await handler.Handle(new FileComplaintCommand(
            new FileComplaintRequest("CASE-CMP", "MEM-001", "BillingError", "High", false, null, "Incorrect charge", DateTime.UtcNow.AddDays(30))),
            CancellationToken.None);

        result.Status.Should().Be("Open");
        result.ComplaintId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RegulatorAgencyProvided_AutoSetsRegulatoryFlag()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        await SupportTestData.For(db).WithCase("CASE-REG", "MEM-001", "Issue", "Open").SeedAsync();

        var handler = new FileComplaintHandler(db);
        await handler.Handle(new FileComplaintCommand(
            new FileComplaintRequest("CASE-REG", "MEM-001", "ClaimDenial", "Critical", false, "CMS", "Regulatory issue", DateTime.UtcNow.AddDays(14))),
            CancellationToken.None);

        var complaint = await db.Complaints.FirstAsync(c => c.CaseId == "CASE-REG");
        complaint.RegulatoryFlag.Should().BeTrue();
        complaint.RegulatorAgency.Should().Be("CMS");
    }

    [Fact]
    public async Task InvalidSeverity_ThrowsMcpException()
    {
        var logger = NullLogger<SupportTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => SupportTools.FileComplaintAsync(mediator.Object, logger,
            "CASE-001", "MEM-001", "BillingError", "Urgent", false, null, "Desc", DateTime.UtcNow.AddDays(30));

        await act.Should().ThrowAsync<McpException>().WithMessage("*Invalid*severity*");
    }

    [Fact]
    public async Task CaseNotFound_ThrowsInvalidOperation()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        var handler = new FileComplaintHandler(db);

        var act = () => handler.Handle(new FileComplaintCommand(
            new FileComplaintRequest("CASE-GHOST", "MEM-001", "BillingError", "High", false, null, "Desc", DateTime.UtcNow.AddDays(30))),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task InvalidComplaintType_ThrowsMcpException()
    {
        var logger = NullLogger<SupportTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => SupportTools.FileComplaintAsync(mediator.Object, logger,
            "CASE-001", "MEM-001", "FraudClaim", "High", false, null, "Desc", DateTime.UtcNow.AddDays(30));

        await act.Should().ThrowAsync<McpException>().WithMessage("*Invalid*complaintType*");
    }
}

public class AuditTrailTests
{
    [Fact]
    public async Task CreateCase_PopulatesAuditFields()
    {
        using var db = SupportTestData.CreateInMemoryContext();
        var handler = new CreateCaseHandler(db);

        var result = await handler.Handle(new CreateCaseCommand(
            new CreateCaseRequest("MEM-001", "Billing", "Invoice question", "Details", "Low", "Email", "CORR-123", "USR-AGENT", "Agent")),
            CancellationToken.None);

        var entity = await db.Cases.FirstAsync(c => c.CaseId == result.CaseId);
        entity.CreatedByUserId.Should().Be("USR-AGENT");
        entity.CreatedBySource.Should().Be("Agent");
        entity.CreatedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entity.UpdatedUtc.Should().Be(entity.CreatedUtc);
    }
}

