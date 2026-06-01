using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Support;
using Shared.Infrastructure.Persistence.Support;

namespace Mcp.CustomerSupport.Application.Commands;

public sealed class EscalateHandler(SupportDbContext db) : IRequestHandler<EscalateCommand, EscalateResultDto>
{
    public async Task<EscalateResultDto> Handle(EscalateCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var escalationId = $"ESC-{Guid.NewGuid():N}"[..20];
        var now = DateTime.UtcNow;

        var parentCase = await db.Cases.FirstOrDefaultAsync(c => c.CaseId == req.CaseId, cancellationToken)
            ?? throw new InvalidOperationException($"Case '{req.CaseId}' not found.");

        parentCase.Status = "Escalated";
        parentCase.UpdatedUtc = now;

        var escalation = new Escalation
        {
            EscalationId = escalationId,
            CaseId = req.CaseId,
            FromUserId = req.FromUserId,
            ToQueue = req.ToQueue,
            Reason = req.Reason,
            ReasonDetail = req.ReasonDetail,
            Status = "Pending",
            EscalatedUtc = now
        };

        db.Escalations.Add(escalation);
        await db.SaveChangesAsync(cancellationToken);

        return new EscalateResultDto(escalationId, "Escalated", now);
    }
}
