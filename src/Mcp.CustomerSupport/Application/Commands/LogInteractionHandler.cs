using MediatR;
using Shared.Contracts.Support;
using Shared.Infrastructure.Persistence.Support;

namespace Mcp.CustomerSupport.Application.Commands;

public sealed class LogInteractionHandler(SupportDbContext db) : IRequestHandler<LogInteractionCommand, LogInteractionResultDto>
{
    public async Task<LogInteractionResultDto> Handle(LogInteractionCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var interactionId = $"INT-{Guid.NewGuid():N}"[..20];
        var now = DateTime.UtcNow;

        var interaction = new Interaction
        {
            InteractionId = interactionId,
            MemberId = req.MemberId,
            CaseId = req.CaseId,
            Channel = req.Channel,
            Direction = req.Direction,
            HandledByAgent = true,
            SessionId = req.SessionId,
            Intent = req.Intent,
            Summary = req.Summary,
            DurationSeconds = req.DurationSec,
            StartedUtc = now
        };

        db.Interactions.Add(interaction);
        await db.SaveChangesAsync(cancellationToken);

        return new LogInteractionResultDto(interactionId, now);
    }
}
