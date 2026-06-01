using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Support;
using Shared.Infrastructure.Persistence.Support;

namespace Mcp.CustomerSupport.Application.Commands;

public sealed class FileComplaintHandler(SupportDbContext db) : IRequestHandler<FileComplaintCommand, FileComplaintResultDto>
{
    public async Task<FileComplaintResultDto> Handle(FileComplaintCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var complaintId = $"CMP-{Guid.NewGuid():N}"[..20];
        var now = DateTime.UtcNow;

        var caseExists = await db.Cases.AnyAsync(c => c.CaseId == req.CaseId, cancellationToken);
        if (!caseExists)
            throw new InvalidOperationException($"Case '{req.CaseId}' not found.");

        // Auto-set RegulatoryFlag when RegulatorAgency is provided
        var regulatoryFlag = req.RegulatoryFlag || !string.IsNullOrWhiteSpace(req.RegulatorAgency);

        var complaint = new Complaint
        {
            ComplaintId = complaintId,
            CaseId = req.CaseId,
            MemberId = req.MemberId,
            ComplaintType = req.ComplaintType,
            Severity = req.Severity,
            RegulatoryFlag = regulatoryFlag,
            RegulatorAgency = req.RegulatorAgency,
            Description = req.Description,
            Status = "Open",
            FiledUtc = now,
            DueUtc = req.DueUtc
        };

        db.Complaints.Add(complaint);
        await db.SaveChangesAsync(cancellationToken);

        return new FileComplaintResultDto(complaintId, "Open", now);
    }
}
