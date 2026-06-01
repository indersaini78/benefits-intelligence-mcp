using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.AccountAccess;
using Shared.Infrastructure.Persistence.Iam;

namespace Mcp.AccountAccess.Application.Commands;

public sealed class UnlockAccountHandler(IamDbContext db) : IRequestHandler<UnlockAccountCommand, UnlockAccountResultDto>
{
    public async Task<UnlockAccountResultDto> Handle(UnlockAccountCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;

        var user = await db.UserAccounts.FirstOrDefaultAsync(u => u.UserId == req.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"User '{req.UserId}' not found.");

        if (user.Status != "Locked")
            throw new InvalidOperationException($"User '{req.UserId}' is not locked (current status: {user.Status}).");

        user.Status = "Active";
        user.FailedLoginCount = 0;
        user.LockedUntilUtc = null;

        // Note: production system would emit to audit topic
        await db.SaveChangesAsync(cancellationToken);

        return new UnlockAccountResultDto(req.UserId, "Active", $"Account unlocked. Reason: {req.ReasonText}");
    }
}
