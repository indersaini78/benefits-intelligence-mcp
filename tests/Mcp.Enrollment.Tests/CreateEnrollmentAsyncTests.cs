using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Mcp.Enrollment.Application.Commands;
using Mcp.Enrollment.Tests.Helpers;
using Mcp.Enrollment.Tools;
using ModelContextProtocol;
using Moq;
using MediatR;
using Shared.Contracts.Enrollment;
using Shared.Infrastructure.Persistence.Enrollment;

namespace Mcp.Enrollment.Tests;

public class CreateEnrollmentAsyncTests
{
    private static readonly string ValidElectionsJson =
        JsonSerializer.Serialize(new[] { new { PlanId = "PLAN-MED-001", Tier = "Employee+Family", Action = "Add" } });

    [Fact]
    public async Task HappyPath_NewHire_ReturnsEnrollmentId_And_WritesAllRows()
    {
        // Arrange
        using var db = EnrollmentTestData.CreateInMemoryContext();
        await EnrollmentTestData.For(db)
            .WithMember("MBR-001", "GRP-001", new DateOnly(2024, 1, 1))
            .SeedAsync();

        var handler = new CreateEnrollmentHandler(db);
        var request = new CreateEnrollmentRequest("MBR-001", "NewHire", new DateOnly(2025, 2, 1), ValidElectionsJson, "Portal");

        // Act
        var result = await handler.Handle(new CreateEnrollmentCommand(request), CancellationToken.None);

        // Assert
        result.EnrollmentId.Should().NotBeNullOrWhiteSpace();
        result.Status.Should().Be("Completed");

        var transaction = await db.EnrollmentTransactions.FirstAsync(t => t.EnrollmentId == result.EnrollmentId);
        transaction.TransactionType.Should().Be("NewHire");
        transaction.MemberId.Should().Be("MBR-001");

        var elections = await db.PlanElections.Where(e => e.EnrollmentId == result.EnrollmentId).ToListAsync();
        elections.Should().HaveCount(1);
        elections[0].PlanId.Should().Be("PLAN-MED-001");
        elections[0].Action.Should().Be("Add");

        var outbox = await db.OutboxEvents.FirstAsync(o => o.AggregateId == result.EnrollmentId);
        outbox.EventType.Should().Be("EnrollmentCreated");

        // Verify PayloadJson is valid JSON
        var act = () => JsonDocument.Parse(outbox.PayloadJson);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task MissingElectionsJson_ThrowsArgumentException()
    {
        using var db = EnrollmentTestData.CreateInMemoryContext();
        var logger = NullLogger<EnrollmentTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => EnrollmentTools.CreateEnrollmentAsync(mediator.Object, logger, "MBR-001", "NewHire", new DateOnly(2025, 1, 1), "", "Portal");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task InvalidTransactionType_InputValidation_ThrowsArgumentException()
    {
        using var db = EnrollmentTestData.CreateInMemoryContext();
        var logger = NullLogger<EnrollmentTools>.Instance;
        var mediator = new Mock<IMediator>();

        // Tool-level validation catches null/empty, handler currently does not validate enum values
        var act = () => EnrollmentTools.CreateEnrollmentAsync(mediator.Object, logger, "MBR-001", " ", new DateOnly(2025, 1, 1), ValidElectionsJson, "Portal");

        await act.Should().ThrowAsync<ArgumentException>();
    }
}

public class TerminateCoverageAsyncTests
{
    [Fact]
    public async Task HappyPath_CreatesTerminationTransaction_And_OutboxEvent()
    {
        using var db = EnrollmentTestData.CreateInMemoryContext();
        await EnrollmentTestData.For(db)
            .WithMember("MBR-002", "GRP-001", new DateOnly(2023, 6, 1))
            .WithCoverage("MBR-002", "PLAN-MED-001", "Employee")
            .SeedAsync();

        var handler = new TerminateCoverageHandler(db);
        var request = new TerminateCoverageRequest("MBR-002", new DateOnly(2025, 7, 1), "Voluntary resignation");

        var result = await handler.Handle(new TerminateCoverageCommand(request), CancellationToken.None);

        result.EnrollmentId.Should().NotBeNullOrWhiteSpace();
        result.Status.Should().Be("Completed");

        var transaction = await db.EnrollmentTransactions.FirstAsync(t => t.EnrollmentId == result.EnrollmentId);
        transaction.TransactionType.Should().Be("Termination");

        var outbox = await db.OutboxEvents.FirstAsync(o => o.AggregateId == result.EnrollmentId);
        outbox.EventType.Should().Be("CoverageTerminated");
        var act = () => JsonDocument.Parse(outbox.PayloadJson);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task EmptyMemberId_ThrowsArgumentException()
    {
        var logger = NullLogger<EnrollmentTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => EnrollmentTools.TerminateCoverageAsync(mediator.Object, logger, "", new DateOnly(2025, 7, 1), "reason");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task EmptyReason_ThrowsArgumentException()
    {
        var logger = NullLogger<EnrollmentTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => EnrollmentTools.TerminateCoverageAsync(mediator.Object, logger, "MBR-001", new DateOnly(2025, 7, 1), "");

        await act.Should().ThrowAsync<ArgumentException>();
    }
}

public class ChangePlanAsyncTests
{
    [Fact]
    public async Task HappyPath_CreatesPlanChangeWith2Elections_And_OutboxEvent()
    {
        using var db = EnrollmentTestData.CreateInMemoryContext();
        await EnrollmentTestData.For(db)
            .WithMember("MBR-003", "GRP-001", new DateOnly(2023, 1, 1))
            .WithCoverage("MBR-003", "PLAN-MED-001", "Employee")
            .SeedAsync();

        var handler = new ChangePlanHandler(db);
        var request = new ChangePlanRequest("MBR-003", "PLAN-MED-001", "PLAN-MED-002", new DateOnly(2025, 3, 1));

        var result = await handler.Handle(new ChangePlanCommand(request), CancellationToken.None);

        result.EnrollmentId.Should().NotBeNullOrWhiteSpace();
        result.Status.Should().Be("Completed");

        var elections = await db.PlanElections.Where(e => e.EnrollmentId == result.EnrollmentId).ToListAsync();
        elections.Should().HaveCount(2);
        elections.Should().Contain(e => e.PlanId == "PLAN-MED-001" && e.Action == "Drop");
        elections.Should().Contain(e => e.PlanId == "PLAN-MED-002" && e.Action == "Add");

        var outbox = await db.OutboxEvents.FirstAsync(o => o.AggregateId == result.EnrollmentId);
        outbox.EventType.Should().Be("PlanChanged");
        var act = () => JsonDocument.Parse(outbox.PayloadJson);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task EmptyFromPlanId_ThrowsArgumentException()
    {
        var logger = NullLogger<EnrollmentTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => EnrollmentTools.ChangePlanAsync(mediator.Object, logger, "MBR-001", "", "PLAN-002", new DateOnly(2025, 1, 1));

        await act.Should().ThrowAsync<ArgumentException>();
    }
}

public class GetEnrollmentStatusAsyncTests
{
    [Fact]
    public async Task ExistingEnrollment_ReturnsDto()
    {
        using var db = EnrollmentTestData.CreateInMemoryContext();
        await EnrollmentTestData.For(db)
            .WithMember("MBR-004", "GRP-001", new DateOnly(2023, 1, 1))
            .WithEnrollmentTransaction("ENR-TEST-00000001", "MBR-004", "NewHire", "Completed")
            .SeedAsync();

        var logger = NullLogger<EnrollmentTools>.Instance;

        var result = await EnrollmentTools.GetEnrollmentStatusAsync(db, logger, "ENR-TEST-00000001");

        result.EnrollmentId.Should().Be("ENR-TEST-00000001");
        result.TransactionType.Should().Be("NewHire");
        result.Status.Should().Be("Completed");
        // MemberId should be redacted
        result.MemberId.Should().EndWith("-004");
        result.MemberId.Should().Contain("*");
    }

    [Fact]
    public async Task NonExistentEnrollment_ThrowsMcpException()
    {
        using var db = EnrollmentTestData.CreateInMemoryContext();
        var logger = NullLogger<EnrollmentTools>.Instance;

        var act = () => EnrollmentTools.GetEnrollmentStatusAsync(db, logger, "ENR-DOESNOTEXIST");

        await act.Should().ThrowAsync<McpException>();
    }
}

public class AddDependentAsyncTests
{
    [Fact]
    public async Task HappyPath_CreatesDependent_And_EnrollmentTransaction_And_OutboxEvent()
    {
        using var db = EnrollmentTestData.CreateInMemoryContext();
        await EnrollmentTestData.For(db)
            .WithMember("MBR-005", "GRP-001", new DateOnly(2023, 1, 1))
            .SeedAsync();

        var handler = new AddDependentHandler(db);
        var request = new AddDependentRequest("MBR-005", "Jane", "Doe", "Spouse", new DateOnly(1992, 5, 15), "F");

        var result = await handler.Handle(new AddDependentCommand(request), CancellationToken.None);

        result.DependentId.Should().NotBeNullOrWhiteSpace();
        result.MemberId.Should().Be("MBR-005");
        result.FirstName.Should().Be("Jane");
        result.IsActive.Should().BeTrue();

        var transaction = await db.EnrollmentTransactions.FirstAsync(t => t.MemberId == "MBR-005" && t.TransactionType == "AddDependent");
        transaction.Status.Should().Be("Completed");

        var outbox = await db.OutboxEvents.FirstAsync(o => o.EventType == "DependentAdded");
        var act = () => JsonDocument.Parse(outbox.PayloadJson);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task EmptyMemberId_ThrowsArgumentException()
    {
        var logger = NullLogger<EnrollmentTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => EnrollmentTools.AddDependentAsync(mediator.Object, logger, "", "Jane", "Doe", "Spouse", new DateOnly(1992, 1, 1));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task EmptyRelationship_ThrowsArgumentException()
    {
        var logger = NullLogger<EnrollmentTools>.Instance;
        var mediator = new Mock<IMediator>();

        var act = () => EnrollmentTools.AddDependentAsync(mediator.Object, logger, "MBR-005", "Jane", "Doe", "", new DateOnly(1992, 1, 1));

        await act.Should().ThrowAsync<ArgumentException>();
    }
}

