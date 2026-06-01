using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Shared.Contracts.Support;
using Shared.Infrastructure.Persistence.Support;
using System.ComponentModel;

namespace Mcp.CustomerSupport.Tools;

[McpServerToolType]
public sealed class SupportTools
{
    private static string RedactMemberId(string memberId) =>
        memberId.Length <= 4 ? "****" : new string('*', memberId.Length - 4) + memberId[^4..];

    private static readonly string[] ValidCaseTypes = ["Inquiry", "Issue", "Complaint", "QLE", "Enrollment", "Billing", "Access"];
    private static readonly string[] ValidPriorities = ["Low", "Medium", "High", "Critical"];
    private static readonly string[] ValidChannels = ["Phone", "Chat", "Portal", "Email", "IVR"];
    private static readonly string[] ValidAuthorTypes = ["Agent", "CSR", "Member"];
    private static readonly string[] ValidDirections = ["Inbound", "Outbound"];
    private static readonly string[] ValidEscalationReasons = ["LowConfidence", "Complaint", "SLABreach", "GuardrailTriggered", "MemberRequest"];
    private static readonly string[] ValidComplaintTypes = ["ServiceQuality", "ClaimDenial", "BillingError", "AccessIssue", "ProviderIssue", "Other"];
    private static readonly string[] ValidSeverities = ["Low", "Medium", "High", "Critical"];
    private static readonly string[] ValidCreatedBySources = ["Agent", "CSR", "Member", "System"];

    private static void ValidateEnum(string value, string[] allowed, string fieldName)
    {
        if (!allowed.Contains(value, StringComparer.OrdinalIgnoreCase))
            throw new McpException($"Invalid {fieldName} '{value}'. Allowed: {string.Join(", ", allowed)}");
    }

    // --- Reads ---

    [McpServerTool, Description("Returns a case with its notes, interactions, and escalations.")]
    public static async Task<CaseDto> GetCaseAsync(
        SupportDbContext db,
        ILogger<SupportTools> logger,
        string caseId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        logger.LogInformation("GetCase for CaseId={CaseId}", caseId);

        var c = await db.Cases
            .AsNoTracking()
            .Include(x => x.CaseNotes.OrderByDescending(n => n.CreatedUtc))
            .Include(x => x.Interactions.OrderByDescending(i => i.StartedUtc))
            .Include(x => x.Escalations.OrderByDescending(e => e.EscalatedUtc))
            .FirstOrDefaultAsync(x => x.CaseId == caseId);

        if (c is null)
            throw new McpException($"Case '{caseId}' not found.");

        return new CaseDto(
            c.CaseId, c.MemberId is not null ? RedactMemberId(c.MemberId) : null,
            c.CaseType, c.Subject, c.Description, c.Status, c.Priority, c.Channel,
            c.AssignedQueue, c.SlaDueUtc, c.SlaBreached, c.CorrelationId,
            c.CreatedBySource, c.CreatedUtc, c.UpdatedUtc, c.ResolvedUtc, c.ClosedUtc,
            c.CaseNotes.Select(n => new CaseNoteDto(n.CaseNoteId, n.AuthorType, n.NoteText, n.IsInternal, n.CreatedUtc)).ToList(),
            c.Interactions.Select(i => new InteractionDto(i.InteractionId, i.Channel, i.Direction, i.HandledByAgent, i.Intent, i.Summary, i.DurationSeconds, i.StartedUtc)).ToList(),
            c.Escalations.Select(e => new EscalationDto(e.EscalationId, e.ToQueue, e.Reason, e.ReasonDetail, e.Status, e.EscalatedUtc, e.AcknowledgedUtc)).ToList());
    }

    [McpServerTool, Description("Lists all open cases for a member.")]
    public static async Task<List<CaseSummaryDto>> ListOpenByMemberAsync(
        SupportDbContext db,
        ILogger<SupportTools> logger,
        string memberId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        logger.LogInformation("ListOpenByMember for Member {MemberId}", RedactMemberId(memberId));

        return await db.Cases
            .AsNoTracking()
            .Where(c => c.MemberId == memberId && c.Status != "Closed" && c.Status != "Resolved")
            .OrderByDescending(c => c.UpdatedUtc)
            .Select(c => new CaseSummaryDto(c.CaseId, c.CaseType, c.Subject, c.Status, c.Priority, c.CreatedUtc, c.UpdatedUtc))
            .ToListAsync();
    }

    [McpServerTool, Description("Lists cases assigned to a queue.")]
    public static async Task<List<CaseSummaryDto>> ListCasesByQueueAsync(
        SupportDbContext db,
        ILogger<SupportTools> logger,
        string queueName,
        int count = 50)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        if (count is < 1 or > 200) count = 50;

        logger.LogInformation("ListCasesByQueue Queue={Queue}, Count={Count}", queueName, count);

        return await db.Cases
            .AsNoTracking()
            .Where(c => c.AssignedQueue == queueName && c.Status != "Closed")
            .OrderByDescending(c => c.UpdatedUtc)
            .Take(count)
            .Select(c => new CaseSummaryDto(c.CaseId, c.CaseType, c.Subject, c.Status, c.Priority, c.CreatedUtc, c.UpdatedUtc))
            .ToListAsync();
    }

    // --- Writes ---

    [McpServerTool, Description("Creates a new support case.")]
    public static async Task<CreateCaseResultDto> CreateCaseAsync(
        IMediator mediator,
        ILogger<SupportTools> logger,
        string memberId,
        string caseType,
        string subject,
        string description,
        string priority,
        string channel,
        string? correlationId,
        string createdBy,
        string createdBySource)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);
        ValidateEnum(caseType, ValidCaseTypes, nameof(caseType));
        ValidateEnum(priority, ValidPriorities, nameof(priority));
        ValidateEnum(channel, ValidChannels, nameof(channel));
        ValidateEnum(createdBySource, ValidCreatedBySources, nameof(createdBySource));

        logger.LogInformation("CreateCase for Member {MemberId}, Type={Type}", RedactMemberId(memberId), caseType);

        return await mediator.Send(new CreateCaseCommand(
            new CreateCaseRequest(memberId, caseType, subject, description, priority, channel, correlationId, createdBy, createdBySource)));
    }

    [McpServerTool, Description("Adds a note to a case.")]
    public static async Task<AddNoteResultDto> AddNoteAsync(
        IMediator mediator,
        ILogger<SupportTools> logger,
        string caseId,
        string authorUserId,
        string authorType,
        string noteText,
        bool isInternal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(authorUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(noteText);
        ValidateEnum(authorType, ValidAuthorTypes, nameof(authorType));

        logger.LogInformation("AddNote to CaseId={CaseId}, Author={AuthorType}", caseId, authorType);

        return await mediator.Send(new AddNoteCommand(new AddNoteRequest(caseId, authorUserId, authorType, noteText, isInternal)));
    }

    [McpServerTool, Description("Logs an interaction (phone, chat, etc.) for a member, optionally linked to a case.")]
    public static async Task<LogInteractionResultDto> LogInteractionAsync(
        IMediator mediator,
        ILogger<SupportTools> logger,
        string memberId,
        string? caseId,
        string channel,
        string direction,
        string? intent,
        string? summary,
        string? sessionId,
        int? durationSec)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        ValidateEnum(channel, ValidChannels, nameof(channel));
        ValidateEnum(direction, ValidDirections, nameof(direction));

        logger.LogInformation("LogInteraction for Member {MemberId}, Channel={Channel}", RedactMemberId(memberId), channel);

        return await mediator.Send(new LogInteractionCommand(
            new LogInteractionRequest(memberId, caseId, channel, direction, intent, summary, sessionId, durationSec)));
    }

    [McpServerTool, Description("Escalates a case to a queue. Updates case status to 'Escalated'.")]
    public static async Task<EscalateResultDto> EscalateAsync(
        IMediator mediator,
        ILogger<SupportTools> logger,
        string caseId,
        string? fromUserId,
        string toQueue,
        string reason,
        string? reasonDetail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(toQueue);
        ValidateEnum(reason, ValidEscalationReasons, nameof(reason));

        logger.LogInformation("Escalate CaseId={CaseId}, ToQueue={Queue}, Reason={Reason}", caseId, toQueue, reason);

        return await mediator.Send(new EscalateCommand(new EscalateRequest(caseId, fromUserId, toQueue, reason, reasonDetail)));
    }

    [McpServerTool, Description("Files a formal complaint against a case. Auto-sets RegulatoryFlag when RegulatorAgency is provided.")]
    public static async Task<FileComplaintResultDto> FileComplaintAsync(
        IMediator mediator,
        ILogger<SupportTools> logger,
        string caseId,
        string memberId,
        string complaintType,
        string severity,
        bool regulatoryFlag,
        string? regulatorAgency,
        string description,
        DateTime dueUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ValidateEnum(complaintType, ValidComplaintTypes, nameof(complaintType));
        ValidateEnum(severity, ValidSeverities, nameof(severity));

        logger.LogInformation("FileComplaint for CaseId={CaseId}, Member {MemberId}, Type={Type}",
            caseId, RedactMemberId(memberId), complaintType);

        return await mediator.Send(new FileComplaintCommand(
            new FileComplaintRequest(caseId, memberId, complaintType, severity, regulatoryFlag, regulatorAgency, description, dueUtc)));
    }
}
