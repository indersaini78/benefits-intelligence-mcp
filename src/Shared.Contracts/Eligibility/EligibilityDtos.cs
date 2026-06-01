namespace Shared.Contracts.Eligibility;

public sealed record CoverageDto(
    string CoverageId,
    string PlanId,
    string PlanName,
    string Tier,
    DateOnly EffectiveDate,
    DateOnly? TermDate,
    string Status,
    string? PrimaryCareProvNpi);

public sealed record AccumulatorDto(
    long AccumulatorId,
    string PlanId,
    int PlanYear,
    string AccumulatorType,
    decimal LimitAmount,
    decimal AppliedAmount,
    decimal RemainingAmount,
    DateTime LastUpdatedUtc);

public sealed record PlanSummaryDto(
    string PlanId,
    string PlanName,
    string LineOfBusiness,
    string CarrierId,
    int PlanYear,
    string? NetworkType,
    decimal? DeductibleInd,
    decimal? DeductibleFam,
    decimal? OopMaxInd,
    decimal? OopMaxFam,
    decimal? Copay,
    decimal? Coinsurance);
