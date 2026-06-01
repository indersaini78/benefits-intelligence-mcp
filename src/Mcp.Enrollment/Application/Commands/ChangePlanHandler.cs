using System.Text.Json;
using MediatR;
using Shared.Contracts.Enrollment;
using Shared.Infrastructure.Persistence.Enrollment;

namespace Mcp.Enrollment.Application.Commands;

public sealed class ChangePlanHandler(EnrollmentDbContext db) : IRequestHandler<ChangePlanCommand, EnrollmentResultDto>
{
    public async Task<EnrollmentResultDto> Handle(ChangePlanCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var enrollmentId = $"ENR-{Guid.NewGuid():N}"[..20];
        var now = DateTime.UtcNow;

        var transaction = new EnrollmentTransaction
        {
            EnrollmentId = enrollmentId,
            MemberId = req.MemberId,
            GroupPolicyId = "DEFAULT",
            TransactionType = "PlanChange",
            Status = "Completed",
            EffectiveDate = req.EffectiveDate,
            SubmittedBy = "MCP-Agent",
            SubmittedChannel = "MCP",
            CorrelationId = Guid.NewGuid().ToString(),
            RequestPayloadJson = JsonSerializer.Serialize(req),
            CreatedUtc = now,
            CompletedUtc = now
        };

        db.EnrollmentTransactions.Add(transaction);

        // Drop old plan
        db.PlanElections.Add(new PlanElection
        {
            ElectionId = $"ELC-{Guid.NewGuid():N}"[..20],
            EnrollmentId = enrollmentId,
            PlanId = req.FromPlanId,
            Tier = "Current",
            EffectiveDate = req.EffectiveDate,
            Action = "Drop"
        });

        // Add new plan
        db.PlanElections.Add(new PlanElection
        {
            ElectionId = $"ELC-{Guid.NewGuid():N}"[..20],
            EnrollmentId = enrollmentId,
            PlanId = req.ToPlanId,
            Tier = "Current",
            EffectiveDate = req.EffectiveDate,
            Action = "Add"
        });

        db.OutboxEvents.Add(new OutboxEvent
        {
            AggregateId = enrollmentId,
            EventType = "PlanChanged",
            PayloadJson = JsonSerializer.Serialize(new { enrollmentId, req.MemberId, req.FromPlanId, req.ToPlanId, req.EffectiveDate }),
            CreatedUtc = now
        });

        await db.SaveChangesAsync(cancellationToken);

        return new EnrollmentResultDto(enrollmentId, transaction.Status, now);
    }
}
