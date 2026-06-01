using MediatR;
using Shared.Contracts.Support;
using Shared.Infrastructure.Persistence.Support;

namespace Mcp.CustomerSupport.Application.Commands;

public sealed class CreateCaseHandler(SupportDbContext db) : IRequestHandler<CreateCaseCommand, CreateCaseResultDto>
{
    public async Task<CreateCaseResultDto> Handle(CreateCaseCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var caseId = $"CAS-{Guid.NewGuid():N}"[..20];
        var now = DateTime.UtcNow;

        var entity = new Case
        {
            CaseId = caseId,
            MemberId = req.MemberId,
            CaseType = req.CaseType,
            Subject = req.Subject,
            Description = req.Description,
            Status = "New",
            Priority = req.Priority,
            Channel = req.Channel,
            CorrelationId = req.CorrelationId,
            CreatedByUserId = req.CreatedBy,
            CreatedBySource = req.CreatedBySource,
            CreatedUtc = now,
            UpdatedUtc = now
        };

        db.Cases.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new CreateCaseResultDto(caseId, "New", now);
    }
}
