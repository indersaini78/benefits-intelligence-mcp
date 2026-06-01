using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.AccountAccess;
using Shared.Infrastructure.Persistence.Iam;

namespace Mcp.AccountAccess.Application.Commands;

public sealed class VerifyIdentityHandler(IamDbContext db) : IRequestHandler<VerifyIdentityCommand, VerifyIdentityResultDto>
{
    public async Task<VerifyIdentityResultDto> Handle(VerifyIdentityCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;

        var user = await db.UserAccounts
            .AsNoTracking()
            .Include(u => u.Member)
            .FirstOrDefaultAsync(u => u.UserId == req.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"User '{req.UserId}' not found.");

        if (user.Member is null)
            return new VerifyIdentityResultDto(false);

        var dobMatch = user.Member.DOB == DateOnly.ParseExact(req.DobYYYYMMDD, "yyyyMMdd");
        var ssnMatch = string.Equals(user.Member.SsnLast4, req.SsnLast4, StringComparison.Ordinal);

        // Note: production system would emit to audit topic
        return new VerifyIdentityResultDto(dobMatch && ssnMatch);
    }
}
