using System.Text.Json;
using MediatR;
using Shared.Contracts.Enrollment;
using Shared.Infrastructure.Persistence.Enrollment;

namespace Mcp.Enrollment.Application.Commands;

public sealed class CreateEnrollmentHandler(EnrollmentDbContext db) : IRequestHandler<CreateEnrollmentCommand, EnrollmentResultDto>
{
    public async Task<EnrollmentResultDto> Handle(CreateEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var enrollmentId = $"ENR-{Guid.NewGuid():N}"[..20];
        var now = DateTime.UtcNow;

        var transaction = new EnrollmentTransaction
        {
            EnrollmentId = enrollmentId,
            MemberId = req.MemberId,
            GroupPolicyId = "DEFAULT",
            TransactionType = req.TransactionType,
            Status = "Completed",
            EffectiveDate = req.EffectiveDate,
            SubmittedBy = "MCP-Agent",
            SubmittedChannel = req.Channel,
            CorrelationId = Guid.NewGuid().ToString(),
            RequestPayloadJson = JsonSerializer.Serialize(req),
            CreatedUtc = now,
            CompletedUtc = now
        };

        db.EnrollmentTransactions.Add(transaction);

        // Parse elections JSON and create PlanElection rows
        var elections = JsonSerializer.Deserialize<List<ElectionInput>>(req.ElectionsJson) ?? [];
        foreach (var e in elections)
        {
            db.PlanElections.Add(new PlanElection
            {
                ElectionId = $"ELC-{Guid.NewGuid():N}"[..20],
                EnrollmentId = enrollmentId,
                PlanId = e.PlanId,
                Tier = e.Tier,
                EffectiveDate = req.EffectiveDate,
                Action = e.Action
            });
        }

        // Outbox event
        db.OutboxEvents.Add(new OutboxEvent
        {
            AggregateId = enrollmentId,
            EventType = "EnrollmentCreated",
            PayloadJson = JsonSerializer.Serialize(new { enrollmentId, req.MemberId, req.TransactionType, req.EffectiveDate }),
            CreatedUtc = now
        });

        await db.SaveChangesAsync(cancellationToken);

        return new EnrollmentResultDto(enrollmentId, transaction.Status, now);
    }

    private sealed record ElectionInput(string PlanId, string Tier, string Action);
}
