using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Shared.Contracts.Enrollment;
using Shared.Infrastructure.Persistence.Enrollment;
using System.ComponentModel;

namespace Mcp.Enrollment.Tools;

[McpServerToolType]
public sealed class EnrollmentTools
{
    private static string RedactMemberId(string memberId) =>
        memberId.Length <= 4 ? "****" : new string('*', memberId.Length - 4) + memberId[^4..];

    [McpServerTool, Description("Creates a new enrollment transaction with plan elections.")]
    public static async Task<EnrollmentResultDto> CreateEnrollmentAsync(
        IMediator mediator,
        ILogger<EnrollmentTools> logger,
        string memberId,
        string transactionType,
        DateOnly effectiveDate,
        string electionsJson,
        string channel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionType);
        ArgumentException.ThrowIfNullOrWhiteSpace(electionsJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(channel);

        logger.LogInformation("CreateEnrollment for Member {MemberId}, Type={Type}",
            RedactMemberId(memberId), transactionType);

        var request = new CreateEnrollmentRequest(memberId, transactionType, effectiveDate, electionsJson, channel);
        return await mediator.Send(new CreateEnrollmentCommand(request));
    }

    [McpServerTool, Description("Terminates all coverage for a member as of a given date.")]
    public static async Task<EnrollmentResultDto> TerminateCoverageAsync(
        IMediator mediator,
        ILogger<EnrollmentTools> logger,
        string memberId,
        DateOnly terminationDate,
        string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        logger.LogInformation("TerminateCoverage for Member {MemberId}, Date={Date}, Reason={Reason}",
            RedactMemberId(memberId), terminationDate, reason);

        var request = new TerminateCoverageRequest(memberId, terminationDate, reason);
        return await mediator.Send(new TerminateCoverageCommand(request));
    }

    [McpServerTool, Description("Changes a member's plan from one to another.")]
    public static async Task<EnrollmentResultDto> ChangePlanAsync(
        IMediator mediator,
        ILogger<EnrollmentTools> logger,
        string memberId,
        string fromPlanId,
        string toPlanId,
        DateOnly effectiveDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        ArgumentException.ThrowIfNullOrWhiteSpace(fromPlanId);
        ArgumentException.ThrowIfNullOrWhiteSpace(toPlanId);

        logger.LogInformation("ChangePlan for Member {MemberId}, From={From}, To={To}",
            RedactMemberId(memberId), fromPlanId, toPlanId);

        var request = new ChangePlanRequest(memberId, fromPlanId, toPlanId, effectiveDate);
        return await mediator.Send(new ChangePlanCommand(request));
    }

    [McpServerTool, Description("Adds a dependent to a member's enrollment.")]
    public static async Task<DependentResultDto> AddDependentAsync(
        IMediator mediator,
        ILogger<EnrollmentTools> logger,
        string memberId,
        string firstName,
        string lastName,
        string relationship,
        DateOnly dob,
        string? gender = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(relationship);

        logger.LogInformation("AddDependent for Member {MemberId}, Dependent={First} {Last}",
            RedactMemberId(memberId), firstName, lastName);

        var request = new AddDependentRequest(memberId, firstName, lastName, relationship, dob, gender);
        return await mediator.Send(new AddDependentCommand(request));
    }

    [McpServerTool, Description("Gets the current status and details of an enrollment transaction.")]
    public static async Task<EnrollmentStatusDto> GetEnrollmentStatusAsync(
        EnrollmentDbContext db,
        ILogger<EnrollmentTools> logger,
        string enrollmentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(enrollmentId);

        logger.LogInformation("GetEnrollmentStatus for EnrollmentId={EnrollmentId}", enrollmentId);

        var transaction = await db.EnrollmentTransactions
            .AsNoTracking()
            .Include(t => t.PlanElections)
            .FirstOrDefaultAsync(t => t.EnrollmentId == enrollmentId);

        if (transaction is null)
            throw new McpException($"Enrollment transaction '{enrollmentId}' not found.");

        var elections = transaction.PlanElections
            .Select(e => new PlanElectionDto(e.ElectionId, e.PlanId, e.Tier, e.EffectiveDate, e.Action))
            .ToList();

        return new EnrollmentStatusDto(
            transaction.EnrollmentId,
            RedactMemberId(transaction.MemberId),
            transaction.TransactionType,
            transaction.Status,
            transaction.EffectiveDate,
            transaction.SubmittedChannel,
            transaction.CreatedUtc,
            transaction.CompletedUtc,
            transaction.ErrorMessage,
            elections);
    }
}
