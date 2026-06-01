using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Support;
using Shared.Infrastructure.Persistence.Support;

namespace Mcp.CustomerSupport.Application.Commands;

public sealed class AddNoteHandler(SupportDbContext db) : IRequestHandler<AddNoteCommand, AddNoteResultDto>
{
    public async Task<AddNoteResultDto> Handle(AddNoteCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var now = DateTime.UtcNow;

        var caseExists = await db.Cases.AnyAsync(c => c.CaseId == req.CaseId, cancellationToken);
        if (!caseExists)
            throw new InvalidOperationException($"Case '{req.CaseId}' not found.");

        var note = new CaseNote
        {
            CaseId = req.CaseId,
            AuthorUserId = req.AuthorUserId,
            AuthorType = req.AuthorType,
            NoteText = req.NoteText,
            IsInternal = req.IsInternal,
            CreatedUtc = now
        };

        db.CaseNotes.Add(note);
        await db.SaveChangesAsync(cancellationToken);

        return new AddNoteResultDto(note.CaseNoteId, now);
    }
}
