using System.Text.Json;
using MediatR;
using Shared.Contracts.Enrollment;
using Shared.Infrastructure.Persistence.Enrollment;

namespace Mcp.Enrollment.Application.Commands;

public sealed class TerminateCoverageHandler(EnrollmentDbContext db) : IRequestHandler<TerminateCoverageCommand, EnrollmentResultDto>
{
    public async Task<EnrollmentResultDto> Handle(TerminateCoverageCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var enrollmentId = $"ENR-{Guid.NewGuid():N}"[..20];
        var now = DateTime.UtcNow;

        var transaction = new EnrollmentTransaction
        {
            EnrollmentId = enrollmentId,
            MemberId = req.MemberId,
            GroupPolicyId = "DEFAULT",
            TransactionType = "Termination",
            Status = "Completed",
            EffectiveDate = req.TerminationDate,
            SubmittedBy = "MCP-Agent",
            SubmittedChannel = "MCP",
            CorrelationId = Guid.NewGuid().ToString(),
            RequestPayloadJson = JsonSerializer.Serialize(req),
            CreatedUtc = now,
            CompletedUtc = now
        };

        db.EnrollmentTransactions.Add(transaction);

        // PlanElection row for the termination
        db.PlanElections.Add(new PlanElection
        {
            ElectionId = $"ELC-{Guid.NewGuid():N}"[..20],
            EnrollmentId = enrollmentId,
            PlanId = "ALL",
            Tier = "N/A",
            EffectiveDate = req.TerminationDate,
            Action = "Drop"
        });

        db.OutboxEvents.Add(new OutboxEvent
        {
            AggregateId = enrollmentId,
            EventType = "CoverageTerminated",
            PayloadJson = JsonSerializer.Serialize(new { enrollmentId, req.MemberId, req.TerminationDate, req.Reason }),
            CreatedUtc = now
        });

        await db.SaveChangesAsync(cancellationToken);

        return new EnrollmentResultDto(enrollmentId, transaction.Status, now);
    }
}
