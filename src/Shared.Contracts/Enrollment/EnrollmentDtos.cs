namespace Shared.Contracts.Enrollment;

// Request records
public sealed record CreateEnrollmentRequest(
    string MemberId,
    string TransactionType,
    DateOnly EffectiveDate,
    string ElectionsJson,
    string Channel);

public sealed record TerminateCoverageRequest(
    string MemberId,
    DateOnly TerminationDate,
    string Reason);

public sealed record ChangePlanRequest(
    string MemberId,
    string FromPlanId,
    string ToPlanId,
    DateOnly EffectiveDate);

public sealed record AddDependentRequest(
    string MemberId,
    string FirstName,
    string LastName,
    string Relationship,
    DateOnly DOB,
    string? Gender);

// Response records
public sealed record EnrollmentResultDto(
    string EnrollmentId,
    string Status,
    DateTime CreatedUtc);

public sealed record EnrollmentStatusDto(
    string EnrollmentId,
    string MemberId,
    string TransactionType,
    string Status,
    DateOnly EffectiveDate,
    string SubmittedChannel,
    DateTime CreatedUtc,
    DateTime? CompletedUtc,
    string? ErrorMessage,
    List<PlanElectionDto> Elections);

public sealed record PlanElectionDto(
    string ElectionId,
    string PlanId,
    string Tier,
    DateOnly EffectiveDate,
    string Action);

public sealed record DependentResultDto(
    string DependentId,
    string MemberId,
    string FirstName,
    string LastName,
    string Relationship,
    DateOnly DOB,
    bool IsActive);
