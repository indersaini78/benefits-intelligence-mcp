namespace Shared.Contracts.Support;

// Read DTOs
public sealed record CaseDto(
    string CaseId,
    string? MemberId,
    string CaseType,
    string Subject,
    string? Description,
    string Status,
    string Priority,
    string Channel,
    string? AssignedQueue,
    DateTime? SlaDueUtc,
    bool SlaBreached,
    string? CorrelationId,
    string CreatedBySource,
    DateTime CreatedUtc,
    DateTime UpdatedUtc,
    DateTime? ResolvedUtc,
    DateTime? ClosedUtc,
    List<CaseNoteDto> Notes,
    List<InteractionDto> Interactions,
    List<EscalationDto> Escalations);

public sealed record CaseSummaryDto(
    string CaseId,
    string CaseType,
    string Subject,
    string Status,
    string Priority,
    DateTime CreatedUtc,
    DateTime UpdatedUtc);

public sealed record CaseNoteDto(
    long CaseNoteId,
    string AuthorType,
    string NoteText,
    bool IsInternal,
    DateTime CreatedUtc);

public sealed record InteractionDto(
    string InteractionId,
    string Channel,
    string Direction,
    bool HandledByAgent,
    string? Intent,
    string? Summary,
    int? DurationSeconds,
    DateTime StartedUtc);

public sealed record EscalationDto(
    string EscalationId,
    string ToQueue,
    string Reason,
    string? ReasonDetail,
    string Status,
    DateTime EscalatedUtc,
    DateTime? AcknowledgedUtc);

// Write requests/responses
public sealed record CreateCaseRequest(
    string MemberId,
    string CaseType,
    string Subject,
    string Description,
    string Priority,
    string Channel,
    string? CorrelationId,
    string CreatedBy,
    string CreatedBySource);

public sealed record CreateCaseResultDto(string CaseId, string Status, DateTime CreatedUtc);

public sealed record AddNoteRequest(string CaseId, string AuthorUserId, string AuthorType, string NoteText, bool IsInternal);
public sealed record AddNoteResultDto(long CaseNoteId, DateTime CreatedUtc);

public sealed record LogInteractionRequest(
    string MemberId,
    string? CaseId,
    string Channel,
    string Direction,
    string? Intent,
    string? Summary,
    string? SessionId,
    int? DurationSec);
public sealed record LogInteractionResultDto(string InteractionId, DateTime StartedUtc);

public sealed record EscalateRequest(string CaseId, string? FromUserId, string ToQueue, string Reason, string? ReasonDetail);
public sealed record EscalateResultDto(string EscalationId, string CaseStatus, DateTime EscalatedUtc);

public sealed record FileComplaintRequest(
    string CaseId,
    string MemberId,
    string ComplaintType,
    string Severity,
    bool RegulatoryFlag,
    string? RegulatorAgency,
    string Description,
    DateTime DueUtc);
public sealed record FileComplaintResultDto(string ComplaintId, string Status, DateTime FiledUtc);
