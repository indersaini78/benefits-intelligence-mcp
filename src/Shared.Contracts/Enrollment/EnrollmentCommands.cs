using MediatR;

namespace Shared.Contracts.Enrollment;

public sealed record CreateEnrollmentCommand(CreateEnrollmentRequest Request) : IRequest<EnrollmentResultDto>;

public sealed record TerminateCoverageCommand(TerminateCoverageRequest Request) : IRequest<EnrollmentResultDto>;

public sealed record ChangePlanCommand(ChangePlanRequest Request) : IRequest<EnrollmentResultDto>;

public sealed record AddDependentCommand(AddDependentRequest Request) : IRequest<DependentResultDto>;
