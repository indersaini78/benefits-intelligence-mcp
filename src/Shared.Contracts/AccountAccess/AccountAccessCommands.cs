using MediatR;

namespace Shared.Contracts.AccountAccess;

public sealed record UnlockAccountCommand(UnlockAccountRequest Request) : IRequest<UnlockAccountResultDto>;

public sealed record InitiatePasswordResetCommand(InitiatePasswordResetRequest Request) : IRequest<PasswordResetResultDto>;

public sealed record VerifyIdentityCommand(VerifyIdentityRequest Request) : IRequest<VerifyIdentityResultDto>;
