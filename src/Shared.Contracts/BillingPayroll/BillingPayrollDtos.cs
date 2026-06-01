namespace Shared.Contracts.BillingPayroll;

// Billing DTOs
public sealed record InvoiceDto(
    string InvoiceId,
    string GroupPolicyId,
    string? MemberId,
    DateOnly BillingPeriodStart,
    DateOnly BillingPeriodEnd,
    DateOnly InvoiceDate,
    DateOnly DueDate,
    decimal TotalAmount,
    decimal BalanceDue,
    string Status,
    List<InvoiceLineDto> Lines);

public sealed record InvoiceLineDto(
    long InvoiceLineId,
    string InvoiceId,
    string PlanId,
    string LineDescription,
    decimal PremiumAmount,
    decimal EmployerPortion,
    decimal EmployeePortion);

public sealed record BalanceDto(
    string MemberId,
    decimal TotalBalanceDue,
    int OpenInvoiceCount);

public sealed record PaymentDto(
    string PaymentId,
    string? InvoiceId,
    decimal Amount,
    string PaymentMethod,
    DateOnly PaymentDate,
    string ConfirmationNumber,
    string Status);

public sealed record InvoiceLineExplanationDto(
    long InvoiceLineId,
    string PlanId,
    string PlanName,
    string LineDescription,
    decimal PremiumAmount,
    decimal EmployerPortion,
    decimal EmployeePortion,
    PremiumRateDto? MatchedRate);

public sealed record PremiumRateDto(
    string RateId,
    string Tier,
    string? AgeBand,
    string? SalaryBand,
    decimal MonthlyPremium,
    DateOnly EffectiveDate,
    DateOnly? EndDate);

// Payroll DTOs
public sealed record PayrollDeductionDto(
    string DeductionId,
    string PlanId,
    DateOnly PayPeriodStart,
    DateOnly PayPeriodEnd,
    decimal PreTaxAmount,
    decimal PostTaxAmount,
    string Status);

public sealed record PaycheckDeltaDto(
    string MemberId,
    DateOnly CurrentPayDate,
    DateOnly PreviousPayDate,
    List<PlanDeductionDiffDto> Diffs);

public sealed record PlanDeductionDiffDto(
    string PlanId,
    decimal PreviousTotal,
    decimal CurrentTotal,
    decimal Difference);
