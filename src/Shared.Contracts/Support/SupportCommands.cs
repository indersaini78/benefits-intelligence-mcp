using MediatR;

namespace Shared.Contracts.Support;

public sealed record CreateCaseCommand(CreateCaseRequest Request) : IRequest<CreateCaseResultDto>;
public sealed record AddNoteCommand(AddNoteRequest Request) : IRequest<AddNoteResultDto>;
public sealed record LogInteractionCommand(LogInteractionRequest Request) : IRequest<LogInteractionResultDto>;
public sealed record EscalateCommand(EscalateRequest Request) : IRequest<EscalateResultDto>;
public sealed record FileComplaintCommand(FileComplaintRequest Request) : IRequest<FileComplaintResultDto>;
