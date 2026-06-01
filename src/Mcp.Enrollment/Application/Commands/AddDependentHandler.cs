using System.Text.Json;
using MediatR;
using Shared.Contracts.Enrollment;
using Shared.Infrastructure.Persistence.Enrollment;

namespace Mcp.Enrollment.Application.Commands;

public sealed class AddDependentHandler(EnrollmentDbContext db) : IRequestHandler<AddDependentCommand, DependentResultDto>
{
    public async Task<DependentResultDto> Handle(AddDependentCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var dependentId = $"DEP-{Guid.NewGuid():N}"[..20];
        var enrollmentId = $"ENR-{Guid.NewGuid():N}"[..20];
        var now = DateTime.UtcNow;

        var dependent = new Dependent
        {
            DependentId = dependentId,
            MemberId = req.MemberId,
            FirstName = req.FirstName,
            LastName = req.LastName,
            Relationship = req.Relationship,
            DOB = req.DOB,
            Gender = req.Gender,
            IsActive = true
        };

        db.Dependents.Add(dependent);

        // Enrollment transaction for the dependent add
        var transaction = new EnrollmentTransaction
        {
            EnrollmentId = enrollmentId,
            MemberId = req.MemberId,
            GroupPolicyId = "DEFAULT",
            TransactionType = "AddDependent",
            Status = "Completed",
            EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
            SubmittedBy = "MCP-Agent",
            SubmittedChannel = "MCP",
            CorrelationId = Guid.NewGuid().ToString(),
            RequestPayloadJson = JsonSerializer.Serialize(req),
            CreatedUtc = now,
            CompletedUtc = now
        };

        db.EnrollmentTransactions.Add(transaction);

        db.PlanElections.Add(new PlanElection
        {
            ElectionId = $"ELC-{Guid.NewGuid():N}"[..20],
            EnrollmentId = enrollmentId,
            PlanId = "DEPENDENT-ADD",
            Tier = req.Relationship,
            EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Action = "Add"
        });

        db.OutboxEvents.Add(new OutboxEvent
        {
            AggregateId = enrollmentId,
            EventType = "DependentAdded",
            PayloadJson = JsonSerializer.Serialize(new { enrollmentId, dependentId, req.MemberId, req.FirstName, req.LastName }),
            CreatedUtc = now
        });

        await db.SaveChangesAsync(cancellationToken);

        return new DependentResultDto(dependentId, req.MemberId, req.FirstName, req.LastName, req.Relationship, req.DOB, true);
    }
}
