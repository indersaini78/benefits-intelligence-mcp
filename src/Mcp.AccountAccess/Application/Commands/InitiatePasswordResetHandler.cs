using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.AccountAccess;
using Shared.Infrastructure.Persistence.Iam;

namespace Mcp.AccountAccess.Application.Commands;

public sealed class InitiatePasswordResetHandler(IamDbContext db) : IRequestHandler<InitiatePasswordResetCommand, PasswordResetResultDto>
{
    public async Task<PasswordResetResultDto> Handle(InitiatePasswordResetCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;

        var user = await db.UserAccounts.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == req.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"User '{req.UserId}' not found.");

        var resetId = $"RST-{Guid.NewGuid():N}"[..20];
        var now = DateTime.UtcNow;

        var resetRequest = new PasswordResetRequest
        {
            ResetRequestId = resetId,
            UserId = req.UserId,
            RequestedUtc = now,
            Channel = req.Channel,
            VerificationMethod = req.VerificationMethod,
            Status = "Pending",
            ExpiresUtc = now.AddHours(1),
            InitiatedBy = "MCP-Agent"
        };

        db.PasswordResetRequests.Add(resetRequest);

        // Note: production system would emit to audit topic
        await db.SaveChangesAsync(cancellationToken);

        return new PasswordResetResultDto(resetId, "Pending", resetRequest.ExpiresUtc);
    }
}
